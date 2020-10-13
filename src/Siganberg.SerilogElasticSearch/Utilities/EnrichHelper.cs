using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog.AspNetCore;

namespace Siganberg.SerilogElasticSearch.Utilities
{
    internal static class EnrichHelper
    {
        internal static void AddDiagnosticContext(RequestLoggingOptions options, IConfiguration config)
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                httpContext.Request.EnableBuffering();
                diagnosticContext.Set("RequestBody", ReadRequestBody(httpContext.Request.Body, httpContext.Request.ContentLength));
                diagnosticContext.Set("Path", httpContext.Request.Path);
                diagnosticContext.Set("QueryString", httpContext.Request.QueryString);

                var includeRequestHeaders = config["Serilog:RequestLoggingOptions:IncludeRequestHeaders"];
                if (string.IsNullOrWhiteSpace(includeRequestHeaders) || includeRequestHeaders.ToLower() == "true")
                    diagnosticContext.Set("RequestHeaders", FormatHeader(httpContext.Request.Headers, config));
            };
        }

        private static string ReadRequestBody(Stream stream, long? length)
        {
            if (length == null || length == 0) return string.Empty;
            stream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[length.Value];
            stream.ReadAsync(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return  Encoding.UTF8.GetString(buffer);
        }

        private static string FormatHeader(IHeaderDictionary requestHeaders, IConfiguration config)
        {
            var exclusion = config.GetSection("Serilog:RequestLoggingOptions:ExcludeHeaderNames")
                ?.Get<string[]>()
                ?.Select(a => a.ToLower())
                .ToList();

            if (exclusion == null || exclusion.Count == 0)
                return string.Join("\\n", requestHeaders);

            var result = requestHeaders.Select(a => new
            {
                a.Key,
                Value = exclusion.Contains(a.Key.ToLower()) ? "<OMITTED>" : a.Value.ToString()
            }).ToDictionary(a => a.Key, a=> a.Value);

            return String.Join("\\n", result);
        }


    }
}