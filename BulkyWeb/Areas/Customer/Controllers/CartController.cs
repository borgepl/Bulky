using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;
using Bulky.DataAccess.UoW;
using Bulky.Utility;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Bulky.Models.Identity;
using Stripe.Checkout;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShoppingCartVM shoppingCartVM {get; set;}
        public CartController(ILogger<CartController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.GetUserId();

            shoppingCartVM = new() {
                ShoppingCartList = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == userId, 
                    includeProperties:"Product"),
                OrderHeader = new()
            };

            foreach (var cart in shoppingCartVM.ShoppingCartList) {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartVM);
        }

        public async Task<ActionResult> Summary()
        {
            var userId = User.GetUserId();

            shoppingCartVM = new() {
                ShoppingCartList = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == userId, 
                    includeProperties:"Product"),
                OrderHeader = new()
            };

            shoppingCartVM.OrderHeader.ApplicationUser = await _unitOfWork.ApplicationUser.GetAsync(u => u.Id == userId, includeProperties:"Address");

            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.Street = shoppingCartVM.OrderHeader.ApplicationUser.Address.Street;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.Address.City;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.Address.State;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.Address.PostalCode;

             foreach (var cart in shoppingCartVM.ShoppingCartList) {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
         public async Task<ActionResult> SummaryPost()
        {
            var userId = User.GetUserId();

            shoppingCartVM.ShoppingCartList = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == userId, 
                    includeProperties:"Product");

            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = await _unitOfWork.ApplicationUser.GetAsync(u => u.Id == userId);

             foreach (var cart in shoppingCartVM.ShoppingCartList) {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

           /*  if (applicationUser.CompanyId.GetValueOrDefault() == 0) {
                // it is a regular customer account
            }   SetStatusForCustomer();
            
            else {
                // it is a company user
                 shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            } */

            SetStatusForCustomer();

            // create orderHeader is DB
            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            // create OrderDetail in DB
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new() {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            // stripe logic for customer account
            var domain = "https://localhost:5001/";
			var options = new SessionCreateOptions 
                {
					SuccessUrl = domain+ $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain+"customer/cart/index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

                foreach(var item in shoppingCartVM.ShoppingCartList) {
                    var sessionLineItem = new SessionLineItemOptions {
                        PriceData = new SessionLineItemPriceDataOptions {
                            UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                            Currency = "eur",
                            ProductData = new SessionLineItemPriceDataProductDataOptions {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }


				var service = new SessionService();
				Session session = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStripePaymentID(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                
                return new StatusCodeResult(303);
        
            //return RedirectToAction(nameof(OrderConfirmation), new { id = shoppingCartVM.OrderHeader.Id});
        }

        private void SetStatusForCustomer()
        {
            shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            OrderHeader orderHeader = await _unitOfWork.OrderHeader.GetAsync(u => u.Id == id, 
                includeProperties:"ApplicationUser");

            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment) {

                 var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid") {
					_unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
				}
                //HttpContext.Session.Clear();

                
            }

            // _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book",
            //     $"<p>New Order Created - {orderHeader.Id}</p>");

            // Delete shoppingCarts when order and payment are confirmed

            IReadOnlyList<ShoppingCart> shoppingCarts = await _unitOfWork.ShoppingCart
                .GetAllAsync(u => u.ApplicationUserId == orderHeader.ApplicationUserId);

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();

            return View(id);
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cartFromDb = await _unitOfWork.ShoppingCart.GetByIdAsync(cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

         public async Task<IActionResult> Minus(int cartId)
        {
            var cartFromDb = await _unitOfWork.ShoppingCart.GetByIdAsync(cartId);
            if (cartFromDb.Count <= 1) {
                // remove from cart
                // remove item from session
                 IReadOnlyList<ShoppingCart> cartsFromDbForUser = await _unitOfWork.ShoppingCart
                    .GetAllAsync(u => u.ApplicationUserId == cartFromDb.ApplicationUserId);
                HttpContext.Session.SetInt32(SD.SessionCart, cartsFromDbForUser.Count()-1);
                _unitOfWork.ShoppingCart.Remove(cartFromDb);

            } else {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            
            _unitOfWork.Save();
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cartFromDb = await _unitOfWork.ShoppingCart.GetByIdAsync(cartId);
              
            // remove from cart
             _unitOfWork.ShoppingCart.Remove(cartFromDb);
            
            // remove item from session
            IReadOnlyList<ShoppingCart> cartsFromDbForUser = await _unitOfWork.ShoppingCart.GetAllAsync(u => u.ApplicationUserId == cartFromDb.ApplicationUserId);
            HttpContext.Session.SetInt32(SD.SessionCart, cartsFromDbForUser.Count()-1);
            
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart )
        {
            if (shoppingCart.Count <= 50) {
                return shoppingCart.Product.Price;
            } else {
                if (shoppingCart.Count <= 100) {
                    return shoppingCart.Product.Price50;
                }
                else {
                    return shoppingCart.Product.Price100;
                }
            }
            
        }
    }
}