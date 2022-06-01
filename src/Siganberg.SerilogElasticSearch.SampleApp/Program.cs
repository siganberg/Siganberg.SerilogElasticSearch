using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Siganberg.SerilogElasticSearch.Formatter;

namespace Siganberg.SerilogElasticSearch.SampleApp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    //loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
                    //loggerConfiguration.WriteTo.Console();
                    loggerConfiguration.WriteTo.Console(new ElasticSearchFormatter());
                    // if (hostingContext.HostingEnvironment.IsDevelopment())
                    //     loggerConfiguration.WriteTo.Console();
                    // else
                    //     loggerConfiguration.WriteTo.Console(new ElasticSearchFormatter());
                }).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}