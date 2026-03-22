using System.Security.Claims;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Get the user ID from claims or fallback to provided userId.
        /// </summary>
        public static string? GetUserId(this ClaimsPrincipal user, string? fallbackUserId = null)
        {
            if (user == null) return fallbackUserId;

            // Try standard NameIdentifier or "sub" claim
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;

            return string.IsNullOrWhiteSpace(userId) ? fallbackUserId : userId;
        }
    }
}
