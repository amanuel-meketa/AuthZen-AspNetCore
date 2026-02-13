using AuthZen.AspNetCore.AuthZen.AspNetCore.Configuration;
using AuthZen.AspNetCore.AuthZen.AspNetCore.Contracts;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Http
{
    public class AuthorizationServiceHttp : IAuthorizationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;

        public AuthorizationServiceHttp(HttpClient httpClient, IOptions<Configuration.Options> options)
        {
            _httpClient = httpClient;
            _url = options.Value.Url;
        }

        public async Task<bool> CheckAccessAsync(IAuthorizationService.CheckAccessDto check)
        {
            var response = await _httpClient.PostAsJsonAsync(_url, check);

            if (!response.IsSuccessStatusCode)
                return false;

            var allowed = await response.Content.ReadFromJsonAsync<bool>();
            return allowed;
        }
    }
}
