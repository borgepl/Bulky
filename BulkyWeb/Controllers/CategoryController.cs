using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly ApplicationDbContext _context;

        public CategoryController(ILogger<CategoryController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<Category> objCategoryList = _context.Categories.ToList();

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
            _context.Categories.Add(category);
            _context.SaveChanges();

            TempData["success"]= "Category created successfully";
             return RedirectToAction("Index");
            }
            return View();
           
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0 )
            {
                return NotFound();
            } 
            Category category = _context.Categories.Find(id);
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
            _context.Categories.Update(category);
            _context.SaveChanges();

            TempData["success"]= "Category updated successfully";
             return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0 )
            {
                return NotFound();
            } 
            Category category = _context.Categories.Find(id);
            if (category == null) {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Category category = _context.Categories.Find(id);
            if (category == null) 
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();

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