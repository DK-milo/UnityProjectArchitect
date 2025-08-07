using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityProjectArchitect.AI.Services
{
    /// <summary>
    /// Interface for HTTP client operations
    /// Unity implementation will use UnityWebRequest, standalone uses HttpClient
    /// </summary>
    public interface IHttpClientWrapper
    {
        Task<string> PostAsync(string url, string content, Dictionary<string, string> headers, int timeoutSeconds = 60);
        Task<HttpResponse> PostWithHeadersAsync(string url, string content, Dictionary<string, string> headers, int timeoutSeconds = 60);
    }

    public class HttpResponse
    {
        public bool IsSuccess { get; set; }
        public string Content { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Standard HttpClient implementation for standalone scenarios
    /// </summary>
    public class StandardHttpClient : IHttpClientWrapper, IDisposable
    {
        private readonly System.Net.Http.HttpClient _httpClient;

        public StandardHttpClient()
        {
            _httpClient = new System.Net.Http.HttpClient();
        }

        public async Task<string> PostAsync(string url, string content, Dictionary<string, string> headers, int timeoutSeconds = 60)
        {
            HttpResponse response = await PostWithHeadersAsync(url, content, headers, timeoutSeconds);
            return response.IsSuccess ? response.Content : throw new Exception(response.ErrorMessage);
        }

        public async Task<HttpResponse> PostWithHeadersAsync(string url, string content, Dictionary<string, string> headers, int timeoutSeconds = 60)
        {
            try
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

                using System.Net.Http.StringContent stringContent = new System.Net.Http.StringContent(content, System.Text.Encoding.UTF8, "application/json");
                
                foreach (KeyValuePair<string, string> header in headers)
                {
                    if (header.Key == "Content-Type") continue; // Already set by StringContent
                    stringContent.Headers.Add(header.Key, header.Value);
                }

                using System.Net.Http.HttpResponseMessage response = await _httpClient.PostAsync(url, stringContent);
                
                Dictionary<string, string> responseHeaders = new Dictionary<string, string>();
                foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
                {
                    responseHeaders[header.Key] = string.Join(", ", header.Value);
                }

                return new HttpResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    Content = await response.Content.ReadAsStringAsync(),
                    StatusCode = (int)response.StatusCode,
                    Headers = responseHeaders,
                    ErrorMessage = response.IsSuccessStatusCode ? string.Empty : response.ReasonPhrase ?? "Unknown error"
                };
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    IsSuccess = false,
                    Content = string.Empty,
                    StatusCode = 0,
                    Headers = new Dictionary<string, string>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}