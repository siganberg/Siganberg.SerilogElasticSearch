using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Siganberg.SerilogElasticSearch.Formatter;
using Siganberg.SerilogElasticSearch.Settings;

namespace Siganberg.SerilogElasticSearch.Extensions;

public static class WebApplicationBuilderExtensions
{
    private static readonly Dictionary<string, string> DefaultMapping =  new()
    {
        {"SourceContext", "callsite"},
        {"RequestMethod", "method"},
        {"Path", "path"},
        {"QueryString", "queryString"},
        {"StatusCode", "responseStatus"},
        {"Elapsed", "durationMs"},
        {"RequestHeaders", "requestHeaders"},
        {"RequestBody", "requestBody"},
        {"ResponseBody", "responseBody"},
        {"ContentType", "contentType"},
        {"ContentLength", "contentLength"}
    };

    public static void UseSerilog(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
        {
            
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
            var settings = hostingContext.Configuration.GetSection(SerilogSettings.KeyName).Get<SerilogSettings>();

            settings.ElasticMappings ??= DefaultMapping;
            
            if (settings.UseDeveloperView)
                loggerConfiguration.WriteTo.Console();
            else
                loggerConfiguration.WriteTo.Console(new ElasticSearchFormatter(settings.ElasticMappings));
            loggerConfiguration.Enrich.FromLogContext();
            loggerConfiguration.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);
        });
    }
}