using AuthZen.AspNetCore.AuthZen.AspNetCore.Enums;
using Action = AuthZen.AspNetCore.AuthZen.AspNetCore.Enums.Action;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizeResourceAttribute : Attribute
    {
        public Resource? ResourceType { get; }
        public Action? Action { get; }
        public string? CustomAction { get; set; }
        public string? UserId { get; set; }   
        public string? ResourceId { get; set; }

        public AuthorizeResourceAttribute(Resource? resourceType = null, Action? action = null)
        {
            ResourceType = resourceType;
            Action = action;
        }
    }
}