using AuthZen.AspNetCore.AuthZen.AspNetCore.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _relation;
        private readonly string? _resourceType;
        private readonly string? _resourceId;
        private readonly string? _userId;

        public AuthorizeAttribute(string relation, string? resourceType = null, string? resourceId = null, string? userId = null)
        {
            _relation = relation;
            _resourceType = resourceType;
            _resourceId = resourceId;
            _userId = userId;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            var userId = _userId ?? context.HttpContext.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var allowed = await authService.CheckAccessAsync(new IAuthorizationService.CheckAccessDto(
                    userId,
                    _relation,
                    _resourceType ?? "DefaultResource",
                    _resourceId ?? "DefaultId"));

            if (!allowed)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
