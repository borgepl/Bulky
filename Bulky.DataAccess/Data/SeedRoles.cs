using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bulky.DataAccess.UoW;
using Bulky.Models.Identity;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;

namespace Bulky.DataAccess.Data
{
    public class SeedRoles
    {
         public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, 
            UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork)
        {
            if (!roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult()) {

                roleManager.CreateAsync( new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                roleManager.CreateAsync( new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                roleManager.CreateAsync( new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                roleManager.CreateAsync( new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();

                // if roles are not created, then we craate admin user as well
                Address myaddress = new Address {
                    Street = "My Street 323",
                    City = "The City",
                    State = "The Good State",
                    PostalCode = "23456-AB"
                };
                unitOfWork.Address.Add(myaddress);
                unitOfWork.Save();

                var adminEmail = "admin@test.com";
                await userManager.CreateAsync(new ApplicationUser {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "Me",
                    PhoneNumber = "123456789",
                    Address = myaddress
                 }, "Admin123!");

                ApplicationUser user = await unitOfWork.ApplicationUser.GetAsync(u => u.Email == adminEmail);
                await userManager.AddToRoleAsync(user, SD.Role_Admin);

            }

        }
    }
}