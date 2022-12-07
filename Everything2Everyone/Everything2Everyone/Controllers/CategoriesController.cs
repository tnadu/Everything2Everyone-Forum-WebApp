using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Mvc;

namespace Everything2Everyone.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext DataBase;

        CategoriesController(ApplicationDbContext context)
        {
            DataBase = context;
        }

        [HttpPost]
        public IActionResult New(Category categoryToBeInserted)
        {
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


        //public IActionResult Edit(int categoryID)
        //{
        //    Category category

        //    return View(category);
        //}


        //[HttpPost]
        //public IActionResult Edit(Category categoryToBeInserted)
        //{

        //}


        //[HttpPost]
        //public IActionResult Delete(int id)
        //{

        //}
    }
}
