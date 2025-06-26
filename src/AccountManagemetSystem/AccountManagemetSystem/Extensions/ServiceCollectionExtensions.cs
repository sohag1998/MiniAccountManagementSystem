using AccountManagemetSystem.Data;
using AccountManagemetSystem.Services;
using Microsoft.AspNetCore.Identity;

namespace AccountManagemetSystem.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPermissionServices(this IServiceCollection services)
        {
            services.AddScoped<IPermissionService, PermissionService>();
            return services;
        }

        public static IServiceCollection AddCustomIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDefaultIdentity<IdentityUser>(options =>
            {

                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            return services;
        }
    }
}
