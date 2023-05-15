using Bulky.DataAccess.Data;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BulkyWeb.Extensions
{
    public static class IdentityServiceExtentions
    {
        public static IServiceCollection AddMyIdentityServices(this IServiceCollection services,  IConfiguration config)
        {
            
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options => {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });

            services.AddScoped<IEmailSender, EmailSender>();

            services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = config["Authentication:Google:ClientId"];
                    googleOptions.ClientSecret = config["Authentication:Google:ClientSecret"];
                    googleOptions.SignInScheme = IdentityConstants.ExternalScheme;
                });

            services.AddAuthorization();

            return services;
        }
    }
}