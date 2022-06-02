using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Siganberg.SerilogElasticSearch.Middleware
{
    public class RequestLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDiagnosticContext _diagnosticContext;
        private readonly IConfiguration _configuration;


        public RequestLogMiddleware(RequestDelegate next, IDiagnosticContext diagnosticContext, IConfiguration configuration)
        {
            _next = next;
            _diagnosticContext = diagnosticContext;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var requestLoggingOptions = httpContext.RequestServices.GetService<IRequestLoggingOptions>();   
            if (requestLoggingOptions?.IncludeRequestWhen(httpContext) ?? true)
            {
                var body = await ReadRequestBody(httpContext.Request);
                _diagnosticContext.Set("Path", httpContext.Request.Path);
                _diagnosticContext.Set("QueryString", httpContext.Request.QueryString);
                var includeRequestHeaders = _configuration["Serilog:RequestLoggingOptions:IncludeRequestHeaders"];
                if (string.Equals(includeRequestHeaders, "true", StringComparison.OrdinalIgnoreCase))
                    _diagnosticContext.Set("RequestHeaders", FormatHeader(httpContext.Request.Headers, _configuration));
                _diagnosticContext.Set("ContentType", httpContext.Request.ContentType);
                _diagnosticContext.Set("ContentLength", httpContext.Request.ContentLength);
                _diagnosticContext.Set("RequestBody", body);
                await _next(httpContext);
            }
        }

        private static async Task<string> ReadRequestBody(HttpRequest request)
        {
            if (request.ContentLength == 0) return null;
            try
            {
                if (!request.Body.CanSeek) return null; 
                request.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(request.Body);
                var bodyAsText = await reader.ReadToEndAsync();
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