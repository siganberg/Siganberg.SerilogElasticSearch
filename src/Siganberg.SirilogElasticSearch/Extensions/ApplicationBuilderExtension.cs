using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder, Func< HttpContext, bool> func = null)
        {
            var context = builder.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            var requestLoggingRules = builder.ApplicationServices.GetRequiredService<IRequestLoggingRules>();

            StaticHttpContextAccessor.Configure(context);
            return builder
                .UseMiddleware<CorrelationIdMiddleWare>()
                .UseSerilogRequestLogging(options =>
                {
                    options.GetLevel = (ctx, _, ex) => EvaluateExclusionRules(ctx, ex, func, requestLoggingRules);
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
        private static LogEventLevel EvaluateExclusionRules(HttpContext ctx,  Exception ex, Func<HttpContext, bool> func = null, IRequestLoggingRules rules = null)
        {
            if (ex != null) return LogEventLevel.Error;

            if (ctx.Response.StatusCode >= 500) return LogEventLevel.Error;

            var result = true;

            if (func != null)
                result = func.Invoke(ctx);
            else if (rules != null)
                result = rules.Evaluate(ctx);

            if (result == false) return LogEventLevel.Verbose;

            return LogEventLevel.Information;
        }
    }
}