using AuthZen.AspNetCore.AuthZen.AspNetCore.Extensions;
using Action = AuthZen.AspNetCore.AuthZen.AspNetCore.Enums.Action;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Resolvers
{
    public static class ActionResolver
    {
        public static string ResolveAction(Action? attributeAction, string httpMethod, string? routeTemplate = null)
        {
            if (attributeAction.HasValue)
                return attributeAction.Value.ToAuthZenAction();

            switch (httpMethod.ToUpper())
            {
                case "GET":
                    if (!string.IsNullOrEmpty(routeTemplate) && routeTemplate.Contains("{id}"))
                        return Action.View.ToAuthZenAction();
                    return Action.ViewAll.ToAuthZenAction();
                case "POST": return Action.Create.ToAuthZenAction();
                case "PUT": return Action.Update.ToAuthZenAction();
                case "DELETE": return Action.Delete.ToAuthZenAction();
                default: return Action.View.ToAuthZenAction();
            }
        }
    }
}
