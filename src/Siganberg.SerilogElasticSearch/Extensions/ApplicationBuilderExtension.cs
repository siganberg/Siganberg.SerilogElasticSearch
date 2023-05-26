using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Siganberg.SerilogElasticSearch.Middleware;
using Siganberg.SerilogElasticSearch.Settings;
using Siganberg.SerilogElasticSearch.Utilities;

namespace Siganberg.SerilogElasticSearch.Extensions;

public static class ApplicationBuilderExtension
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder, Func<HttpContext, bool> func = null)
    {
        var context = builder.ApplicationServices.GetService<IHttpContextAccessor>();
        var requestLoggingInterceptor = builder.ApplicationServices.GetService<IRequestLoggingInterceptor>() 
                                        ?? DefaultRequestLoggingInterceptor.Instance;
            
        var configuration = builder.ApplicationServices.GetRequiredService<IConfiguration>();
        var settings = configuration.GetSection(SerilogSettings.KeyName).Get<SerilogSettings>();
        
            
        builder.Use((con, next) =>
        {
            if (requestLoggingInterceptor?.IncludeRequestWhen(con) ??  true)
                con.Request.EnableBuffering();
            return next();
        });
            
        StaticHttpContextAccessor.Configure(context);

        builder.UseMiddleware<CorrelationIdMiddleWare>()
            .UseSerilogRequestLogging( options =>
            {
                options.GetLevel = (ctx, _, ex) => EvaluateExclusionRules(ctx, ex, func, requestLoggingInterceptor);
            });

        if (settings.RequestLoggingOptions.IncludeResponseBody)
            builder.UseMiddleware<ResponseLoggerMiddleware>();

        builder.UseMiddleware<RequestLogMiddleware>();
            
        return builder;
    }


    private static LogEventLevel EvaluateExclusionRules(HttpContext ctx, Exception ex, Func<HttpContext, bool> func = null, IRequestLoggingInterceptor interceptor = null)
    {
        if (ex != null) return LogEventLevel.Error;

        if (ctx.Response.StatusCode >= 500) return LogEventLevel.Error;

        var result = true;

        if (func != null)
            result = func.Invoke(ctx);
        else if (interceptor != null)
            result = interceptor.IncludeRequestWhen(ctx);

        return result == false 
            ? LogEventLevel.Verbose 
            : LogEventLevel.Information;
    }
}