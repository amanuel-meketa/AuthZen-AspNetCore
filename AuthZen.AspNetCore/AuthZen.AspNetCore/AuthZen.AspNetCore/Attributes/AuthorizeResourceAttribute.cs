using AuthZen.AspNetCore.AuthZen.AspNetCore.Enums;
using Action = AuthZen.AspNetCore.AuthZen.AspNetCore.Enums.Action;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeResourceAttribute : Attribute
    {
        // Constructor takes no parameters
        public AuthorizeResourceAttribute() { }

        // Optional properties (must be set by name)
        public Resource ResourceType { get; set; }    // enum type
        public Action Action { get; set; }           // enum type
        public string? CustomAction { get; set; }    // string fallback
        public string? UserId { get; set; }
        public string? ResourceId { get; set; }
    }
}