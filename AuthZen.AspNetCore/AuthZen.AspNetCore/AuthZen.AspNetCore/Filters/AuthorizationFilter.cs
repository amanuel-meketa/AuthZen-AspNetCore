using AuthZen.AspNetCore.AuthZen.AspNetCore.Attributes;
using AuthZen.AspNetCore.AuthZen.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static AuthZen.AspNetCore.AuthZen.Contracts.IAuthorizationService;
using Action = AuthZen.AspNetCore.AuthZen.AspNetCore.Enums.Action;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Filters
{
    public sealed class AuthorizationFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _authService;

        public AuthorizationFilter(IAuthorizationService authService)
        {
            ArgumentNullException.ThrowIfNull(authService);
            _authService = authService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get the AuthorizeResourceAttribute from action or controller
            var methodInfo = (context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)?.MethodInfo;
            var attribute = methodInfo?.GetCustomAttributes(typeof(AuthorizeResourceAttribute), true).FirstOrDefault() as AuthorizeResourceAttribute
                            ?? context.Controller.GetType().GetCustomAttributes(typeof(AuthorizeResourceAttribute), true).FirstOrDefault() as AuthorizeResourceAttribute;

            if (attribute is null)
            {
                await next(); // No attribute → allow
                return;
            }

            // Get userId: use attribute.UserId if provided, else fallback to claim
            var userId = !string.IsNullOrWhiteSpace(attribute.UserId) ? attribute.UserId : context.HttpContext.User?.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                context.Result = new ObjectResult(new { reason = "User not authenticated" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            // Get resourceId: use attribute.ResourceId if provided, else look for action argument
            var resourceId = !string.IsNullOrWhiteSpace(attribute.ResourceId) ? attribute.ResourceId
                                : context.ActionArguments.TryGetValue("resourceId", out var ridObj) && ridObj is not null ? ridObj.ToString()! : null;

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                context.Result = new ObjectResult(new { reason = "ResourceId must be provided either via attribute or action argument." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
                return;
            }

            // Build check request
            var checkRequest = new CheckAccessDto
            {
                Subject = new SubjectDto { Id = userId, Type = "user" },
                Resource = new ResourceDto { Id = resourceId, Type = attribute.ResourceType.ToString() },
                Action = MapAction(attribute.Action)
            };

            var decision = await _authService.CheckAccessAsync(checkRequest);

            if (!string.Equals(decision.Decision, "allow", StringComparison.OrdinalIgnoreCase))
            {
                // Return the PDP response directly with 403
                context.Result = new ObjectResult(decision)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
        }

        private static string MapAction(Action action) => action switch
        {
            Action.Create => "can_create",
            Action.Update => "can_update",
            Action.Delete => "can_delete",
            Action.View => "can_view",
            Action.Assign => "can_assign",
            Action.Unassign => "can_unassign",
            Action.Start => "can_start",
            _ => throw new ArgumentOutOfRangeException(nameof(action), "Unknown action")
        };
    }
}