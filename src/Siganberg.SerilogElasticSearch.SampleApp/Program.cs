using System;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Siganberg.SerilogElasticSearch.Formatter;

namespace Siganberg.SerilogElasticSearch.SampleApp
{
    public class Program
    {
        public static void Main()
        {
            var builder = CreateWebHostBuilder(null);
            var host = builder.Build();
            var appName = AppDomain.CurrentDomain.FriendlyName;
            var logger = host.Services.GetService<ILogger<Program>>();
            try
            {
                logger.LogInformation($@"Service {appName} Starting at {DateTime.Now}.");
                logger.LogInformation("Hosting Environment: {env}", builder.GetSetting("Environment"));
                logger.LogInformation("Now listening on: {Host}", host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.First());
                AppDomain.CurrentDomain.ProcessExit += (s, e) => logger.LogInformation($@"Service {appName} ProcessExit by SIGTERM at {DateTime.Now}.");
                host.Run();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);
            builder.UseStartup<Startup>()
                .SuppressStatusMessages(true)
                .ConfigureAppConfiguration(ConfigureConfiguration)
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                        loggerConfiguration.WriteTo.Console();
                    else
                        loggerConfiguration.WriteTo.Console(new ElasticSearchFormatter());
                });

            return builder;
        }

        private static void ConfigureConfiguration(WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            var env = hostingContext.HostingEnvironment;
            config.SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
        }
    }
}