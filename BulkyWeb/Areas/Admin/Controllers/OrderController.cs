using Bulky.DataAccess.UoW;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(ILogger<OrderController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
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
        public async Task<IActionResult> GetAll()
        {
            List<OrderHeader> orderList = (List<OrderHeader>) await _unitOfWork.OrderHeader.GetAllAsync(null,includeProperties:"ApplicationUser");

            return Json(new {data = orderList});
        }   

        #endregion
    }
}