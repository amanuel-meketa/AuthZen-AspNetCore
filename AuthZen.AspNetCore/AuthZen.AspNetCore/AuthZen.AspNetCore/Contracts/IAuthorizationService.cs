namespace AuthZen.AspNetCore.Contracts
{
    /// <summary>
    /// Core authorization service using AuthZEN protocol.
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Checks whether the given subject is allowed to perform the action on the resource.
        /// Returns an AuthZEN-compliant decision.
        /// </summary>
        /// <param name="check">The access check request.</param>
        /// <returns>An AuthZEN-compliant decision response.</returns>
        Task<AuthZenDecisionResponseDto> CheckAccessAsync(CheckAccessDto check);
    }

    /// <summary>
    /// AuthZEN Check Request
    /// </summary>
    public sealed class CheckAccessDto
    {
        /// <summary>
        /// The subject (user or other principal) performing the action.
        /// </summary>
        public SubjectDto Subject { get; set; } = default!;

        /// <summary>
        /// The resource the action is being performed on.
        /// </summary>
        public ResourceDto Resource { get; set; } = default!;

        /// <summary>
        /// The action/relation being checked (e.g., "view", "edit").
        /// </summary>
        public string Action { get; set; } = default!;
    }

    /// <summary>
    /// The subject performing the action.
    /// </summary>
    public sealed class SubjectDto
    {
        /// <summary>
        /// Type of the subject. Default is "user".
        /// </summary>
        public string Type { get; set; } = "user";

        /// <summary>
        /// Unique identifier of the subject.
        /// </summary>
        public string Id { get; set; } = default!;
    }

    /// <summary>
    /// The resource being acted upon.
    /// </summary>
    public sealed class ResourceDto
    {
        /// <summary>
        /// Type of the resource (e.g., "Template", "Document").
        /// </summary>
        public string Type { get; set; } = default!;

        /// <summary>
        /// Unique identifier of the resource.
        /// </summary>
        public string Id { get; set; } = default!;
    }

    /// <summary>
    /// AuthZEN-compliant decision response.
    /// </summary>
    public sealed class AuthZenDecisionResponseDto
    {
        /// <summary>
        /// Decision of the access check. Either "allow" or "deny".
        /// </summary>
        public string Decision { get; set; } = "deny";

        /// <summary>
        /// Optional reason explaining the decision.
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Optional obligations that the caller must fulfill for access.
        /// </summary>
        public List<object> Obligations { get; set; } = new();

        /// <summary>
        /// Optional advice returned by the authorization service.
        /// </summary>
        public List<object> Advice { get; set; } = new();
    }
}
