using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Siganberg.SirilogElasticSearch.Extensions;

namespace Siganberg.SirilogElasticSearch.SampleApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddHttpContextAccessor();
            services.AddSingleton<IRequestLoggingRules, RequestLoggingRules>();
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