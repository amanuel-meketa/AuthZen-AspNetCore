using AuthZen.AspNetCore.AuthZen.AspNetCore.Enums;
using Action = AuthZen.AspNetCore.AuthZen.AspNetCore.Enums.Action;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeResourceAttribute : Attribute
    {
        public AuthorizeResourceAttribute() { }

        public Resource ResourceType { get; set; }    
        public Action Action { get; set; }          
        public string? CustomAction { get; set; }   
        public string? UserId { get; set; }
        public string? ResourceId { get; set; }
    }
}