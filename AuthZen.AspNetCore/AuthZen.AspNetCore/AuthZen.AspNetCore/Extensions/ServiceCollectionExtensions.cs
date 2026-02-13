using AuthZen.AspNetCore.AuthZen.AspNetCore.Configuration;
using AuthZen.AspNetCore.AuthZen.AspNetCore.Http;
using AuthZen.AspNetCore.AuthZen.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFgaAuthorization(this IServiceCollection services, Action<Options> configure)
        {
            services.Configure(configure);
            services.AddHttpClient<IAuthorizationService, AuthorizationServiceHttp>();

            return services;
        }
    }
}
