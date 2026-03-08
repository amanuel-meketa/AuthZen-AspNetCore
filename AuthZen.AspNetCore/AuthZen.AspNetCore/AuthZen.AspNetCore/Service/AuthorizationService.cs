using AuthZen.AspNetCore.AuthZen.Contracts;
using System.Net.Http.Json;
using static AuthZen.AspNetCore.AuthZen.Contracts.IAuthorizationService;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Service
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly HttpClient _http;

        public AuthorizationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<AuthZenDecisionResponseDto> CheckAccessAsync(CheckAccessDto check)
        {
            if (check == null) throw new ArgumentNullException(nameof(check));

            var request = new
            {
                subject = new { id = check.Subject.Id, type = check.Subject.Type },
                action = check.Action,
                resource = new { id = check.Resource.Id, type = check.Resource.Type }
            };

            try
            {
                var response = await _http.PostAsJsonAsync("/check-access", request);

                if (!response.IsSuccessStatusCode)
                {
                    return new AuthZenDecisionResponseDto
                    {
                        Decision = "deny",
                        Reason = $"AuthZEN returned status {response.StatusCode}"
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<AuthZenResponse>();

                return new AuthZenDecisionResponseDto
                {
                    Decision = result?.Decision ?? "deny",
                    Reason = result?.Reason,
                    Obligations = result?.Obligations ?? new List<object>(),
                    Advice = result?.Advice ?? new List<object>()
                };
            }
            catch (Exception ex)
            {
                return new AuthZenDecisionResponseDto
                {
                    Decision = "deny",
                    Reason = $"AuthZEN request failed: {ex.Message}"
                };
            }
        }

        private class AuthZenResponse
        {
            public string Decision { get; set; } = "deny";
            public string? Reason { get; set; }
            public List<object> Obligations { get; set; } = new();
            public List<object> Advice { get; set; } = new();
        }
    }
}