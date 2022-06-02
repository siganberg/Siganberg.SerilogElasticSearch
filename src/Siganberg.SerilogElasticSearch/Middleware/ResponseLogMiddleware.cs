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

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var originalBodyStream = httpContext.Response.Body;
            await using var responseBody = new MemoryStream();
            httpContext.Response.Body = responseBody;
            await _next(httpContext);   
            var responseBodyPayload = await ReadResponseBody(httpContext.Response);
            _diagnosticContext.Set("ResponseBody", responseBodyPayload);
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            using var stream = new StreamReader(response.Body, leaveOpen:true);
            var responseBody = await stream.ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return responseBody;
        }
       
    }
}