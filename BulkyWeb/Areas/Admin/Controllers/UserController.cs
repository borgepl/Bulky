using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bulky.Utility;
using Microsoft.Extensions.Logging;
using Bulky.DataAccess.UoW;
using Bulky.Models.Identity;
using Bulky.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ILogger<UserController> logger, IUnitOfWork unitOfWork, 
            ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Edit(string id)
        {
             string roleId = _dbContext.UserRoles.FirstOrDefault(u => u.UserId == id).RoleId;

            ApplicationUser user = _dbContext.ApplicationUsers.Include(u => u.Address).FirstOrDefault(u => u.Id == id);

            RoleManagementVM roleManagementVM = new()
            {
                ApplicationUser = user,
                RoleList = (IEnumerable<SelectListItem>)_dbContext.Roles
                    .Select(u => new SelectListItem
                        {
                            Text = u.Name,
                            Value = u.Name
                        })
            };
            roleManagementVM.ApplicationUser.Role = _dbContext.Roles.FirstOrDefault(u => u.Id == roleId).Name;

            return View(roleManagementVM);


        }

        [HttpPost]
        public IActionResult Edit(RoleManagementVM roleVM)
        {
            string roleId = _dbContext.UserRoles.FirstOrDefault(u => u.UserId == roleVM.ApplicationUser.Id).RoleId;
            string oldRole = _dbContext.Roles.FirstOrDefault(u => u.Id == roleId).Name;

            ApplicationUser userFromDB = _dbContext.ApplicationUsers.Find(roleVM.ApplicationUser.Id);

            if (!(roleVM.ApplicationUser.Role == oldRole)) {
                _userManager.RemoveFromRoleAsync(userFromDB, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(userFromDB, roleVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }

            if (ModelState.IsValid) 
            {
                userFromDB.Name = roleVM.ApplicationUser.Name;
                userFromDB.Email = roleVM.ApplicationUser.Email;
                _dbContext.ApplicationUsers.Update(userFromDB);
                _dbContext.SaveChanges();

                TempData["success"]= "User updated successfully";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }


        #region APICALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> userList = _dbContext.ApplicationUsers.Include(u => u.Address).ToList();

            var userRoles = _dbContext.UserRoles.ToList();
            var roles = _dbContext.Roles.ToList();

            foreach (var user in userList)
            {

                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                if (roleId != null) {
                    user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                } else {
                    user.Role = "-";
                }
                
                if (user.Address == null) {
                    user.Address = new() { City = ""};
                }
            }

            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var userFromDb = _dbContext.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (userFromDb == null) {
                return Json(new { success = false, message = "Error while locking or unlocking account" });
            }

            if (userFromDb.LockoutEnd != null && userFromDb.LockoutEnd > DateTime.Now ) {
                // user is currently locked and we will need to unlock
                userFromDb.LockoutEnd = DateTime.Now;
            } else {
                userFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _dbContext.SaveChanges();

            return Json(new { success = true, message = "Locked/unlocked successfully" });
        }

        #endregion

    }
}