using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace Everything2Everyone.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext DataBase;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CommentsController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            DataBase = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult Index()
        {
            FetchCategories();

            var comments = DataBase.Comments.Where(comment => comment.UserID == User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (!comments.Any())
            {
                TempData["ActionMessage"] = "You haven't posted any comments yet.";
                return Redirect("/articles/index");
            }

            ViewBag.UserComments = comments;
            return View();
        }


        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult Edit(int commentID)
        {
            Comment comment;

            // making sure specified ID is valid
            try
            {
                comment = DataBase.Comments.Where(comment => comment.CommentID == commentID).First();

                // users can only modify their own comments
                if(comment.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                {
                    TempData["ActionMessage"] = "You do not have permission to edit this comment!";
                    return Redirect("/articles/show/" + comment.ArticleID);
                }

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
        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult Edit(Comment commentToBeInserted) 
        { 
            if (ModelState.IsValid)
            {
                Comment comment;
                // making sure specified ID is valid
                comment = DataBase.Comments.Find(commentToBeInserted.CommentID);

                if (comment == null)
                {
                    TempData["ActionMessage"] = "No comment with specified ID could be found.";
                    return Redirect("/articles/index/");
                }

                // users can only edit their own comments
                if(comment.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                {
                    TempData["ActionMessage"] = "You do not have permission to edit this comment!";
                    return Redirect("/articles/show/" + comment.ArticleID);
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
        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult Delete(int commentID)
        {
            // making sure specified ID is valid
            Comment commentToBeDeleted = DataBase.Comments.Find(commentID);

            if (commentToBeDeleted == null)
            {
                TempData["ActionMessage"] = "No comment with specified ID could be found.";
                return Redirect("/articles/index/");
            }

            if (!User.IsInRole("Administrator") && commentToBeDeleted.Article.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value && commentToBeDeleted.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                TempData["ActionMessage"] = "You do not have permission to delete this comment.";
                return Redirect("/articles/show/" + commentToBeDeleted.ArticleID);
            }

            DataBase.Comments.Remove(commentToBeDeleted);
            DataBase.SaveChanges();

            TempData["ActionMessage"] = "Comment successfully deleted.";
            return Redirect("/articles/show/" + commentToBeDeleted.ArticleID);
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