using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.AspNetCore;

namespace Siganberg.SerilogElasticSearch.Utilities
{
    internal static class EnrichHelper
    {
        internal static void AddDiagnosticContext(RequestLoggingOptions options, IConfiguration config)
        {
            async void OptionsEnrichDiagnosticContext(IDiagnosticContext diagnosticContext, HttpContext httpContext)
            {
                if (httpContext?.Request == null) return;
                diagnosticContext.Set("Path", httpContext.Request.Path);
                diagnosticContext.Set("QueryString", httpContext.Request.QueryString);
                var includeRequestHeaders = config["Serilog:RequestLoggingOptions:IncludeRequestHeaders"];
                if (string.Equals(includeRequestHeaders, "true", StringComparison.OrdinalIgnoreCase))
                    diagnosticContext.Set("RequestHeaders", FormatHeader(httpContext.Request.Headers, config));
                diagnosticContext.Set("ContentType", httpContext.Request.ContentType);
                diagnosticContext.Set("ContentLength", httpContext.Request.ContentLength);
                diagnosticContext.Set("RequestBody", await ReadRequestBody(httpContext.Request));
            }
            options.EnrichDiagnosticContext = OptionsEnrichDiagnosticContext;
        }

        private static async Task<string> ReadRequestBody(HttpRequest request)
        {
            // Exit early if the caller claims there's no body content (e.g. a GET request).
            // As of .NET 5, checking the actual Body.Length is sporadically unreliable.
            if (request.ContentLength == 0) return null;

            try
            {
                request.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(request.Body, leaveOpen: true);
                var bodyAsText = await reader.ReadToEndAsync();
                request.Body.Seek(0, SeekOrigin.Begin);
                return bodyAsText;
            }
            catch // This could happen if Body is actually empty.
            {
                return null;
            }
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

            return string.Join("\\n", result);
        }
    }
}