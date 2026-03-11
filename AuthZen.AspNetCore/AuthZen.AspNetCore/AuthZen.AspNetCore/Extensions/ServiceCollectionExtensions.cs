using AuthZen.AspNetCore.AuthZen.AspNetCore.Configuration;
using AuthZen.AspNetCore.AuthZen.Contracts;
using AuthZen.AspNetCore.Filters;
using AuthZen.AspNetCore.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AuthZen.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthZenAuthorization(this IServiceCollection services, Action<AuthZenOptions> configure)
        {
            services.Configure(configure);

            services.AddHttpClient<IAuthorizationService, AuthorizationService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<AuthZenOptions>>().Value;
                if (string.IsNullOrWhiteSpace(options.Url))
                    throw new InvalidOperationException("AuthZenOptions.Url must be configured.");

                client.BaseAddress = new Uri(options.Url);
                client.Timeout = TimeSpan.FromSeconds(5);
            });

            services.AddScoped<AuthorizationFilter>();

            return services;
        }
    }
}