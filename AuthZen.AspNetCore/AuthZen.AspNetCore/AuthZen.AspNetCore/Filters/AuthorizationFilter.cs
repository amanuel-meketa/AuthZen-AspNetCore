using AuthZen.AspNetCore.AuthZen.AspNetCore.Attributes;
using AuthZen.AspNetCore.AuthZen.AspNetCore.Extensions;
using AuthZen.AspNetCore.AuthZen.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Security.Claims;

namespace AuthZen.AspNetCore.Filters
{
    public sealed class AuthorizationFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _authService;
        private readonly ILogger<AuthorizationFilter> _logger;

        public AuthorizationFilter(IAuthorizationService authService, ILogger<AuthorizationFilter> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var methodInfo = (context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)?.MethodInfo;
            var controllerType = context.Controller?.GetType()!;
            var attribute = methodInfo?.GetCustomAttributes(typeof(AuthorizeResourceAttribute), true) .FirstOrDefault() as AuthorizeResourceAttribute;

            if (attribute == null)
            {
                await next();
                return;
            }

            // Resolve userId: attribute first, then JWT claim
            string? userId = attribute.UserId ?? context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Authorization failed: userId missing. Attribute UserId: {AttrUserId}", attribute.UserId);

                context.Result = new BadRequestObjectResult(new
                {
                    reason = "UserId must come from attribute or claim."
                });
                return;
            }

            // Resolve resourceId: attribute first, then controller route
            string? resourceId = !string.IsNullOrWhiteSpace(attribute.ResourceId) ? attribute.ResourceId
                                 : GetControllerRoute(controllerType);

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                _logger.LogWarning("Authorization failed: resourceId missing for controller {Controller}", controllerType.Name);
                context.Result = new BadRequestObjectResult(new
                {
                    reason = "ResourceId could not be determined from attribute or controller route."
                });
                return;
            }

            var checkRequest = new IAuthorizationService.CheckAccessDto
            {
                Subject = new IAuthorizationService.SubjectDto { Id = userId, Type = "user" },
                Resource = new IAuthorizationService.ResourceDto { Id = resourceId, Type = attribute.ResourceType.ToString() },
                Action = attribute.Action.ToAuthZenAction()
            };

            var decision = await _authService.CheckAccessAsync(checkRequest);

            if (!string.Equals(decision.Decision, "allow", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation( "Authorization denied for user {UserId} on resource {ResourceId} ({ResourceType}) with action {Action}. Reason: {Reason}",
                    userId, resourceId, attribute.ResourceType, attribute.Action, decision.Reason);

                context.Result = new ObjectResult(decision) { StatusCode = 403 };
                return;
            }

            _logger.LogDebug( "Authorization granted for user {UserId} on resource {ResourceId} ({ResourceType}) with action {Action}.",
                userId, resourceId, attribute.ResourceType, attribute.Action);

            await next();
        }

        /// <summary>
        /// Gets the first [Route] attribute template from the controller.
        /// Returns the route template as resourceId (e.g., "approval-template").
        /// </summary>
        private static string? GetControllerRoute(Type controllerType)
        {
            var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
            if (routeAttr == null) return null;

            var template = routeAttr.Template ?? string.Empty;
            if (template.Contains("[controller]"))
            {
                template = template.Replace("[controller]", controllerType.Name.Replace("Controller", "", StringComparison.OrdinalIgnoreCase));
            }

            return template.Trim('/');
        }
    }
}