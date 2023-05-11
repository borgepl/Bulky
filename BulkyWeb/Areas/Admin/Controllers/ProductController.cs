using System.Collections.Generic;
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.UoW;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ILogger<ProductController> logger, IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            
            _logger = logger;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> productList = (List<Product>) await _unitOfWork.Product.GetAllAsync(includeProperties:"Category");

            return View(productList);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            IReadOnlyList<Category> categoryList = await _unitOfWork.Category.GetAllAsync();
            IEnumerable<SelectListItem> categorySelectList = categoryList
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

            ProductVM productVM = new()
            {
                Product = new Product(),
                CategoryList = categorySelectList
            };

            if (id == null || id == 0)
            {
                // create new product
                return View(productVM);
            }
            else
            {
                // update existing product
                productVM.Product = await _unitOfWork.Product.GetByIdAsync((int)id);
                if (productVM.Product == null) return NotFound();
            
                return View(productVM);
            }

            
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(ProductVM productVM, IFormFile file )
        {

            if (ModelState.IsValid) 
            {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\product");

                if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                {
                    // delete old image
                    var oldImagePath = Path.Combine(wwwRootPath,productVM.Product.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using ( var filestream = new FileStream(Path.Combine(productPath, filename), FileMode.Create))
                {
                    file.CopyTo(filestream);
                }
                productVM.Product.ImageUrl = @"\images\product\" + filename;
            }

            if (productVM.Product.Id == 0) // new product
            {
                _unitOfWork.Product.Add(productVM.Product);
                TempData["success"]= "Product created successfully";
            }
            else 
            {
                _unitOfWork.Product.Update(productVM.Product);
                TempData["success"]= "Product updated successfully";
            }
            
            _unitOfWork.Save();
            return RedirectToAction("Index");

            }
            else 
            {
                IReadOnlyList<Category> categoryList = await _unitOfWork.Category.GetAllAsync();
                IEnumerable<SelectListItem> categorySelectList = categoryList
                    .Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    });

                    productVM.CategoryList = categorySelectList;

                    return View(productVM);
            }
            
           
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


        [HttpPost, ActionName("DeleteProd")]
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

        #region APICALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<Product> productList = (List<Product>) await _unitOfWork.Product.GetAllAsync(includeProperties:"Category");

            return Json(new {data = productList});
        }   

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            Product productToDelete = await _unitOfWork.Product.GetByIdAsync((int)id);
            if (productToDelete == null) return Json(new { success = false, message = "Error while deleting" });

            // delete old image
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            var oldImagePath = Path.Combine(wwwRootPath,productToDelete.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(productToDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Product deleted successfully" });
        }   


        #endregion
    }
}