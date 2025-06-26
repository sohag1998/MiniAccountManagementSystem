using AccountManagemetSystem.Attributes;
using AccountManagemetSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccountManagemetSystem.Pages.Admin.RolePermissions
{
    [Permission("UserManagement", "View")]
    public class IndexModel : PageModel
    {
        private readonly IPermissionService _permissionService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(IPermissionService permissionService, RoleManager<IdentityRole> roleManager)
        {
            _permissionService = permissionService;
            _roleManager = roleManager;
        }

        public List<IdentityRole> Roles { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public void OnGet()
        {
            Roles = _roleManager.Roles.ToList();
        }
    }
}
