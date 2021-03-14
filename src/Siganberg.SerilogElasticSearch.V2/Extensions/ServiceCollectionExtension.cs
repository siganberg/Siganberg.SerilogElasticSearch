using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Siganberg.SerilogElasticSearch.V2.Handlers;

namespace Siganberg.SerilogElasticSearch.V2.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddRequestLogging(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<RequestIdMessageHandler>();

            services
                .AddHttpClient("", client => { })
                .AddHttpMessageHandler<RequestIdMessageHandler>();

            return services;
        }
    }
}