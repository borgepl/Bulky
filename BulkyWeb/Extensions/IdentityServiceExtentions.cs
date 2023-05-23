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

            services.Configure<IdentityOptions>( options => {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 5;
                // options.Password.RequireDigit = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
                // options.SignIn.RequireConfirmedEmail = true;
            });

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