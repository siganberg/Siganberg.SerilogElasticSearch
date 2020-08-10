using System.IO;
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
                diagnosticContext.Set("QueryString", httpContext.Request.QueryString);
                diagnosticContext.Set("RequestHeaders", FormatHeader(httpContext.Request.Headers));
            };
        }

        private static string ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = new StreamReader(response.Body).ReadToEnd();
            response.Body.Seek(0, SeekOrigin.Begin);
            return responseBody;
        }

        private static string ReadRequestBody(Stream stream, long? length)
        {
            if (length == null || length == 0) return string.Empty;
            stream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[length.Value];
            stream.Read(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return  Encoding.UTF8.GetString(buffer);
        }

        private static string FormatHeader(IHeaderDictionary requestHeaders)
        {
            return string.Join("\\n", requestHeaders);
        }
    }
}