using API.Extensions;
using Bulky.DataAccess.UoW;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM orderVM { get; set; }
        public OrderController(ILogger<OrderController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int orderId)
        {
            orderVM = new() {
                OrderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderId, includeProperties:"ApplicationUser"),
                OrderDetail = await _unitOfWork.OrderDetail.GetAllAsync(u => u.OrderHeaderId == orderId, includeProperties:"Product")
            };

            return View(orderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> UpdateOrderDetails()
        {
           var orderHeaderFromDb = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderVM.OrderHeader.Id);

            orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.Street = orderVM.OrderHeader.Street;
            orderHeaderFromDb.City = orderVM.OrderHeader.City;
            orderHeaderFromDb.State = orderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier)) {
                orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber)) {
                orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order Detail updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();

             TempData["success"] = "Order Detail updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> ShipOrder()
        {
            var orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderVM.OrderHeader.Id);
            orderHeader.Carrier = orderVM.OrderHeader.Carrier;
            orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

             TempData["success"] = "Order shipped successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> CancelOrder()
        {
            var orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == orderVM.OrderHeader.Id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved) {

                // payment already done - give refund
                var options = new RefundCreateOptions {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId, 
                };

                var service = new RefundService();
                Refund refund = service.CreateAsync(options).GetAwaiter().GetResult();

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);

            } else {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            _unitOfWork.Save();

            TempData["success"] = "Order cancelled successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id});
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
            IEnumerable<OrderHeader> orderList;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee)) {
                orderList = await _unitOfWork.OrderHeader.GetAllAsync(null,includeProperties:"ApplicationUser");
            } else {
                var userId = User.GetUserId();
                 orderList = await _unitOfWork.OrderHeader
                    .GetAllAsync(u => u.ApplicationUserId == userId,includeProperties:"ApplicationUser");
            }

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