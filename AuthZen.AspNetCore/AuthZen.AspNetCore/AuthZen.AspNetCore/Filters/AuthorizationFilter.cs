using AuthZen.AspNetCore.AuthZen.AspNetCore.Attributes;
using AuthZen.AspNetCore.AuthZen.AspNetCore.Enums;
using AuthZen.AspNetCore.AuthZen.Contracts;
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
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get AuthorizeResourceAttribute
            var methodInfo = (context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)?.MethodInfo;
            var attribute = methodInfo?.GetCustomAttributes(typeof(AuthorizeResourceAttribute), true).FirstOrDefault() as AuthorizeResourceAttribute;

            if (attribute == null)
            {
                await next();
                return;
            }

            // ONLY get userId and resourceId from action arguments
            var userId = context.ActionArguments.TryGetValue("userId", out var uidObj) ? uidObj?.ToString() : attribute.UserId;
            var resourceId = context.ActionArguments.TryGetValue("resourceId", out var ridObj) ? ridObj?.ToString() : attribute.ResourceId;

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(resourceId))
            {
                context.Result = new BadRequestObjectResult(new
                {
                    reason = "Both userId and resourceId must be provided in action arguments."
                });
                return;
            }

            var checkRequest = new CheckAccessDto
            {
                Subject = new SubjectDto
                {
                    Id = userId,
                    Type = "user"
                },
                Resource = new ResourceDto
                {
                    Id = resourceId,
                    Type = attribute.ResourceType.ToString()
                },
                Action = MapAction(attribute.Action)
            };

            var decision = await _authService.CheckAccessAsync(checkRequest);

            if (!string.Equals(decision.Decision, "allow", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(decision)
                {
                    StatusCode = 403
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
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };
    }
}