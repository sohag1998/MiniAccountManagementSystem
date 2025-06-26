using AccountManagemetSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace AccountManagemetSystem.Attributes
{
    public class PermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _module;
        private readonly string _permission;

        public PermissionAttribute(string module, string permission)
        {
            _module = module;
            _permission = permission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
            var hasPermission = await permissionService.CheckUserPermissionAsync(userId, _module, _permission);

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
