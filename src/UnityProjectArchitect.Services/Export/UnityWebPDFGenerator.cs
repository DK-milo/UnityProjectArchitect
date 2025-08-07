using System;
using System.IO;
using System.Threading.Tasks;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Services
{
    public class UnityWebPDFGenerator
    {
        public class PDFGenerationOptions
        {
            public string Title { get; set; } = "Documentation";
            public string Author { get; set; } = "Unity Project Architect";
            public bool UseSystemBrowser { get; set; } = true;
            public string OutputFormat { get; set; } = "html"; // html or pdf-ready-html
        }

        public class PDFGenerationResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; } = "";
            public string OutputPath { get; set; } = "";
            public long FileSizeBytes { get; set; }
            public TimeSpan ProcessingTime { get; set; }
            public string Instructions { get; set; } = "";
        }

        public async Task<PDFGenerationResult> GeneratePDFFromHtmlAsync(string htmlContent, string outputPath, PDFGenerationOptions options)
        {
            DateTime startTime = DateTime.Now;
            PDFGenerationResult result = new PDFGenerationResult();

            try
            {
                // Generate print-ready HTML file
                string printReadyHtml = GeneratePrintReadyHtml(htmlContent, options);
                
                // Determine output path - change extension to .html
                string htmlOutputPath = Path.ChangeExtension(outputPath, ".html");
                
                // Ensure output directory exists
                string outputDirectory = Path.GetDirectoryName(htmlOutputPath);
                if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                
                // Write HTML file
                await File.WriteAllTextAsync(htmlOutputPath, printReadyHtml);
                
                result.Success = true;
                result.OutputPath = htmlOutputPath;
                result.FileSizeBytes = new System.IO.FileInfo(htmlOutputPath).Length;
                result.ProcessingTime = DateTime.Now - startTime;
                result.Instructions = GeneratePDFInstructions(htmlOutputPath);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"HTML generation failed: {ex.Message}";
                Debug.LogError($"UnityWebPDFGenerator error: {ex}");
            }

            return result;
        }

        private string GeneratePrintReadyHtml(string htmlContent, PDFGenerationOptions options)
        {
            // Check if htmlContent is already a complete HTML document
            if (htmlContent.TrimStart().StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) ||
                htmlContent.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase))
            {
                // Content is already a complete HTML document, just add our print instructions
                return InsertPrintInstructions(htmlContent, options);
            }
            
            // Content is just HTML fragments, wrap it in a complete document
            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{options.Title}</title>
    <style>
        /* Print-optimized CSS */
        @media print {{
            @page {{
                size: A4;
                margin: 2cm 1.5cm;
            }}
            
            body {{
                margin: 0;
                padding: 0;
                line-height: 1.6;
                color: #000;
                background: white;
            }}
            
            .no-print {{
                display: none !important;
            }}
            
            .page-break {{
                page-break-before: always;
            }}
            
            h1, h2, h3 {{
                page-break-after: avoid;
            }}
            
            pre, blockquote {{
                page-break-inside: avoid;
            }}
            
            img {{
                max-width: 100% !important;
                height: auto !important;
            }}
        }}
        
        /* Screen styles */
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            font-size: 11pt;
            line-height: 1.6;
            color: #333;
            max-width: 210mm;
            margin: 0 auto;
            padding: 20px;
            background: white;
        }}
        
        h1, h2, h3, h4, h5, h6 {{
            color: #2c3e50;
            margin-top: 1.5em;
            margin-bottom: 0.5em;
        }}
        
        h1 {{
            font-size: 24pt;
            border-bottom: 2px solid #3498db;
            padding-bottom: 0.3em;
        }}
        
        h2 {{
            font-size: 18pt;
        }}
        
        h3 {{
            font-size: 14pt;
        }}
        
        p {{
            margin: 1em 0;
            text-align: justify;
        }}
        
        ul, ol {{
            margin: 1em 0;
            padding-left: 2em;
        }}
        
        li {{
            margin: 0.5em 0;
        }}
        
        code {{
            background-color: #f8f9fa;
            padding: 0.2em 0.4em;
            border-radius: 3px;
            font-family: 'Courier New', monospace;
            font-size: 10pt;
        }}
        
        pre {{
            background-color: #f8f9fa;
            border: 1px solid #e9ecef;
            border-radius: 4px;
            padding: 1em;
            margin: 1em 0;
            font-family: 'Courier New', monospace;
            font-size: 9pt;
            overflow-x: auto;
        }}
        
        pre code {{
            background: none;
            padding: 0;
        }}
        
        blockquote {{
            border-left: 4px solid #3498db;
            margin: 1em 0;
            padding-left: 1em;
            color: #666;
        }}
        
        table {{
            border-collapse: collapse;
            width: 100%;
            margin: 1em 0;
        }}
        
        th, td {{
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
        }}
        
        th {{
            background-color: #f8f9fa;
            font-weight: bold;
        }}
        
        .print-instructions {{
            background: #e8f4f8;
            border: 1px solid #3498db;
            border-radius: 4px;
            padding: 15px;
            margin: 20px 0;
        }}
        
        .print-instructions h3 {{
            color: #2980b9;
            margin-top: 0;
        }}
        
        @media print {{
            .print-instructions {{
                display: none;
            }}
        }}
        
        .header {{
            text-align: center;
            margin-bottom: 2em;
            border-bottom: 1px solid #eee;
            padding-bottom: 1em;
        }}
        
        .footer {{
            text-align: center;
            margin-top: 2em;
            border-top: 1px solid #eee;
            padding-top: 1em;
            font-size: 9pt;
            color: #666;
        }}
    </style>
