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
            services.Configure(configure);

            services.AddHttpClient<IAuthorizationService, AuthorizationService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<Options>>().Value;

                if (string.IsNullOrWhiteSpace(options.Url))
                    throw new InvalidOperationException("AuthorizationService BaseUrl must be configured.");

                client.BaseAddress = new Uri(options.Url);
                client.Timeout = TimeSpan.FromSeconds(5);
            });

            services.AddScoped<AuthorizationFilter>();

            return services;
        }
    }
}