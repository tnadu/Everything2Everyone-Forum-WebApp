using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult MyComments()
        {
            // The user will be the user who is currently using the app => TO DO 
            var userID = "318d855d-4d7a-4b5e-a293-40720ca8faac";

            FetchCategories();

            try
            {
                var comments = DataBase.Comments.Where(comment => comment.UserID == userID);
                //  var comments = DataBase.Comments.Where(comment => comment.UserID == _userManager.GetUserById(User))
                ViewBag.UserComments = comments;
                return View();
            }
            catch
            {
                TempData["ActionMessage"] = "No user with specified ID could be found!";
                return Redirect("/articles/index");
            }
        }

        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int commentID)
        {
            Comment comment;

            // making sure specified ID is valid
            try
            {
                comment = DataBase.Comments.Where(comment => comment.CommentID == commentID).First();

                //if(comment.UserID != _userManager.GetUserById(User))
                // {
                //    TempData["ActionMessage"] = "You do not have permission to edit this comment!";
                //    return Redirect("/articles/show/" + comment.ArticleID);
                //}
                ViewBag.ArticleCommented = DataBase.Articles.Where(article => article.ArticleID == comment.ArticleID).First();

            }
            catch
            {
                TempData["ActionMessage"] = "No comment with specified ID could be found.";
                return Redirect("/articles/index/");
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(comment);
        }


        [HttpPost]
        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(Comment commentToBeInserted) 
        { 
            if (ModelState.IsValid)
            {
                Comment comment;
                // making sure specified ID is valid
                try
                {
                    comment = DataBase.Comments.Find(commentToBeInserted.CommentID);
                    //if(comment.UserID != _userManager.GetUserById(User))
                    // {
                    //    TempData["ActionMessage"] = "You do not have permission to edit this comment!";
                    //    return Redirect("/articles/show/" + comment.ArticleID);
                    //}
                }
                catch
                {
                    TempData["ActionMessage"] = "No comment with specified ID could be found.";
                    return Redirect("/articles/index/");
                }

                comment.Content = commentToBeInserted.Content;
                // comment got edited just now
                comment.DateEdited = DateTime.Now;
                DataBase.SaveChanges();

                return Redirect("/articles/show/" + comment.ArticleID);
            }

            // Fetch categories for side menu
            FetchCategories();
            // Complete the ViewBag again
            ViewBag.ArticleCommented = DataBase.Articles.Where(article => article.ArticleID == commentToBeInserted.ArticleID).First();

            return View(commentToBeInserted);
        }


        [HttpPost]
        public IActionResult Delete(int commentID)
        {
            // making sure specified ID is valid
            try
            {
                Comment commentToBeDeleted = DataBase.Comments.Find(commentID);

                //if(commentToBeDeleted.UserID != _userManager.GetUserById(User))
                // {
                //    TempData["ActionMessage"] = "You do not have permission to delete this comment!";
                //    return Redirect("/articles/show/" + commentToBeDeleted.ArticleID);
                //}

                DataBase.Comments.Remove(commentToBeDeleted);
                DataBase.SaveChanges();

                TempData["ActionMessage"] = "Comment successfully deleted.";
   
                return Redirect("/articles/show/" + commentToBeDeleted.ArticleID);
            }
            catch
            {
                TempData["ActionMessage"] = "No comment with specified ID could be found.";
                return Redirect("/articles/index/");
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