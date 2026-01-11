using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Foundation.Web.Utility
{
    public static class HttpRequestLogger
    {
        public static async Task LogFullRequestAsync(ILogger logger, HttpRequest request, LogLevel logLevel = LogLevel.Error)
        {
            // Enable buffering so we can read the body multiple times if needed
            request.EnableBuffering();

            // Build the full request message
            var requestMessage = new StringBuilder();

            // Method and URL
            requestMessage.AppendLine($"{request.Method} {request.Path}{request.QueryString} HTTP/1.1");
            requestMessage.AppendLine($"Host: {request.Host}");

            // Headers
            foreach (var header in request.Headers)
            {
                requestMessage.AppendLine($"{header.Key}: {header.Value}");
            }

            // Body
            string requestBody = string.Empty;
            if (request.Body.CanRead)
            {
                using (var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    // Reset the stream position so downstream middleware/controller can still read it
                    request.Body.Position = 0;
                }
                requestMessage.AppendLine();
                requestMessage.AppendLine(requestBody);
            }

            // Log the full request
            logger.Log(logLevel, "Full HTTP Request Details:\n{RequestMessage}", requestMessage.ToString());
        }
   }
}