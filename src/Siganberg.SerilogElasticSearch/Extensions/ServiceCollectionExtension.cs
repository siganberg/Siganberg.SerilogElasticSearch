using Microsoft.Extensions.DependencyInjection;
using Siganberg.SerilogElasticSearch.Handlers;

namespace Siganberg.SerilogElasticSearch.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddRequestLogging(this IServiceCollection services)
        {
            services.AddHttpContextAccessor()
                .AddTransient<RequestIdMessageHandler>();

            services
                .AddHttpClient("", client => { })
                .AddHttpMessageHandler<RequestIdMessageHandler>();

            return services;
        }
    }
}