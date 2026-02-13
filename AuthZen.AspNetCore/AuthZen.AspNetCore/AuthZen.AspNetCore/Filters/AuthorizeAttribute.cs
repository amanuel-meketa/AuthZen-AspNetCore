using AuthZen.AspNetCore.AuthZen.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace AuthZen.AspNetCore.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class AuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _action;
        private readonly string? _resourceType;
        private readonly string? _resourceId;
        private readonly string? _userIdFromController;

        /// <summary>
        /// Protects an endpoint using AuthZEN protocol.
        /// </summary>
        /// <param name="action">The action/relation to check, e.g., "view", "edit".</param>
        /// <param name="resourceType">Optional resource type override.</param>
        /// <param name="resourceId">Optional resource ID override.</param>
        /// <param name="userIdFromController">Optional userId explicitly passed from controller (hardcoded).</param>
        public AuthorizeAttribute(string action, string? resourceType = null, string? resourceId = null, string? userIdFromController = null)
        {
            _action = action;
            _resourceType = resourceType;
            _resourceId = resourceId;
            _userIdFromController = userIdFromController;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            // Use controller userId if provided, otherwise fallback to hardcoded dev userId
            var subjectId = _userIdFromController ?? "123";

            // Resource defaults
            var resourceType = _resourceType ?? "DefaultResource";
            var resourceId = _resourceId ?? "DefaultId";

            // Construct AuthZEN request
            var checkRequest = new CheckAccessDto
            {
                Subject = new SubjectDto { Id = subjectId, Type = "user" },
                Resource = new ResourceDto { Id = resourceId, Type = resourceType },
                Action = _action
            };

            // Perform AuthZEN check
            AuthZenDecisionResponseDto decision;
            try
            {
                decision = await authService.CheckAccessAsync(checkRequest);
            }
            catch
            {
                // If service fails, deny access
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            if (!string.Equals(decision.Decision, "allow", StringComparison.OrdinalIgnoreCase))
            {
                // Deny access
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            // Continue pipeline
            await next();
        }
    }
}
