namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Enums
{
    public enum Action
    {
        Create,
        Update,
        Delete,
        View,
        ViewAll,
        Assign,
        Unassign,
        Start
    }

    public enum Resource
    {
        Template,
        TempInstance,
        Stage,
        StageInstance,
        TemplateHub,
        Form,
        FormInstance,
        UserManagement,
        RoleManagement
    }
}
