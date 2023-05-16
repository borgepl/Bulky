using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;
using Bulky.DataAccess.UoW;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly IUnitOfWork _unitOfWork;

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