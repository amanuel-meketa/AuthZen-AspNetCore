using AuthZen.AspNetCore.AuthZen.Contracts;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Http
{
    public sealed class AuthorizationServiceHttp : IAuthorizationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;

        public AuthorizationServiceHttp(HttpClient httpClient, IOptions<Configuration.Options> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _url = options?.Value?.Url ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<AuthZenDecisionResponseDto> CheckAccessAsync(CheckAccessDto check)
        {
            if (check == null)
                throw new ArgumentNullException(nameof(check));

            // Send request to AuthZEN API
            var response = await _httpClient.PostAsJsonAsync(_url, check);

            if (!response.IsSuccessStatusCode)
            {
                // Return deny decision if API call fails
                return new AuthZenDecisionResponseDto
                {
                    Decision = "deny",
                    Reason = $"AuthZEN API call failed with status code {response.StatusCode}"
                };
            }

            // Read AuthZEN-compliant response
            var decision = await response.Content.ReadFromJsonAsync<AuthZenDecisionResponseDto>();

            // Fallback to deny if response is null
            return decision ?? new AuthZenDecisionResponseDto { Decision = "deny" };
        }
    }
}
