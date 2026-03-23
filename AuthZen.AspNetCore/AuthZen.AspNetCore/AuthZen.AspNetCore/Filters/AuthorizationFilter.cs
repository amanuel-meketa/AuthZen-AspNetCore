using AuthZen.AspNetCore.AuthZen.AspNetCore.Attributes;
using AuthZen.AspNetCore.AuthZen.AspNetCore.Extensions;
using AuthZen.AspNetCore.AuthZen.AspNetCore.Mappers;
using AuthZen.AspNetCore.AuthZen.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Action = AuthZen.AspNetCore.AuthZen.AspNetCore.Enums.Action;

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
            var attribute = methodInfo?.GetCustomAttributes(typeof(AuthorizeResourceAttribute), true)
                                   .FirstOrDefault() as AuthorizeResourceAttribute;

            if (attribute == null)
            {
                await next();
                return;
            }

            // --- Get UserId from claim or attribute fallback ---
            string? userId = context.HttpContext.User.GetUserId(attribute.UserId);
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning($"Authorization failed: userId missing. Attribute UserId: {attribute.UserId}");
                context.Result = new BadRequestObjectResult(new { reason = "UserId must come from attribute or claim." });
                return;
            }

            // --- Resolve ResourceId: Attribute -> Controller Route ---
            string? resourceId = !string.IsNullOrWhiteSpace(attribute.ResourceId) ? attribute.ResourceId : GetControllerRoute(controllerType);

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                _logger.LogWarning($"Authorization failed: resourceId missing for controller {controllerType.Name}");
                context.Result = new BadRequestObjectResult(new { reason = "ResourceId could not be determined." });
                return;
            }

            // --- Resolve ResourceType: Attribute -> Mapper -> null ---
            var resourceType = attribute.ResourceType ?? DefaultResourceMapper.Map(resourceId);

            // --- Resolve Action: CustomAction -> Enum -> HTTP method ---
            var action = attribute.CustomAction ?? (attribute.Action.HasValue ? attribute.Action.Value.ToAuthZenAction()
                             : ResolveAction(null, context.HttpContext.Request.Method, resourceId));

            // --- Build authorization request ---
            var checkRequest = new IAuthorizationService.CheckAccessDto
            {
                Subject = new IAuthorizationService.SubjectDto { Id = userId, Type = "user" },
                Resource = new IAuthorizationService.ResourceDto
                {
                    Id = resourceId,
                    Type = resourceType?.ToString()
                },
                Action = action
            };

            // --- Perform authorization check ---
            var decision = await _authService.CheckAccessAsync(checkRequest);

            if (!string.Equals(decision.Decision, "allow", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"Authorization denied for user {userId} on resource {resourceId} ({resourceType}) with action {action}. Reason: {decision.Reason}");
                context.Result = new ObjectResult(decision) { StatusCode = 403 };
                return;
            }

            _logger.LogDebug($"Authorization granted for user {userId} on resource {resourceId} ({resourceType}) with action {action}.");
            await next();
        }

        private static string? GetControllerRoute(Type controllerType)
        {
            var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
            if (routeAttr == null) return null;

            var template = routeAttr.Template ?? string.Empty;
            if (template.Contains("[controller]"))
                template = template.Replace("[controller]", controllerType.Name.Replace("Controller", "", StringComparison.OrdinalIgnoreCase));

            return template.Trim('/');
        }

        private static string ResolveAction(Action? attributeAction, string httpMethod, string? routeTemplate)
        {
            if (attributeAction.HasValue)
                return attributeAction.Value.ToAuthZenAction();

            // Map HTTP method to default AuthZen actions
            return httpMethod.ToUpper() switch
            {
                "GET" => (!string.IsNullOrEmpty(routeTemplate) && routeTemplate.Contains("{id}")) ? Action.View.ToAuthZenAction() : Action.ViewAll.ToAuthZenAction(),
                "POST" => Action.Create.ToAuthZenAction(),
                "PUT" => Action.Update.ToAuthZenAction(),
                "DELETE" => Action.Delete.ToAuthZenAction(),
                _ => Action.View.ToAuthZenAction()
            };
        }
    }
}