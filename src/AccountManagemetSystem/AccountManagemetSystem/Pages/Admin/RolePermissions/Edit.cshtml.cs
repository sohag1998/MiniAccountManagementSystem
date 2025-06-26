using AccountManagemetSystem.Attributes;
using AccountManagemetSystem.Models;
using AccountManagemetSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccountManagemetSystem.Pages.Admin.RolePermissions
{
    [Permission("UserManagement", "Edit")]
    public class EditModel : PageModel
    {
        private readonly IPermissionService _permissionService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EditModel(IPermissionService permissionService, RoleManager<IdentityRole> roleManager)
        {
            _permissionService = permissionService;
            _roleManager = roleManager;
        }

        [BindProperty]
        public RolePermissionViewModel ViewModel { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound();
            }

            ViewModel.RoleId = roleId;
            ViewModel.RoleName = role.Name ?? "";

            await LoadRolePermissionsAsync(roleId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRolePermissionsAsync(ViewModel.RoleId);
                return Page();
            }

            var successCount = 0;
            var errorCount = 0;

            foreach (var moduleGroup in ViewModel.ModulePermissions)
            {
                foreach (var permission in moduleGroup.Permissions)
                {
                    var result = await _permissionService.SaveRolePermissionAsync(
                        ViewModel.RoleId,
                        moduleGroup.ModuleId,
                        permission.PermissionId,
                        permission.IsGranted);

                    if (result.Success)
                        successCount++;
                    else
                        errorCount++;
                }
            }

            StatusMessage = errorCount == 0 ?
                $"Success: {successCount} permissions updated successfully." :
                $"Warning: {successCount} permissions updated, {errorCount} failed.";

            return RedirectToPage("./Index");
        }

        private async Task LoadRolePermissionsAsync(string roleId)
        {
            var rolePermissions = await _permissionService.GetRolePermissionsAsync(roleId);

            ViewModel.ModulePermissions = rolePermissions
                .GroupBy(rp => new { rp.ModuleId, rp.ModuleName })
                .Select(g => new ModulePermissionGroup
                {
                    ModuleId = g.Key.ModuleId,
                    ModuleName = g.Key.ModuleName,
                    Permissions = g.Select(p => new PermissionItem
                    {
                        PermissionId = p.PermissionId,
                        PermissionName = p.PermissionName,
                        IsGranted = p.IsGranted
                    }).ToList()
                }).ToList();
        }
    }
}
