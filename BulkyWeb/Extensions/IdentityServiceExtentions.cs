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
            services.AddScoped<IEmailSender, EmailSender>();

            return services;
        }
    }
}