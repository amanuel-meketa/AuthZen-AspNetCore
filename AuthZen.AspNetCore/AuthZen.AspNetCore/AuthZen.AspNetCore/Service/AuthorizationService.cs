using AuthZen.AspNetCore.AuthZen.Contracts;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using static AuthZen.AspNetCore.AuthZen.Contracts.IAuthorizationService;

namespace AuthZen.AspNetCore.Service
{
    public sealed class AuthorizationService : IAuthorizationService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(HttpClient http, ILogger<AuthorizationService> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthZenDecisionResponseDto> CheckAccessAsync(CheckAccessDto check)
        {
            if (_http.BaseAddress == null)
                throw new InvalidOperationException("AuthZEN BaseAddress is not configured.");

            var request = new
            {
                subject = new { id = check.Subject.Id, type = check.Subject.Type },
                resource = new { id = check.Resource.Id, type = check.Resource.Type },
                action = check.Action
            };

            try
            {
                var response = await _http.PostAsJsonAsync("/api/access/check", request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AuthZen returned non-success status {Status} for user {UserId}", response.StatusCode, check.Subject.Id);

                    return new AuthZenDecisionResponseDto
                    {
                        Decision = "deny",
                        Reason = $"AuthZen returned status {response.StatusCode}"
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<AuthZenDecisionResponseDto>();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthZen request failed for user {UserId}", check.Subject.Id);
                return new AuthZenDecisionResponseDto
                {
                    Decision = "deny",
                    Reason = $"AuthZen request failed: {ex.Message}"
                };
            }
        }
    }
}