using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.AI.Services
{
    public class APIKeyManager
    {
        private const string KEY_PREFIX = "UnityProjectArchitect_";
        private const string CLAUDE_API_KEY = KEY_PREFIX + "ClaudeAPIKey";
        private const string ENCRYPTION_KEY = KEY_PREFIX + "EncryptionKey";
        private const string KEY_VALIDATION_HASH = KEY_PREFIX + "KeyValidationHash";

        private static APIKeyManager? _instance;
        private static readonly object _lock = new object();
        private readonly ISettingsProvider _settingsProvider;
        private readonly ILogger _logger;

        public static APIKeyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new APIKeyManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private APIKeyManager() : this(new InMemorySettingsProvider(), new ConsoleLogger())
        {
        }

        public APIKeyManager(ISettingsProvider settingsProvider, ILogger logger)
        {
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            InitializeEncryption();
        }

        public bool HasClaudeAPIKey()
        {
            try
            {
                string encryptedKey = GetEncryptedValue(CLAUDE_API_KEY);
                string validationHash = GetEncryptedValue(KEY_VALIDATION_HASH);
                
                return !string.IsNullOrEmpty(encryptedKey) && 
                       !string.IsNullOrEmpty(validationHash) &&
                       ValidateKeyIntegrity(encryptedKey, validationHash);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking Claude API key: {ex.Message}");
                return false;
            }
        }

        public string GetClaudeAPIKey()
        {
            try
            {
                string encryptedKey = GetEncryptedValue(CLAUDE_API_KEY);
                if (string.IsNullOrEmpty(encryptedKey))
                {
                    return null;
                }

                // Check if the stored value is a valid base64 string
                // If not, it might be a legacy plain text key that needs to be migrated
                if (!IsValidBase64String(encryptedKey))
                {
                    _logger.LogWarning("Found legacy API key format. Attempting to migrate to encrypted format.");
                    
                    // Try to validate if it's a valid API key in plain text
                    var validation = ValidateAPIKey(encryptedKey);
                    if (validation.IsValid)
                    {
                        // Migrate the plain text key to encrypted format
                        var migrationResult = SetClaudeAPIKey(encryptedKey);
                        if (migrationResult.IsValid)
                        {
                            _logger.Log("Successfully migrated API key to encrypted format.");
                            return encryptedKey;
                        }
                    }
                    
                    // If migration fails, clear the invalid key
                    _logger.LogWarning("Invalid legacy API key found. Clearing stored value.");
                    ClearClaudeAPIKey();
                    return null;
                }

                string decryptedKey = Decrypt(encryptedKey);
                
                if (string.IsNullOrEmpty(decryptedKey))
                {
                    _logger.LogWarning("Failed to decrypt Claude API key");
                    return null;
                }

                string validationHash = GetEncryptedValue(KEY_VALIDATION_HASH);
                if (!ValidateKeyIntegrity(encryptedKey, validationHash))
                {
                    _logger.LogWarning("Claude API key integrity validation failed");
                    return null;
                }

                return decryptedKey;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving Claude API key: {ex.Message}");
                
                // If there's a decryption error, try to clear the corrupted key
                try
                {
                    _logger.LogWarning("Clearing potentially corrupted API key due to decryption error.");
                    ClearClaudeAPIKey();
                }
                catch (Exception clearEx)
                {
                    _logger.LogError($"Failed to clear corrupted API key: {clearEx.Message}");
                }
                
                return null;
            }
        }

        public ValidationResult SetClaudeAPIKey(string apiKey)
        {
            ValidationResult validation = ValidateAPIKey(apiKey);
            if (!validation.IsValid)
            {
                return validation;
            }

            try
            {
                string encryptedKey = Encrypt(apiKey);
                string validationHash = GenerateValidationHash(encryptedKey);

                SetEncryptedValue(CLAUDE_API_KEY, encryptedKey);
                SetEncryptedValue(KEY_VALIDATION_HASH, validationHash);

                _logger.Log("Claude API key successfully stored and encrypted");
                return ValidationResult.Success("API key stored successfully");
            }
            catch (Exception ex)
            {
                string error = $"Failed to store Claude API key: {ex.Message}";
                _logger.LogError(error);
                return ValidationResult.Failure(error);
            }
        }

        public void ClearClaudeAPIKey()
        {
            try
            {
                ClearEncryptedValue(CLAUDE_API_KEY);
                ClearEncryptedValue(KEY_VALIDATION_HASH);
                _logger.Log("Claude API key cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing Claude API key: {ex.Message}");
            }
        }

        public ValidationResult ValidateAPIKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return ValidationResult.Failure("API key cannot be empty");
            }

            if (apiKey.Length < 10)
            {
                return ValidationResult.Failure("API key appears to be too short");
            }

            if (!apiKey.StartsWith("sk-"))
            {
                return ValidationResult.Failure("Claude API key should start with 'sk-'");
            }

            if (apiKey.Contains(" ") || apiKey.Contains("\t") || apiKey.Contains("\n"))
            {
                return ValidationResult.Failure("API key contains invalid whitespace characters");
            }

            if (apiKey.Length > 200)
            {
                return ValidationResult.Failure("API key appears to be too long");
            }

            return ValidationResult.Success("API key format is valid");
        }

        public bool IsAPIKeyExpired()
        {
            return false;
        }

        public DateTime? GetKeyCreationTime()
        {
            try
            {
                string timestampKey = KEY_PREFIX + "KeyCreationTime";
                string timestampStr = GetEncryptedValue(timestampKey);
                
                if (string.IsNullOrEmpty(timestampStr))
                {
                    return null;
                }

                if (long.TryParse(timestampStr, out long ticks))
                {
                    return new DateTime(ticks);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting key creation time: {ex.Message}");
                return null;
            }
        }

        private void InitializeEncryption()
        {
            if (!HasEncryptionKey())
            {
                GenerateNewEncryptionKey();
            }
        }

        private bool HasEncryptionKey()
        {
            string key = GetEncryptedValue(ENCRYPTION_KEY);
            return !string.IsNullOrEmpty(key);
        }

        private void GenerateNewEncryptionKey()
        {
            try
            {
                byte[] keyBytes = new byte[32];
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(keyBytes);
                }

                string base64Key = Convert.ToBase64String(keyBytes);
                SetEncryptedValue(ENCRYPTION_KEY, base64Key);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to generate encryption key: {ex.Message}");
                throw;
            }
        }

        private byte[] GetEncryptionKey()
        {
            try
            {
                string base64Key = GetEncryptedValue(ENCRYPTION_KEY);
                if (string.IsNullOrEmpty(base64Key))
                {
                    throw new InvalidOperationException("Encryption key not found");
                }

                return Convert.FromBase64String(base64Key);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to retrieve encryption key: {ex.Message}");
                throw;
            }
        }

        private string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            try
            {
                byte[] keyBytes = GetEncryptionKey();
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.GenerateIV();

                    using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        byte[] encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
                        
                        byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                        Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                        Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Encryption failed: {ex.Message}");
                throw;
            }
        }

        private string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return string.Empty;
            }

            try
            {
                byte[] keyBytes = GetEncryptionKey();
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    
                    byte[] iv = new byte[aes.IV.Length];
                    Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    byte[] encryptedData = new byte[cipherBytes.Length - iv.Length];
                    Array.Copy(cipherBytes, iv.Length, encryptedData, 0, encryptedData.Length);

                    using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Decryption failed: {ex.Message}");
                throw;
            }
        }

        private string GenerateValidationHash(string data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }

        private bool IsValidBase64String(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            try
            {
                // Check if string length is multiple of 4 (required for base64)
                if (value.Length % 4 != 0)
                {
                    return false;
                }

                // Try to convert from base64 - this will throw if invalid
                Convert.FromBase64String(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateKeyIntegrity(string encryptedKey, string expectedHash)
        {
            if (string.IsNullOrEmpty(encryptedKey) || string.IsNullOrEmpty(expectedHash))
            {
                return false;
            }

            string actualHash = GenerateValidationHash(encryptedKey);
            return actualHash.Equals(expectedHash, StringComparison.Ordinal);
        }

        private string GetEncryptedValue(string key)
        {
            return _settingsProvider.GetEncryptedValue(key, string.Empty);
        }

        private void SetEncryptedValue(string key, string value)
        {
            _settingsProvider.SetEncryptedValue(key, value);
        }

        private void ClearEncryptedValue(string key)
        {
            _settingsProvider.ClearEncryptedValue(key);
        }

        public void ClearAllKeys()
        {
            try
            {
                ClearEncryptedValue(CLAUDE_API_KEY);
                ClearEncryptedValue(KEY_VALIDATION_HASH);
                ClearEncryptedValue(ENCRYPTION_KEY);
                ClearEncryptedValue(KEY_PREFIX + "KeyCreationTime");
                
                _logger.Log("All API keys and encryption data cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing all keys: {ex.Message}");
            }
        }

        public APIKeyStatus GetKeyStatus()
        {
            APIKeyStatus status = new APIKeyStatus();
            
            try
            {
                status.HasKey = HasClaudeAPIKey();
                status.IsValid = status.HasKey;
                status.CreationTime = GetKeyCreationTime();
                status.IsExpired = IsAPIKeyExpired();
                
                if (status.HasKey)
                {
                    string key = GetClaudeAPIKey();
                    status.KeyPreview = !string.IsNullOrEmpty(key) ? 
                        $"sk-...{key.Substring(Math.Max(0, key.Length - 6))}" : 
                        "Invalid key";
                }
                else
                {
                    status.KeyPreview = "No key stored";
                }
            }
            catch (Exception ex)
            {
                status.HasKey = false;
                status.IsValid = false;
                status.ErrorMessage = ex.Message;
                _logger.LogError($"Error getting key status: {ex.Message}");
            }

            return status;
        }
    }

    [Serializable]
    public class APIKeyStatus
    {
        public bool HasKey { get; set; }
        public bool IsValid { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? CreationTime { get; set; }
        public string KeyPreview { get; set; }
        public string ErrorMessage { get; set; }

        public string GetStatusDescription()
        {
            if (!HasKey)
                return "No API key configured";
            
            if (!IsValid)
                return $"Invalid API key: {ErrorMessage}";
            
            if (IsExpired)
                return "API key has expired";
            
            return "API key is valid and ready";
        }

        public bool IsReady => HasKey && IsValid && !IsExpired;
    }
}