namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Extensions
{
    public static class ActionExtensions
    {
        public static string ToAuthZenAction(this Enums.Action action) => action switch
        {
            Enums.Action.Create => "can_create",
            Enums.Action.Update => "can_update",
            Enums.Action.Delete => "can_delete",
            Enums.Action.View => "can_view",
            Enums.Action.ViewAll => "can_view_all",
            Enums.Action.Assign => "can_assign",
            Enums.Action.Unassign => "can_unassign",
            Enums.Action.Start => "can_start",
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };
    }
}
