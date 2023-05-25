using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Siganberg.SerilogElasticSearch.Handlers;

namespace Siganberg.SerilogElasticSearch.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddHttpClientTraceId(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<RequestIdMessageHandler>();
            services.AddHttpClient("", _ => { })
                .AddHttpMessageHandler<RequestIdMessageHandler>();

            return services;
        }
    }
}