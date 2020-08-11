using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Siganberg.SerilogElasticSearch.Extensions;

namespace Siganberg.SerilogElasticSearch.SampleApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddRequestLogging()
                .AddSingleton<IRequestLoggingOptions, RequestLoggingOptions>()
                .AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseRequestLogging();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
        }
    }


}