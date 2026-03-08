using AuthZen.AspNetCore.AuthZen.AspNetCore.Configuration;
using AuthZen.AspNetCore.AuthZen.AspNetCore.Filters;
using AuthZen.AspNetCore.AuthZen.AspNetCore.Service;
using AuthZen.AspNetCore.AuthZen.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFgaAuthorization(this IServiceCollection services, Action<Options> configure)
        {
            services.Configure(configure);
            services.AddHttpClient<IAuthorizationService, AuthorizationService>();
            services.AddScoped<AuthorizationFilter>();

            return services;
        }
    }
}
