using AccountManagemetSystem.Attributes;
using AccountManagemetSystem.Models;
using AccountManagemetSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccountManagemetSystem.Pages.Admin.UserManagement
{
    [Permission("UserManagement", "Edit")]
    public class IndexModel : PageModel
    {
        private readonly IPermissionService _permissionService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(IPermissionService permissionService, RoleManager<IdentityRole> roleManager)
        {
            _permissionService = permissionService;
            _roleManager = roleManager;
        }

        public UserRoleManagementViewModel ViewModel { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            ViewModel.Users = await _permissionService.GetUsersWithRolesAsync();
            ViewModel.AvailableRoles = _roleManager.Roles.ToList();
        }

        public async Task<IActionResult> OnPostAsync(string userId, string roleId, string action)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleId))
            {
                StatusMessage = "Error: Invalid user or role selection.";
                return RedirectToPage();
            }

            var result = await _permissionService.ManageUserRoleAsync(action.ToUpper(), userId, roleId);

            StatusMessage = result.Success ?
                $"Success: {result.Message}" :
                $"Error: {result.Message}";

            return RedirectToPage();
        }
    }

}
