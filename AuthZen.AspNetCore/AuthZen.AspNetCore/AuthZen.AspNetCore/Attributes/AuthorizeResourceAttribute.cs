using AuthZen.AspNetCore.AuthZen.AspNetCore.Enums;
using Action = AuthZen.AspNetCore.AuthZen.AspNetCore.Enums.Action;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeResourceAttribute : Attribute
    {
        // Allow setting in constructor
        public Resource? ResourceType { get; private set; }
        public Action? Action { get; private set; }

        // Optional custom action string
        public string? CustomAction { get; set; }

        public string? UserId { get; set; }
        public string? ResourceId { get; set; }

        // Constructor with optional parameters
        public AuthorizeResourceAttribute(Resource? resourceType = null, Action? action = null)
        {
            ResourceType = resourceType;
            Action = action;
        }
    }
}