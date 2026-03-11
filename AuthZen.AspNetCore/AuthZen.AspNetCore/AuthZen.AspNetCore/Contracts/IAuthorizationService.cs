namespace AuthZen.AspNetCore.AuthZen.Contracts
{
    /// <summary>
    /// Core authorization service using AuthZEN protocol.
    /// </summary>
    public interface IAuthorizationService
    {
        Task<AuthZenDecisionResponseDto> CheckAccessAsync(CheckAccessDto check);

        public sealed class CheckAccessDto
        {
            public SubjectDto Subject { get; set; } = default!;
            public ResourceDto Resource { get; set; } = default!;
            public string Action { get; set; } = default!;
        }

        public sealed class SubjectDto
        {
            public string Type { get; set; } = "user";
            public string Id { get; set; } = default!;
        }

        public sealed class ResourceDto
        {
            public string Type { get; set; } = default!;
            public string Id { get; set; } = default!;
        }

        public sealed class AuthZenDecisionResponseDto
        {
            public string Decision { get; set; } = "deny";
            public string? Reason { get; set; }
            public List<object> Obligations { get; set; } = new();
            public List<object> Advice { get; set; } = new();
        }
    }
}
