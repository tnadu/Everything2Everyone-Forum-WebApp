using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;

namespace Everything2Everyone.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext DataBase;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CategoriesController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            DataBase = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public IActionResult New(Category categoryToBeInserted)
        {
            if (ModelState.IsValid)
            {
                var sanitizer = new HtmlSanitizer();
                categoryToBeInserted.Title = sanitizer.Sanitize(categoryToBeInserted.Title);

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


        [Authorize(Roles = "Administrator")]
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
        [Authorize(Roles = "Administrator")]
        public IActionResult Edit(Category categoryToBeInserted)
        {
            if (ModelState.IsValid)
            {
                Category category;

                category = DataBase.Categories.Find(categoryToBeInserted.CategoryID);

                if (category == null)
                {
                    TempData["ActionMessage"] = "No category with specified ID could be found.";
                }

                var sanitizer = new HtmlSanitizer();

                category.Title = sanitizer.Sanitize(categoryToBeInserted.Title);
                DataBase.SaveChanges();

                TempData["ActionMessage"] = "Category edited successfully";

                return Redirect("/articles/index/");
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(categoryToBeInserted);
        }


        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public IActionResult Delete(int categoryID)
        {
            Category category = DataBase.Categories.Find(categoryID);

            if (category == null)
            {
                TempData["ActionMessage"] = "No category with specified ID could be found.";
            }

            DataBase.Categories.Remove(category);
            DataBase.SaveChanges();
            TempData["ActionMessage"] = "Category successfully delete.";
            
            return Redirect("/articles/index/");
        }


        // method which stores all categories into a viewbag item,
        // in order to be displayed in the side-menu partial view
        [NonAction]
        public void FetchCategories()
        {
            ViewBag.GlobalCategories = DataBase.Categories.OrderBy(category => category.Title);
        }
    }
}
