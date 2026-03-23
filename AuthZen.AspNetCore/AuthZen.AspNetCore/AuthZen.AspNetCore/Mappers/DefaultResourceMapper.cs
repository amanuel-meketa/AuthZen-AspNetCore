using AuthZen.AspNetCore.AuthZen.AspNetCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuthZen.AspNetCore.AuthZen.AspNetCore.Mappers
{
    public static class DefaultResourceMapper
    {
        public static readonly Dictionary<string, Resource> DefaultResourceIds = new()
        {
            { "approval-template", Resource.Template },
            { "application-instance", Resource.TempInstance },
            { "application-Stage", Resource.Stage },
            { "application-StageInstance", Resource.StageInstance },
            { "application-TemplateHub", Resource.TemplateHub },
            { "application-form", Resource.Form },
            { "application-FormInstance", Resource.FormInstance },
            { "application-UserManagement", Resource.UserManagement },
            { "application-RoleManagement", Resource.RoleManagement }
        };

        public static Resource? Map(string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId)) return null;
            return DefaultResourceIds.TryGetValue(resourceId, out var res) ? res : null;
        }
    }
}
