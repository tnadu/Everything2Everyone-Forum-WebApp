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

                TempData["ActionMessage"] = "Category added successfully.";
            }
            else
            {
                TempData["ActionMessage"] = "Title is required. Length must be between 5 and 30 characters.";
            }

            return Redirect("/articles/index/");
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
                TempData["ActionMessage"] = "No category with specified ID could be found.";
                return Redirect("/articles/index/");
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
                Category category;

                try
                {
                    category = DataBase.Categories.Find(categoryToBeInserted.CategoryID);
                    category.Title = categoryToBeInserted.Title;
                    DataBase.SaveChanges();

                    TempData["ActionMessage"] = "Category edited successfully";
                }
                catch
                {
                    TempData["ActionMessage"] = "No category with specified ID could be found.";
                }

                return Redirect("/articles/index/");
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
                TempData["ActionMessage"] = "Category deleted successfully!";
            }
            catch
            {
                TempData["ActionMessage"] = "No category with specified ID could be found.";
            }

            return Redirect("/articles/index/");
        }


        // method which stores all categories into a viewbag item, in order to be shown in the side-menu partial view
        [NonAction]
        public void FetchCategories()
        {
            ViewBag.GlobalCategories = DataBase.Categories.OrderBy(category => category.Title);
        }
    }
}
