using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Everything2Everyone.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext DataBase;

        public CommentsController(ApplicationDbContext context)
        {
            DataBase = context;
        }

        public IActionResult Edit(int commentID)
        {
            Comment comment;

            // making sure specified ID is valid
            try
            {
                comment = DataBase.Comments.Where(comment => comment.CommentID == commentID).First();

            }
            catch
            {
                TempData["ActionMessage"] = "No comment with specified ID could be found.";
                return Redirect("/articles/index/filter-sort");
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(comment);
        }


        [HttpPost]
        public IActionResult Edit(Comment commentToBeInserted) 
        { 
            if (ModelState.IsValid)
            {
                Comment comment;
                // making sure specified ID is valid
                try
                {
                    comment = DataBase.Comments.Find(commentToBeInserted.CommentID);
                }
                catch
                {
                    TempData["ActionMessage"] = "No comment with specified ID could be found.";
                    return Redirect("/articles/index/filter-sort");
                }

                comment.Content = commentToBeInserted.Content;
                // comment got edited just now
                comment.DateEdited = DateTime.Now;
                DataBase.SaveChanges();

                return Redirect("/articles/show/" + comment.ArticleID);
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(commentToBeInserted);
        }


        [HttpPost]
        public IActionResult Delete(int commentID)
        {
            // making sure specified ID is valid
            try
            {
                Comment commentToBeDeleted = DataBase.Comments.Find(commentID);
                DataBase.Comments.Remove(commentToBeDeleted);
                DataBase.SaveChanges();

                TempData["ActionMessage"] = "Comment successfully deleted.";
   
                return Redirect("/articles/show" + commentToBeDeleted.ArticleID);
            }
            catch
            {
                TempData["ActionMessage"] = "No comment with specified ID could be found.";
                return Redirect("/articles/index/filter-sort");
            }
        }


        // method which stores all categories into a viewbag item, in order to be shown in the side-menu partial view
        [NonAction]
        public void FetchCategories()
        {
            ViewBag.GlobalCategories = DataBase.Categories.OrderBy(category => category.Title);
        }
    }
}