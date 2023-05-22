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

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _dbContext;

        public UserController(ILogger<UserController> logger, IUnitOfWork unitOfWork, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
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