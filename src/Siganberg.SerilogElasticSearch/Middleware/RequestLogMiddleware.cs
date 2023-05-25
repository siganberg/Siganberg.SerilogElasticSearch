using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Siganberg.SerilogElasticSearch.Settings;

namespace Siganberg.SerilogElasticSearch.Middleware
{
    public class RequestLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDiagnosticContext _diagnosticContext;
        private readonly SerilogSettings _settings;

        public RequestLogMiddleware(RequestDelegate next, IDiagnosticContext diagnosticContext, IConfiguration configuration)
        {
            _next = next;
            _diagnosticContext = diagnosticContext;
            _settings = configuration.GetSection(SerilogSettings.KeyName).Get<SerilogSettings>();

        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var requestLoggingInterceptor = httpContext.RequestServices.GetService<IRequestLoggingInterceptor>() 
                                        ?? DefaultRequestLoggingInterceptor.Instance;

            if (requestLoggingInterceptor?.IncludeRequestWhen(httpContext) ?? true)
            {
                var body = await ReadRequestBody(httpContext.Request);
                _diagnosticContext.Set("Path", httpContext.Request.Path);
                _diagnosticContext.Set("QueryString", httpContext.Request.QueryString);
                var includeRequestHeaders = _settings.RequestLoggingOptions.IncludeRequestHeaders;
                if (includeRequestHeaders)
                    _diagnosticContext.Set("RequestHeaders", FormatHeader(httpContext.Request.Headers));
                _diagnosticContext.Set("ContentType", httpContext.Request.ContentType);
                _diagnosticContext.Set("ContentLength", httpContext.Request.ContentLength);
                _diagnosticContext.Set("RequestBody", body);
            }
            await _next(httpContext);
        }

        private static async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(request.Body, leaveOpen:true);
            var bodyAsText = await reader.ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin);
            return bodyAsText;
          
        }
        private  string FormatHeader(IHeaderDictionary requestHeaders)
        {
            if (requestHeaders == null) return string.Empty;

            var exclusion = _settings.RequestLoggingOptions.ExcludeHeaderNames;

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