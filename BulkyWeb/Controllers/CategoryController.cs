using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly ICategoryRepository _repo;

        public CategoryController(ILogger<CategoryController> logger, ICategoryRepository categoryRepository)
        {
            
            _logger = logger;
            _repo = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> objCategoryList = (List<Category>) await _repo.GetAllAsync();

            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if ( category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name","The Display Order cannot exactly match the Name.");
            }

            if ( category.Name != null && category.Name.ToLower() == "test")
            {
                ModelState.AddModelError("","The Category Name 'test' is invalid");
            }

            if (ModelState.IsValid) 
            {
            _repo.Add(category);
            _repo.Save();

            TempData["success"]= "Category created successfully";
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
            Category category = await _repo.GetByIdAsync(id);
            if (category == null) {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            
            if (ModelState.IsValid) 
            {
            _repo.Update(category);
            _repo.Save();

            TempData["success"]= "Category updated successfully";
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
            Category category = await _repo.GetAsync(u => u.Id == id);
            if (category == null) {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            Category category = await _repo.GetByIdAsync(id);
            if (category == null) 
            {
                return NotFound();
            }

            _repo.Remove(category);
            _repo.Save();

            TempData["success"]= "Category deleted successfully";
             return RedirectToAction("Index");
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}