</head>
<body>
    <div class=""print-instructions no-print"">
        <h3>📄 Convert to PDF</h3>
        <p><strong>To generate a PDF from this document:</strong></p>
        <ol>
            <li>Press <kbd>Ctrl+P</kbd> (or <kbd>Cmd+P</kbd> on Mac)</li>
            <li>Choose ""Save as PDF"" as the destination</li>
            <li>Click ""More settings"" and ensure ""Headers and footers"" is unchecked</li>
            <li>Set margins to ""Minimum"" for best results</li>
            <li>Click ""Save"" and choose your desired location</li>
        </ol>
        <p><em>This instruction box will not appear in the printed PDF.</em></p>
    </div>
    
    <div class=""header"">
        <h1>{options.Title}</h1>
        <p>Generated by Unity Project Architect</p>
    </div>
    
    <main>
        {htmlContent}
    </main>
    
    <div class=""footer"">
        <p>Generated on {DateTime.Now:yyyy-MM-dd HH:mm} • Unity Project Architect</p>
    </div>
</body>
</html>";
        }

        private string InsertPrintInstructions(string htmlContent, PDFGenerationOptions options)
        {
            // Insert print instructions after the opening <body> tag
            string printInstructions = @"
    <div class=""print-instructions no-print"">
        <h3>📄 Convert to PDF</h3>
        <p><strong>To generate a PDF from this document:</strong></p>
        <ol>
            <li>Press <kbd>Ctrl+P</kbd> (or <kbd>Cmd+P</kbd> on Mac)</li>
            <li>Choose ""Save as PDF"" as the destination</li>
            <li>Click ""More settings"" and ensure ""Headers and footers"" is unchecked</li>
            <li>Set margins to ""Minimum"" for best results</li>
            <li>Click ""Save"" and choose your desired location</li>
        </ol>
        <p><em>This instruction box will not appear in the printed PDF.</em></p>
    </div>";

            // Find the opening <body> tag and insert instructions after it
            int bodyIndex = htmlContent.IndexOf("<body>", StringComparison.OrdinalIgnoreCase);
            if (bodyIndex == -1)
            {
                bodyIndex = htmlContent.IndexOf("<body ", StringComparison.OrdinalIgnoreCase);
                if (bodyIndex != -1)
                {
                    // Find the end of the opening tag
                    int closeIndex = htmlContent.IndexOf('>', bodyIndex);
                    if (closeIndex != -1)
                    {
                        bodyIndex = closeIndex;
                    }
                }
            }
            
            if (bodyIndex != -1)
            {
                int insertIndex = bodyIndex + htmlContent.Substring(bodyIndex).IndexOf('>') + 1;
                return htmlContent.Insert(insertIndex, printInstructions);
            }
            
            // Fallback: couldn't find body tag, return original content
            return htmlContent;
        }

        private string GeneratePDFInstructions(string htmlFilePath)
        {
            return $@"HTML file created successfully!

📄 To convert to PDF:
1. Open the file in your browser: {htmlFilePath}
2. Press Ctrl+P (or Cmd+P on Mac)
3. Choose ""Save as PDF""
4. Click ""More settings"" and uncheck ""Headers and footers""
5. Set margins to ""Minimum""
6. Save your PDF

The HTML file is optimized for PDF printing and will look professional.";
        }
    }
}