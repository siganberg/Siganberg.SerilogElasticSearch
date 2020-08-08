using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Siganberg.SirilogElasticSearch.Middleware;
using Siganberg.SirilogElasticSearch.Utilities;

namespace Siganberg.SirilogElasticSearch.Extensions
{
    /// <summary>
    ///
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ApplicationBuilderExtension
    {
        private static readonly List<string> ExcludedPath = new List<string>
        {
            "healthz",
            "swagger"
        };

        /// <summary>
        ///
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            StaticHttpContextAccessor.Configure(builder.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            return builder
                .UseMiddleware<CorrelationIdMiddleWare>()
                .UseSerilogRequestLogging(options =>
                {
                    options.GetLevel = ExcludePaths;
                    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                    {
                        httpContext.Request.EnableBuffering();
                        httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                        var buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength)];
                        httpContext.Request.Body.Read(buffer, 0, buffer.Length);
                        var requestBody = Encoding.UTF8.GetString(buffer);

                        diagnosticContext.Set("QueryString", httpContext.Request.QueryString);
                        diagnosticContext.Set("RequestHeaders", FormatHeader(httpContext.Request.Headers));
                        diagnosticContext.Set("RequestBody", requestBody);
                    };
                });
        }

        private static string FormatHeader(IHeaderDictionary requestHeaders)
        {
            return string.Join("\\n", requestHeaders);
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="_"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static LogEventLevel ExcludePaths(HttpContext ctx, double _, Exception ex)
        {
            if (ex != null) return LogEventLevel.Error;

            if (ctx.Response.StatusCode >= 500) return LogEventLevel.Error;

            if (ExcludedPath.Any(a => ctx.Request.Path.ToString().Contains(a)))
                return LogEventLevel.Verbose;

            return LogEventLevel.Information;
        }
    }
}