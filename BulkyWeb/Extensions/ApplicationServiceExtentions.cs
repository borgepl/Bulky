using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddMyAppServices(this IServiceCollection services, 
                                                                 IConfiguration config)

        {
            
            services.AddDbContext<ApplicationDbContext>(opt => 
            {
                opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
           
            services.AddScoped<ICategoryRepository, CategoryRepository>();
           
            // services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            // services.AddAutoMapper(typeof(MappingProfiles).Assembly);

            
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                        // .WithOrigins("http://localhost:4200")
                        // .WithOrigins("https://localhost:4200")
                        // .WithOrigins("https://accounts.google.com");
                });
            });

            return services;
        }
    }
}