using System.Diagnostics;
using API.Extensions;
using Bulky.DataAccess.UoW;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Customer.Controllers;

[Area("Customer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        IEnumerable<Product> productList = await _unitOfWork.Product.GetAllAsync(null,includeProperties:"Category");

        return View(productList);
    }

    public async Task<IActionResult> Details(int id)
    {
        Product product = await _unitOfWork.Product.GetAsync(u => u.Id == id, includeProperties:"Category");

        ShoppingCart shoppingCart = new() {
            Product = product,
            Count = 1,
            ProductId = product.Id
        };

        return View(shoppingCart);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Details(ShoppingCart shoppingCart)
    {

        var userId = User.GetUserId();
        shoppingCart.ApplicationUserId = userId;

        ShoppingCart cartFromDB = await _unitOfWork.ShoppingCart.GetAsync(u => u.ApplicationUserId == userId &&
            u.ProductId == shoppingCart.ProductId);

        if (cartFromDB != null) {
            // shopping cart exists - update cart
            cartFromDB.Count += shoppingCart.Count;
            _unitOfWork.ShoppingCart.Update(cartFromDB);

        } else {
            // add cart
            _unitOfWork.ShoppingCart.Add(shoppingCart);
        }

        
        _unitOfWork.Save();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
