using Bulky.DataAccess.UoW;
using Bulky.Models;
using Bulky.Utility;
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
        public async Task<IActionResult> GetAll(string status)
        {
            IEnumerable<OrderHeader> orderList = await _unitOfWork.OrderHeader.GetAllAsync(null,includeProperties:"ApplicationUser");

            switch (status) {
                case "pending":
                orderList = orderList.Where(s => s.PaymentStatus == SD.PaymentStatusPending);
                break;
                case "inprocess":
                orderList = orderList.Where(s => s.OrderStatus == SD.StatusInProcess);
                break;
                case "completed":
                orderList = orderList.Where(s => s.OrderStatus == SD.StatusShipped);
                break;  
                case "approved":
                orderList = orderList.Where(s => s.OrderStatus == SD.StatusApproved);
                break;
                default:
                
                break;
            }

            return Json(new {data = orderList});
        }   

        #endregion
    }
}