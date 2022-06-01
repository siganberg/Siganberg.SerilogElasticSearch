using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
                var syncIoFeature = httpContext.Features.Get<IHttpBodyControlFeature>();
                if (syncIoFeature != null)
                    syncIoFeature.AllowSynchronousIO = true;
                if (httpContext?.Request == null) return;
                diagnosticContext.Set("Path", httpContext.Request.Path);
                diagnosticContext.Set("QueryString", httpContext.Request.QueryString);
                var includeRequestHeaders = config["Serilog:RequestLoggingOptions:IncludeRequestHeaders"];
                if (string.Equals(includeRequestHeaders, "true", StringComparison.OrdinalIgnoreCase))
                    diagnosticContext.Set("RequestHeaders", FormatHeader(httpContext.Request.Headers, config));
                diagnosticContext.Set("ContentType", httpContext.Request.ContentType);
                diagnosticContext.Set("ContentLength", httpContext.Request.ContentLength);
                diagnosticContext.Set("RequestBody",  ReadRequestBody(httpContext.Request));
            };
        }

        private static string ReadRequestBody(HttpRequest request)
        {
            if (request.ContentLength == 0) return null;
            try
            {
                request.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(request.Body, leaveOpen: true);
                var bodyAsText = reader.ReadToEnd();
                request.Body.Seek(0, SeekOrigin.Begin);
                return bodyAsText;
            }
            catch 
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