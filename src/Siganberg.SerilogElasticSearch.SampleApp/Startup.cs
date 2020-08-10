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

            services.AddHttpContextAccessor();
            services.AddSingleton<IRequestLoggingOptions, RequestLoggingOptions>();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRequestLogging();
            app.UseMvc();

        }
    }


}