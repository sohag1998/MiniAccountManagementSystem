using AccountManagemetSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AccountManagemetSystem.Services
{
    public class PermissionService: IPermissionService
    {
        private readonly string _connectionString;

        public PermissionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
        }

        public async Task<List<Module>> GetAllModulesAsync()
        {
            var modules = new List<Module>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetAllModules", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                modules.Add(new Module
                {
                    ModuleId = reader.GetInt32("ModuleId"),
                    ModuleName = reader.GetString("ModuleName"),
                    ModuleDescription = reader.IsDBNull("ModuleDescription") ? "" : reader.GetString("ModuleDescription"),
                    IsActive = reader.GetBoolean("IsActive")
                });
            }

            return modules;
        }

        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            var permissions = new List<Permission>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetAllPermissions", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                permissions.Add(new Permission
                {
                    PermissionId = reader.GetInt32("PermissionId"),
                    PermissionName = reader.GetString("PermissionName"),
                    PermissionDescription = reader.IsDBNull("PermissionDescription") ? "" : reader.GetString("PermissionDescription"),
                    IsActive = reader.GetBoolean("IsActive")
                });
            }

            return permissions;
        }

        public async Task<List<RolePermissionDto>> GetRolePermissionsAsync(string roleId)
        {
            var rolePermissions = new List<RolePermissionDto>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetRolePermissions", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@RoleId", roleId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                rolePermissions.Add(new RolePermissionDto
                {
                    ModuleId = reader.GetInt32("ModuleId"),
                    ModuleName = reader.GetString("ModuleName"),
                    PermissionId = reader.GetInt32("PermissionId"),
                    PermissionName = reader.GetString("PermissionName"),
                    IsGranted = reader.GetBoolean("IsGranted")
                });
            }

            return rolePermissions;
        }

        public async Task<ApiResponse> SaveRolePermissionAsync(string roleId, int moduleId, int permissionId, bool isGranted)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_SaveRolePermissions", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@RoleId", roleId);
            command.Parameters.AddWithValue("@ModuleId", moduleId);
            command.Parameters.AddWithValue("@PermissionId", permissionId);
            command.Parameters.AddWithValue("@IsGranted", isGranted);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ApiResponse
                {
                    Success = reader.GetInt32("Success") == 1 ? true : false,
                    Message = reader.GetString("Message")
                };
            }

            return new ApiResponse { Success = false, Message = "Unknown error occurred" };
        }

        public async Task<bool> CheckUserPermissionAsync(string userId, string moduleName, string permissionName)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_CheckUserPermission", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@ModuleName", moduleName);
            command.Parameters.AddWithValue("@PermissionName", permissionName);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }

        public async Task<List<UserWithRolesDto>> GetUsersWithRolesAsync()
        {
            var users = new List<UserWithRolesDto>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetUsersWithRoles", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new UserWithRolesDto
                {
                    UserId = reader.GetString("UserId"),
                    UserName = reader.GetString("UserName"),
                    Email = reader.GetString("Email"),
                    EmailConfirmed = reader.GetBoolean("EmailConfirmed"),
                    LockoutEnabled = reader.GetBoolean("LockoutEnabled"),
                    Roles = reader.IsDBNull("Roles") ? "" : reader.GetString("Roles")
                });
            }

            return users;
        }

        public async Task<ApiResponse> ManageUserRoleAsync(string action, string userId, string roleId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ManageUserRole", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Action", action);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@RoleId", roleId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ApiResponse
                {
                    Success = reader.GetInt32("Success") == 1 ? true : false,
                    Message = reader.GetString("Message")
                };
            }

            return new ApiResponse { Success = false, Message = "Unknown error occurred" };
        }

        public async Task<ApiResponse> InitializeDefaultRolesPermissionsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_InitializeDefaultRolesPermissions", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ApiResponse
                {
                    Success = reader.GetBoolean("Success"),
                    Message = reader.GetString("Message")
                };
            }

            return new ApiResponse { Success = false, Message = "Unknown error occurred" };
        }
    }
}
