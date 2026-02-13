using AuthZen.AspNetCore.AuthZen.Contracts;
using Microsoft.AspNetCore.Mvc;
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
        private readonly string? _subjectId;

        /// <summary>
        /// Protects an endpoint using AuthZEN protocol.
        /// </summary>
        /// <param name="action">The action/relation to check, e.g., "view", "edit".</param>
        /// <param name="resourceType">Optional resource type override.</param>
        /// <param name="resourceId">Optional resource ID override.</param>
        /// <param name="subjectId">Optional subject override (otherwise uses authenticated user).</param>
        public AuthorizeAttribute(string action, string? resourceType = null, string? resourceId = null, string? subjectId = null)
        {
            _action = action;
            _resourceType = resourceType;
            _resourceId = resourceId;
            _subjectId = subjectId;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            // Determine subject
            var subjectId = _subjectId ?? context.HttpContext.User?.Identity?.Name;
            if (string.IsNullOrEmpty(subjectId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Determine resource
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
                // If the service fails, default deny
                context.Result = new ForbidResult();
                return;
            }

            if (!string.Equals(decision.Decision, "allow", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Continue pipeline
            await next();
        }
    }
}