using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Siganberg.SerilogElasticSearch.V2.Middleware;
using Siganberg.SerilogElasticSearch.V2.Utilities;

namespace Siganberg.SerilogElasticSearch.V2.Extensions
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder, Func<HttpContext, bool> func = null)
        {
            builder.Use((con, next) =>
            {
                con.Request.EnableBuffering();
                return next();
            });

            var context = builder.ApplicationServices.GetService<IHttpContextAccessor>();
            var requestLoggingRules = builder.ApplicationServices.GetService<IRequestLoggingOptions>();
            var config = builder.ApplicationServices.GetService<IConfiguration>();

            StaticHttpContextAccessor.Configure(context);


            builder.UseMiddleware<CorrelationIdMiddleWare>()
                .UseSerilogRequestLogging( options =>
                {
                    options.GetLevel = (ctx, _, ex) => EvaluateExclusionRules(ctx, ex, func, requestLoggingRules);
                    EnrichHelper.AddDiagnosticContext(options, config);
                });

            if (config["Serilog:RequestLoggingOptions:IncludeResponseBody"]?.ToLower() == "true")
                builder.UseMiddleware<ResponseLoggerMiddleware>();

            return builder;
        }


        private static LogEventLevel EvaluateExclusionRules(HttpContext ctx, Exception ex, Func<HttpContext, bool> func = null, IRequestLoggingOptions options = null)
        {
            if (ex != null) return LogEventLevel.Error;

            if (ctx.Response.StatusCode >= 500) return LogEventLevel.Error;

            var result = true;

            if (func != null)
                result = func.Invoke(ctx);
            else if (options != null)
                result = options.IncludeRequestWhen(ctx);

            if (result == false) return LogEventLevel.Verbose;

            return LogEventLevel.Information;
        }
    }
}