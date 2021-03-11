using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog.AspNetCore;

namespace Siganberg.SerilogElasticSearch.Utilities
{
    internal static class EnrichHelper
    {
        internal static void AddDiagnosticContext(RequestLoggingOptions options, IConfiguration config)
        {
            options.EnrichDiagnosticContext = async (diagnosticContext, httpContext) =>
            {
                if (httpContext == null) return; 
                diagnosticContext.Set("RequestBody", await ReadRequestBody(httpContext.Request));
                diagnosticContext.Set("Path", httpContext.Request?.Path ?? "");
                diagnosticContext.Set("QueryString", httpContext.Request?.QueryString);

                var includeRequestHeaders = config["Serilog:RequestLoggingOptions:IncludeRequestHeaders"];
                if (string.IsNullOrWhiteSpace(includeRequestHeaders) || includeRequestHeaders.ToLower() == "true")
                    diagnosticContext.Set("RequestHeaders", FormatHeader(httpContext.Request?.Headers, config));
            };
        }

        private static async Task<string> ReadRequestBody(HttpRequest request)
        {
            if (request == null) return string.Empty;
            request.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(request.Body);
            var bodyAsText = await reader.ReadToEndAsync();
            return bodyAsText;
        }

        private static string FormatHeader(IHeaderDictionary requestHeaders, IConfiguration config)
        {
            if (requestHeaders == null) return string.Empty; 
            
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
            }).ToDictionary(a => a.Key, a => a.Value);

            return String.Join("\\n", result);
        }
    }
}