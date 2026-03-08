using AuthZen.AspNetCore.AuthZen.AspNetCore.Filters;
using AuthZen.AspNetCore.AuthZen.AspNetCore.Service;
using AuthZen.AspNetCore.AuthZen.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Options = AuthZen.AspNetCore.AuthZen.AspNetCore.Configuration.Options;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFgaAuthorization(this IServiceCollection services, Action<Options> configure)
        {
            // Configure options
            services.Configure(configure);

            // Configure HttpClient with BaseAddress from Options
            services.AddHttpClient<IAuthorizationService, AuthorizationService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<Options>>().Value;

                if (string.IsNullOrWhiteSpace(options.Url))
                    throw new InvalidOperationException("AuthorizationService BaseUrl must be configured.");

                client.BaseAddress = new Uri(options.Url); // Set the BaseAddress properly
            });

            // Register the AuthorizationFilter
            services.AddScoped<AuthorizationFilter>();

            return services;
        }
    }
}