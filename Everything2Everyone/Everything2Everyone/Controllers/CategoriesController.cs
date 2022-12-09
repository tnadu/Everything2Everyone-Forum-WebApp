using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Mvc;

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
                return Redirect("/Articles/Index/filter-sort");
            }

            TempData["message"] = "Category is invalid: Title is required. Length must be between 5 and 30 characters.";
            return Redirect("/Articles/Index/filter-sort");
        }

        [HttpPost]
        public IActionResult Edit(Category categoryToBeInserted)
        {
            // TO DO: Check if there is another category with the name we want to insert
            // If all validations were passed successfully
            if (ModelState.IsValid)
            {
                Category DBcategory = DataBase.Categories.Find(categoryToBeInserted.CategoryID);
                DBcategory.Title = categoryToBeInserted.Title;
                DataBase.Categories.Add(DBcategory);
                DataBase.SaveChanges();

                TempData["message"] = "Category edited successfully!";
            }
            else
            {
                TempData["message"] = "Category is invalid: Title is required. Length must be between 5 and 30 characters.";
            }

            return Redirect("/Articles/Index/filter-sort");
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                Category category = DataBase.Categories.Find(id);
                DataBase.Categories.Remove(category);
                DataBase.SaveChanges();
                TempData["Messages"] = "Category deleted successfully!";
            }
            catch
            {
                TempData["message"] = "Category does not exist!";
            }

            return Redirect("/Articles/Index/filter-sort");
        }
    }
}
