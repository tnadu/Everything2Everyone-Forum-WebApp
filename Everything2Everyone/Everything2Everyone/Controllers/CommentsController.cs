using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

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
        public IActionResult Index()
        {
            FetchCategories();

            var comments = DataBase.Comments.Where(comment => comment.UserID == "fa1c312d-549a-42bd-8623-c1071cfd581e");
            //var comments = DataBase.Comments.Where(comment => comment.UserID == User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewBag.UserComments = comments;
            return View();
        }

        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int commentID)
        {
            Comment comment;

            // making sure specified ID is valid
            try
            {
                comment = DataBase.Comments.Where(comment => comment.CommentID == commentID).First();

                //if(comment.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
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

            // Fetch categories for side menu
            FetchCategories();
            // sending the article to the front-end, to prompt the user with
            // details about the article corresponding to the current comment
            ViewBag.ArticleOfComment = DataBase.Articles.Where(article => article.ArticleID == comment.ArticleID).First();

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

                    //if(comment.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
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

                TempData["ActionMessage"] = "Comment successfully edited.";
                return Redirect("/articles/show/" + comment.ArticleID);
            }

            // Fetch categories for side menu
            FetchCategories();
            // sending the article to the front-end, to prompt the user with
            // details about the article corresponding to the current comment
            ViewBag.ArticleOfComment = DataBase.Articles.Where(article => article.ArticleID == commentToBeInserted.ArticleID).First();

            return View(commentToBeInserted);
        }


        [HttpPost]
        public IActionResult Delete(int commentID)
        {
            // making sure specified ID is valid
            try
            {
                Comment commentToBeDeleted = DataBase.Comments.Find(commentID);

                //if(comment.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                // {
                //    TempData["ActionMessage"] = "You do not have permission to delete this comment!";
                //    return Redirect("/articles/show/" + comment.ArticleID);
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


        // method which stores all categories into a viewbag item,
        // in order to be displayed in the side-menu partial view
        [NonAction]
        public void FetchCategories()
        {
            ViewBag.GlobalCategories = DataBase.Categories.OrderBy(category => category.Title);
        }
    }
}