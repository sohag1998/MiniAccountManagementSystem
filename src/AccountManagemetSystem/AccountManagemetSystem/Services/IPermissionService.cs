using AccountManagemetSystem.Models;

namespace AccountManagemetSystem.Services
{
    public interface IPermissionService
    {
        Task<List<Module>> GetAllModulesAsync();
        Task<List<Permission>> GetAllPermissionsAsync();
        Task<List<RolePermissionDto>> GetRolePermissionsAsync(string roleId);
        Task<ApiResponse> SaveRolePermissionAsync(string roleId, int moduleId, int permissionId, bool isGranted);
        Task<bool> CheckUserPermissionAsync(string userId, string moduleName, string permissionName);
        Task<List<UserWithRolesDto>> GetUsersWithRolesAsync();
        Task<ApiResponse> ManageUserRoleAsync(string action, string userId, string roleId);
        Task<ApiResponse> InitializeDefaultRolesPermissionsAsync();
    }
}
