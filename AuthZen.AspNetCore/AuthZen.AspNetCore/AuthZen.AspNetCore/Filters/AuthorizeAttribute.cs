using AuthZen.AspNetCore.AuthZen.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;

namespace AuthZen.AspNetCore.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class AuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _action;
        private readonly string? _resourceType;
        private readonly string? _resourceId;
        private readonly string? _subjectId;

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

            string? subjectId = _subjectId;

            if (string.IsNullOrEmpty(subjectId))
            {
                // Dev mode: header override
                subjectId = context.HttpContext.Request.Headers["X-User-Id"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(subjectId))
            {
                // Production: extract from Bearer token
                var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);

                    subjectId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                }
            }

            if (string.IsNullOrEmpty(subjectId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var resourceType = _resourceType ?? "DefaultResource";
            var resourceId = _resourceId ?? "DefaultId";

            var checkRequest = new CheckAccessDto
            {
                Subject = new SubjectDto { Id = subjectId, Type = "user" },
                Resource = new ResourceDto { Id = resourceId, Type = resourceType },
                Action = _action
            };

            AuthZenDecisionResponseDto decision;
            try
            {
                decision = await authService.CheckAccessAsync(checkRequest);
            }
            catch
            {
                context.Result = new ObjectResult(new AuthZenDecisionResponseDto
                {
                    Decision = "deny",
                    Reason = "AuthZEN service unreachable"
                })
                { StatusCode = 403 };
                return;
            }

            if (!string.Equals(decision.Decision, "allow", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(decision) { StatusCode = 403 };
                return;
            }

            await next();
        }
    }
}
