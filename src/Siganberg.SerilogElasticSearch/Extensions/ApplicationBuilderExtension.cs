using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Siganberg.SerilogElasticSearch.Middleware;
using Siganberg.SerilogElasticSearch.Utilities;

namespace Siganberg.SerilogElasticSearch.Extensions
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder, Func<HttpContext, bool> func = null)
        {
            var context = builder.ApplicationServices.GetService<IHttpContextAccessor>();
            var requestLoggingRules = builder.ApplicationServices.GetService<IRequestLoggingOptions>();
            var config = builder.ApplicationServices.GetRequiredService<IConfiguration>();
            
            builder.Use((con, next) =>
            {
                if (requestLoggingRules?.IncludeRequestWhen(con) ??  true)
                    con.Request.EnableBuffering();
                return next();
            });
            
            StaticHttpContextAccessor.Configure(context);

            builder.UseMiddleware<CorrelationIdMiddleWare>()
                .UseSerilogRequestLogging( options =>
                {
                    options.GetLevel = (ctx, _, ex) => EvaluateExclusionRules(ctx, ex, func, requestLoggingRules);
                });

            if (config["Serilog:RequestLoggingOptions:IncludeResponseBody"]?.ToLower() == "true")
                builder.UseMiddleware<ResponseLoggerMiddleware>();

            builder.UseMiddleware<RequestLogMiddleware>();
            
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