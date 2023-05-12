using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;

namespace Bulky.DataAccess.Data
{
    public class SeedRoles
    {
         public static Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult()) {

                roleManager.CreateAsync( new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                roleManager.CreateAsync( new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                roleManager.CreateAsync( new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                roleManager.CreateAsync( new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
            }

            return Task.CompletedTask;
        }
    }
}