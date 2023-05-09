using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.UoW;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(ILogger<ProductController> logger, IUnitOfWork unitOfWork)
        {
            
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> productList = (List<Product>) await _unitOfWork.Product.GetAllAsync();

            return View(productList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {

            if (ModelState.IsValid) 
            {
            _unitOfWork.Product.Add(product);
            _unitOfWork.Save();

            TempData["success"]= "Product created successfully";
             return RedirectToAction("Index");
            }
            return View();
           
        }

        public async Task<IActionResult> Edit(int id)
        {
            if ( id == 0 )
            {
                return NotFound();
            } 
            Product product = await _unitOfWork.Product.GetByIdAsync(id);
            if (product == null) {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            
            if (ModelState.IsValid) 
            {
            _unitOfWork.Product.Update(product);
            _unitOfWork.Save();

            TempData["success"]= "Product updated successfully";
             return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0 )
            {
                return NotFound();
            } 
            Product product = await _unitOfWork.Product.GetAsync(u => u.Id == id);
            if (product == null) {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            Product product = await _unitOfWork.Product.GetByIdAsync(id);
            if (product == null) 
            {
                return NotFound();
            }

            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();

            TempData["success"]= "Product deleted successfully";
             return RedirectToAction("Index");
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}