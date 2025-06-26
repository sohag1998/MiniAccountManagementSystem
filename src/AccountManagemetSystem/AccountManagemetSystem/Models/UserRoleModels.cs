using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AccountManagemetSystem.Models
{
    public class Module
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleDescription { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Permission
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string PermissionDescription { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class RolePermissionDto
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
    }

    public class UserWithRolesDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public string Roles { get; set; } = string.Empty;
        public List<string> RoleList => string.IsNullOrEmpty(Roles)
            ? new List<string>()
            : Roles.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public class RolePermissionViewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<ModulePermissionGroup> ModulePermissions { get; set; } = new();
    }

    public class ModulePermissionGroup
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public List<PermissionItem> Permissions { get; set; } = new();
    }

    public class PermissionItem
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
    }

    public class AssignRoleViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string RoleId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class UserRoleManagementViewModel
    {
        public List<UserWithRolesDto> Users { get; set; } = new();
        public List<IdentityRole> AvailableRoles { get; set; } = new();
        public AssignRoleViewModel AssignRole { get; set; } = new();
    }
}
