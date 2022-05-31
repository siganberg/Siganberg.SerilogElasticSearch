using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Siganberg.SerilogElasticSearch.Middleware
{
    public class ResponseLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDiagnosticContext _diagnosticContext;

        public ResponseLoggerMiddleware(RequestDelegate next, IDiagnosticContext diagnosticContext)
        {
            _next = next;
            _diagnosticContext = diagnosticContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;
                await _next(context);
                var responseBodyPayload = await ReadResponseBody(context.Response);
                _diagnosticContext.Set("ResponseBody", responseBodyPayload);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            string responseBody = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return responseBody;
        }
    }
}