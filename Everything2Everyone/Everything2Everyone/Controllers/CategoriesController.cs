using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Everything2Everyone.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext DataBase;

        public CategoriesController(ApplicationDbContext context)
        {
            DataBase = context;
        }


        [HttpPost]
        public IActionResult New(Category categoryToBeInserted)
        {
            // TO DO: Check if there is another category with the name we want to insert
            if (ModelState.IsValid)
            {
                DataBase.Categories.Add(categoryToBeInserted);
                DataBase.SaveChanges();

                TempData["message"] = "Category added successfully.";
                return Redirect("/articles/index/filter-sort");
            }

            return Redirect("/Articles/Index/filter-sort");
        }


        public IActionResult Edit(int categoryID)
        {
            Category category;

            // making sure provided ID is valid
            try
            {
                category = DataBase.Categories.Where(category => category.CategoryID == categoryID).First();
            }
            catch
            {
                TempData["message"] = "No category with specified ID could be found.";
                return Redirect("/articles/index/filter-sort");
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(category);
        }


        [HttpPost]
        public IActionResult Edit(Category categoryToBeInserted)
        {
            // TO DO: Check if there is another category with the name we want to insert
            // If all validations were passed successfully
            if (ModelState.IsValid)
            {
                Category category = DataBase.Categories.Find(categoryToBeInserted.CategoryID);
                categoryToBeInserted.Title = categoryToBeInserted.Title;
                DataBase.SaveChanges();

                TempData["message"] = "Category edited successfully";
                return Redirect("/Articles/Index/filter-sort");
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(categoryToBeInserted);
        }


        [HttpPost]
        public IActionResult Delete(int categoryID)
        {
            try
            {
                Category category = DataBase.Categories.Find(categoryID);
                DataBase.Categories.Remove(category);
                DataBase.SaveChanges();
                TempData["Messages"] = "Category deleted successfully!";
            }
            catch
            {
                TempData["message"] = "No category with specified ID could be found.";
            }

            return Redirect("/Articles/Index/filter-sort");
        }


        // method which stores all categories into a viewbag item, in order to be shown in the side-menu partial view
        [NonAction]
        public void FetchCategories()
        {
            ViewBag.GlobalCategories = DataBase.Categories.OrderBy(category => category.Title);
        }
    }
}
