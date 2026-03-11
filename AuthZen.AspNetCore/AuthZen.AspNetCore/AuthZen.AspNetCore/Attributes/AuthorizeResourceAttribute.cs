using AuthZen.AspNetCore.AuthZen.AspNetCore.Enums;
using Action = AuthZen.AspNetCore.AuthZen.AspNetCore.Enums.Action;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizeResourceAttribute : Attribute
    {
        public Resource ResourceType { get; }
        public Action Action { get; }

        /// <summary>Optional hardcoded userId.</summary>
        public string? UserId { get; set; }

        /// <summary>Optional hardcoded resourceId.</summary>
        public string? ResourceId { get; set; }

        public AuthorizeResourceAttribute(Resource resourceType, Action action)
        {
            ResourceType = resourceType;
            Action = action;
        }
    }
}