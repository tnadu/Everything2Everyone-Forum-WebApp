using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;

namespace Everything2Everyone.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext DataBase;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ArticlesController (ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            DataBase = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        // intermediary method intended to guide the user through the process
        // of choosing a version based on which to modify their article
        [Authorize(Roles = "Editor,Administrator")]
        public IActionResult ChooseVersion(int articleID)
        {
            // fetching current article version from the database
            Article currentArticleVersion;

            // making sure provided articleID is valid
            try
            {
                currentArticleVersion = DataBase.Articles.Include("Category").Where(articleVersion => articleVersion.ArticleID == articleID).First();
            }
            catch
            {
                TempData["ActionMessage"] = "No article with specified ID could be found.";
                return Redirect("/articles/index");
            }


            // user, editor authorization
            if (!User.IsInRole("Administrator") && currentArticleVersion.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                TempData["ActionMessage"] = "You don't have permission to access this resource.";
                return Redirect("/articles/show/" + articleID);
            }

            // admin authorization => fails when admin attempts to change another admin's article
            bool isTheRequestedUserAnAdmin = DataBase.UserRoles.Where(u => u.UserId == currentArticleVersion.UserID).Select(u => u.RoleId).First() ==
                                                DataBase.Roles.Where(r => r.Name == "Administrator").Select(r => r.Id).First();

            if (User.IsInRole("Administrator") && isTheRequestedUserAnAdmin && currentArticleVersion.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                TempData["ActionMessage"] = "You don't have permission to access this resource.";
                return Redirect("/articles/show/" + articleID);
            }


            // fetching all previous article versions from the database
            var articleVersions = DataBase.ArticleVersions.Include("Category").Where(articleVersion => articleVersion.ArticleID == articleID)
                                                                              .OrderByDescending(articleVersion => articleVersion.VersionID);
            // pagination
            int articlesPerPage = 10;
            int numberOfArticles = articleVersions.Count();
            var currentPageNumber = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var lastPage = Convert.ToInt32(Math.Ceiling((float)numberOfArticles / (float)articlesPerPage));

            // <=1st page: offset = 0
            // 2nd page: offset = 10
            // ...
            // >=last page: offset = 10 * (last page - 1)
            var offset = 0;
            if (currentPageNumber <= 1 || lastPage == 1 || lastPage == 0)
                ViewBag.CurrentArticleVersion = currentArticleVersion;
            else if (currentPageNumber >= lastPage)
                offset = (lastPage - 1) * articlesPerPage;
            else
                offset = (currentPageNumber - 1) * articlesPerPage;
            articleVersions = (IOrderedQueryable<ArticleVersion>)articleVersions.Skip(offset).Take(articlesPerPage);

            ViewBag.articleVersions = articleVersions;
            ViewBag.articleID = articleID;
            ViewBag.lastPage = lastPage;

            // Fetch categories for side menu
            FetchCategories();

            return View();
        }


        // when a request is issued, the 'IsRestricted' attribute
        // of the specified article is set to its opposite, acting
        // like an on/off switch
        [Authorize(Roles="Administrator")]
        public IActionResult Restrict(int articleID)
        {
            // Fetch categories for side menu
            FetchCategories();

            Article article;

            // making sure provided articleID is valid
            article = DataBase.Articles.Find(articleID);
            
            if (article == null)
            {
                TempData["ActionMessage"] = "No article with specified ID could be found.";
                return Redirect("/articles/index/");
            }

            // admin authorization => fails when admin attempts to change another admin's article
            bool isTheRequestedUserAnAdmin = DataBase.UserRoles.Where(u => u.UserId == article.UserID).Select(u => u.RoleId).First() ==
                                                DataBase.Roles.Where(r => r.Name == "Administrator").Select(r => r.Id).First();

            if (isTheRequestedUserAnAdmin && article.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                TempData["ActionMessage"] = "You don't have permission to access this resource.";
                return Redirect("/articles/show/" + articleID);
            }

            article.IsRestricted = ! article.IsRestricted;
            DataBase.SaveChanges();

            return Redirect("/articles/show/" + articleID);
        }


        // Index method which returns the list of published articles filtered by category and
        // sorted chronologically/alphabetically. Under the user-specific-mode, only the articles
        // belonging to the user making the request are returned. All parameters are optional.
        // It is accesibile only to signed-in users of all kinds.
        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult Index(int? categoryID, int? sort, int? userSpecificMode)
        {
            // Fetch categories for side menu
            FetchCategories();

            // query returns a list of all the articles in the database
            var returnedArticles = DataBase.Articles.Include("Category").Include("Chapters").Include("User");

            // filter - category is specified
            ViewBag.CategoryName = null;
            if (categoryID != null)
            {
                // making sure provided categoryID is valid
                returnedArticles = returnedArticles.Where(article => article.CategoryID == categoryID);

                // when provided categoryID is invalid, all articles are returned
                if (!returnedArticles.Any())
                    returnedArticles = DataBase.Articles.Include("Category").Include("Chapters").Include("User");
                else
                    ViewBag.CategoryName = DataBase.Categories.Where(category => category.CategoryID == categoryID).Select(c => c.Title).First();
            }

            // sort (if specified)
            if (sort == 0)
                returnedArticles = returnedArticles.OrderBy(article => article.PublicationDate);
            else if (sort == 1)
                returnedArticles = returnedArticles.OrderByDescending(article => article.PublicationDate);
            else if (sort == 2)
                returnedArticles = returnedArticles.OrderBy(article => article.Title);
            else if (sort == 3)
                returnedArticles = returnedArticles.OrderByDescending(article => article.Title);    

            // filter articles by the user making the request
            if (userSpecificMode != null)
                returnedArticles = returnedArticles.Where(article => article.UserID == User.FindFirst(ClaimTypes.NameIdentifier).Value);


            // pagination
            int articlesPerPage = 10;
            int numberOfArticles = returnedArticles.Count();
            var currentPageNumber = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var lastPage = Convert.ToInt32(Math.Ceiling((float)numberOfArticles / (float)articlesPerPage));

            // <=1st page: offset = 0
            // 2nd page: offset = 10
            // ...
            // >=last page: offset = 10 * (last page - 1)
            var offset = 0;
            if (lastPage > 1)
            {
                if (currentPageNumber >= lastPage)
                    offset = (lastPage - 1) * articlesPerPage;
                else if (currentPageNumber > 1)
                    offset = (currentPageNumber - 1) * articlesPerPage;
            }
            returnedArticles = returnedArticles.Skip(offset).Take(articlesPerPage);

            ViewBag.CurrentArticleQuery = returnedArticles;
            ViewBag.CategoryID = categoryID;
            ViewBag.Sorting = sort;
            ViewBag.UserSpecified = userSpecificMode;
            ViewBag.lastPage = lastPage;

            // message received
            if (TempData.ContainsKey("ActionMessage"))
            {
                ViewBag.DisplayedMessage = TempData["ActionMessage"];
            }

            return View();
        }


        // Method which returns the specified article's content and details, along with its
        // associated comments, category and user. From this view, the restrict, edit and
        // delete actions are accessible
        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult Show(int articleID)
        {
            // Fetch categories for side menu
            FetchCategories();

            Article returnedArticle;
            List<Chapter> returnedChapters = new List<Chapter>();

            // making sure provided articleID is valid
            try
            {
                returnedArticle = DataBase.Articles.Include("Category").Include("Chapters")
                                                   .Include("Comments").Include("User").Where(article => article.ArticleID == articleID).First();

                returnedChapters = DataBase.Chapters.Where(returnedArticle => returnedArticle.ArticleID == articleID).ToList();
            }
            catch
            {
                TempData["ActionMessage"] = "No article with specified ID could be found.";
                return Redirect("/articles/index/");
            }

            // message received
            if (TempData.ContainsKey("ActionMessage"))
            {
                ViewBag.DisplayedMessage = TempData["ActionMessage"];
            }

            // mechanism to show/hide edit-delete-restrict buttons on article and comments depending on the user who made the request           
            bool isTheRequestedUserAnAdmin = DataBase.UserRoles.Where(u => u.UserId == returnedArticle.UserID).Select(u => u.RoleId).First() ==
                                                DataBase.Roles.Where(r => r.Name == "Administrator").Select(r => r.Id).First();

            ViewBag.ShowEditDeleteButtons = (!User.IsInRole("Administrator") && returnedArticle.UserID == User.FindFirst(ClaimTypes.NameIdentifier).Value) ||
                (User.IsInRole("Administrator") && !(isTheRequestedUserAnAdmin && returnedArticle.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value));

            ViewBag.ShowRestrictButton = ViewBag.ShowEditDeleteButtons && User.IsInRole("Administrator");


            ArticleBundle returnedArticleBundle = new ArticleBundle();
            returnedArticleBundle.Article = returnedArticle;
            returnedArticleBundle.Chapters = returnedChapters;

            // Fetch categories for side menu
            FetchCategories();

            return View(returnedArticleBundle);
        }


        // because Views accept only one Model, we will use the POST Show method to add comments
        // to the corresponding article
        [HttpPost]
        // [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult Show([FromForm] Comment commentToBeInserted)
        {
            commentToBeInserted.UserID = "fa1c312d-549a-42bd-8623-c1071cfd581e";
            // commentToBeInserted.UserID = User.FindFirst(ClaimTypes.NameIdentifier).Value
            // at first, the dates are identical
            commentToBeInserted.DateAdded = DateTime.Now;
            commentToBeInserted.DateEdited = DateTime.Now;

            if (ModelState.IsValid)
            {
                DataBase.Comments.Add(commentToBeInserted);
                DataBase.SaveChanges();
                TempData["ActionMessage"] = "Comment added successfully.";
            }

            return Redirect("/articles/show/" + commentToBeInserted.ArticleID);
        }


        // action returning the associated View
        // [Authorize(Roles = "Editor,Administrator")]
        public IActionResult New()
        {
            // Fetch categories for side menu
            FetchCategories();

            ArticleBundle articleToBeInserted = new ArticleBundle();
            articleToBeInserted.Article = new Article();
            articleToBeInserted.Categories = StoreCategories();

            // message received
            if (TempData.ContainsKey("ActionMessage"))
            {
                ViewBag.DisplayedMessage = TempData["ActionMessage"];
            }

            return View(articleToBeInserted);
        }


        // action inserting the new article and its associated
        // chapters into the database
        [HttpPost]
        // [Authorize(Roles = "Editor,Administrator")]
        public IActionResult New(ArticleBundle articleBundle)
        {
            // Fetch categories for side menu
            FetchCategories();

            // message received
            if (TempData.ContainsKey("ActionMessage"))
            {
                ViewBag.DisplayedMessage = TempData["ActionMessage"];
            }

            // when article is firstly published, both dates are identical
            articleBundle.Article.PublicationDate = DateTime.Now;
            articleBundle.Article.CommitDate = DateTime.Now;
            // and it is not restricted by an administrator
            articleBundle.Article.IsRestricted = false;
            articleBundle.Article.UserID = User.FindFirst(ClaimTypes.NameIdentifier).Value; 

            // provided article and chapters are valid
            if (ModelState.IsValid)
            {
                if (articleBundle.Chapters.Count == 0)
                {
                    articleBundle.Categories = StoreCategories();
                    TempData["ActionMessage"] = "An article must contain at least one chapter";
                    return View(articleBundle);
                }

                DataBase.Articles.Add(articleBundle.Article);
                DataBase.SaveChanges();

                // select the article with the biggest ID, because that will be
                // the last article inserted in the Database,
                // which is the article inserted above
                int ArticleCreatedID = (from article in DataBase.Articles
                                        orderby article.ArticleID descending
                                        select article.ArticleID).First();

                foreach (Chapter chapter in articleBundle.Chapters)
                {
                    chapter.ArticleID = ArticleCreatedID;
                    DataBase.Chapters.Add(chapter);
                    DataBase.SaveChanges();
                }

                TempData["ActionMessage"] = "Article added successfully.";
                return Redirect("/articles/show/" + articleBundle.Article.ArticleID);
            }
            // provided article and chapters are invalid, so the 'New' View
            // is returned and populated with the said article and chapters

            articleBundle.Categories = StoreCategories();
            return View(articleBundle);
        }


        // [Authorize(Roles = "Editor,Administrator")]
        public IActionResult Edit(int articleID, int versionID)
        {
            // the most recent version of the article is required, in order
            // the authorize the user who attempts to edit any other version
            // (access to IsRestricted is necessary);
            // the category of said article will also be requested, in case the
            // requested version is indeed the most recent one
            Article article;
            // making sure provided articleID is valid
            try
            {
                article = DataBase.Articles.Include("Category").Where(article => article.ArticleID == articleID).First();
            }
            catch
            {
                TempData["ActionMessage"] = "No article with specified ID could be found.";
                return Redirect("/articles/index?userSpecificMode=1");
            }

            //Authorize(article.UserID, articleID, article.IsRestricted);


            ArticleVersionBundle articleVersionBundle = new ArticleVersionBundle();
            articleVersionBundle.Article = new ArticleVersion();

            // the request involves editing a version other than the most recent version of the article with the provided articleID
            if (versionID != -1)
            {
                // making sure provided versionID is valid
                try
                {
                    // storing chosen article version into the bundled object
                    articleVersionBundle.Article = DataBase.ArticleVersions.Include("Category").Where(article => article.ArticleID == articleID)
                                                                                    .Where(article => article.VersionID == versionID).First();
                }
                catch
                {
                    TempData["ActionMessage"] = "No version with specified version ID could be found.";
                    return Redirect("/articles/index?userSpecificMode=1");
                }

                // storing chosen article's chapters in to bundled object
                articleVersionBundle.Chapters = DataBase.ChapterVersions.Where(chapter => chapter.ArticleID == articleID)
                                                                        .Where(chapter => chapter.VersionID == versionID).ToList();
            }
            // the request involves editing the most recent version of the article with the provided articleID
            else
            {
                // copying values into the newly created 'ArticleVersionBundle' object
                articleVersionBundle.Article.ArticleID = article.ArticleID;
                articleVersionBundle.Article.CategoryID = article.CategoryID;
                articleVersionBundle.Article.Title = article.Title;
                articleVersionBundle.Article.CommitTitle = article.CommitTitle;
                articleVersionBundle.Article.CommitDate = article.CommitDate;

                articleVersionBundle.Chapters = new List<ChapterVersion>();
                // since the chapters associated with the most recent version of the specified article
                // are stored in the CHAPTERS table, each chapter's information must be inserted sequentially
                // into the 'ArticleVersionBundle' object
                foreach (Chapter chapter in DataBase.Chapters.Where(chapter => chapter.ArticleID == articleID))
                {
                    ChapterVersion chapterVersion = new ChapterVersion();
                    chapterVersion.ChapterID = chapter.ChapterID;
                    chapterVersion.ArticleID = chapter.ArticleID;
                    chapterVersion.Title = chapter.Title;
                    chapterVersion.ContentUnparsed = chapter.ContentUnparsed;
                    chapterVersion.ContentParsed = chapter.ContentParsed;

                    // adding the chapter into the bundled object
                    articleVersionBundle.Chapters.Add(chapterVersion);
                }
            }

            articleVersionBundle.Categories = StoreCategories();

            // message received
            if (TempData.ContainsKey("ActionMessage"))
            {
                ViewBag.DisplayedMessage = TempData["ActionMessage"];
            }

            // Fetch categories for side menu
            FetchCategories();

            // storing categories which will be sent in the front
            // end for the dropdown selector
            articleVersionBundle.Categories = StoreCategories();

            return View(articleVersionBundle);
        }


        [HttpPost]
        // [Authorize(Roles = "Editor,Administrator")]
        public IActionResult Edit(ArticleVersionBundle articleVersionBundle)
        {
            // Fetch categories for side menu
            FetchCategories();

            Article currentArticle;

            // making sure provided bundled object stores a valid articleID
            try
            {
                currentArticle = DataBase.Articles.Include("Chapters").Where(a => a.ArticleID == articleVersionBundle.Article.ArticleID).First();
            }
            catch
            {
                TempData["ActionMessage"] = "No article with specified ID could be found.";
                return Redirect("/articles/index/");
            }

            //Authorize(currentArticle.UserID, currentArticle.ArticleID, currentArticle.IsRestricted);

            if (ModelState.IsValid)
            {
                // there must be at least one chapter associated with every article
                if (articleVersionBundle.Chapters == null)
                {
                    articleVersionBundle.Chapters = new List<ChapterVersion>();
                    articleVersionBundle.Categories = StoreCategories();

                    TempData["ActionMessage"] = "An article must contain at least one chapter";
                    return View(articleVersionBundle);
                }

                // copying the previous version of the article and the versions
                // of all chapters associated with it into the ARTICLE_VERSIONS
                // and CHAPTER_VERSIONS tables
                ArticleVersion oldArticleToBeInserted = new ArticleVersion();
                oldArticleToBeInserted.ArticleID = currentArticle.ArticleID;
                oldArticleToBeInserted.CategoryID = currentArticle.CategoryID;
                oldArticleToBeInserted.Title = currentArticle.Title;
                oldArticleToBeInserted.CommitTitle = currentArticle.CommitTitle;
                oldArticleToBeInserted.CommitDate = currentArticle.CommitDate;

                // previously most recent article version successfully stored in ARTICLE_VERSIONS
                DataBase.ArticleVersions.Add(oldArticleToBeInserted);
                DataBase.SaveChanges();

                // computing the currentVersionID, based on entries in ARTICLE_VERSIONS
                int currentVersionID = (from articleVersion in DataBase.ArticleVersions
                                       where articleVersion.ArticleID == articleVersionBundle.Article.ArticleID
                                       select articleVersion.VersionID).Max();

                // processing each previously most recent chapter
                foreach (Chapter oldChapter in currentArticle.Chapters)
                {
                    ChapterVersion oldChapterToBeInserted = new ChapterVersion();
                    oldChapterToBeInserted.ChapterID = oldChapter.ChapterID;
                    oldChapterToBeInserted.ArticleID = oldChapter.ArticleID;
                    oldChapterToBeInserted.VersionID = currentVersionID;
                    oldChapterToBeInserted.Title = oldChapter.Title;
                    oldChapterToBeInserted.ContentUnparsed = oldChapter.ContentUnparsed;
                    oldChapterToBeInserted.ContentParsed = oldChapter.ContentParsed;

                    // on of the previously most recent chapter successfully stored in the database
                    DataBase.ChapterVersions.Add(oldChapterToBeInserted);
                    DataBase.SaveChanges();

                    // removing chapters associated with previous article version,
                    // to allow for the currently most recent chapter versions to replace them
                    DataBase.Chapters.Remove(oldChapter);
                    DataBase.SaveChanges();
                }


                // overwriting current article version, based on the 'ArticleVersionBundle' received through the form
                currentArticle.CategoryID = articleVersionBundle.Article.CategoryID;
                currentArticle.Title = articleVersionBundle.Article.Title;
                currentArticle.CommitTitle = articleVersionBundle.Article.CommitTitle;
                currentArticle.CommitDate = DateTime.Now;

                DataBase.SaveChanges();

                // storing current chapter versions, based on the 'ArticleVersionBundle' received through the form
                foreach (ChapterVersion newChapterVersion in articleVersionBundle.Chapters)
                {
                    Chapter newChapter = new Chapter();
                    newChapter.ChapterID = newChapterVersion.ChapterID;
                    newChapter.ArticleID = newChapterVersion.ArticleID;
                    newChapter.Title = newChapterVersion.Title;
                    newChapter.ContentUnparsed = newChapterVersion.ContentUnparsed;
                    newChapter.ContentParsed = newChapterVersion.ContentParsed;

                    DataBase.Chapters.Add(newChapter);
                    DataBase.SaveChanges();
                }

                TempData["ActionMessage"] = "Article edited successfully.";
                return Redirect("/articles/show/" + articleVersionBundle.Article.ArticleID);
            }

            // bundled object wasn't valid, so it is sent back
            // to the view for the necessary modifications
            articleVersionBundle.Categories = StoreCategories();
            return View(articleVersionBundle);
        }


        [HttpPost]
        // [Authorize(Roles = "Editor,Administrator")]
        public IActionResult Delete(int articleID)
        {
            Article article;

            // making sure provided articleID is valid
            article = DataBase.Articles.Find(articleID);
            
            if (article == null)
            {
                TempData["ActionMessage"] = "No article with specified ID could be found.";
                return Redirect("/articles/index/my-articles");
            }

            //Authorize(article.UserID, articleID, article.IsRestricted);

            DataBase.Articles.Remove(article);
            DataBase.SaveChanges();

            TempData["ActionMessage"] = "Article deleted successfully.";

            // when an Administrator deletes an article of a user other
            // than himself, he is redirected to the Index page
            // if (article.UserId != User.FindFirst(ClaimTypes.NameIdentifier).Value) {
            //      return Redirect("/articles/index");
            // }

            return Redirect("/articles/index?userSpecificMode=1");
        }


        // method which fetches the key-value pairs of all stored categories (id
        // and title) and returns a list of 'SelectListItem' objects based on them
        [NonAction]
        public IEnumerable<SelectListItem> StoreCategories()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            foreach (var category in DataBase.Categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryID.ToString(),
                    Text = category.Title.ToString()
                });
            }

            return selectList;
        }


        // method which stores all categories into a viewbag item, in order to be shown in the side-menu partial view
        [NonAction]
        public void FetchCategories()
        {
            ViewBag.GlobalCategories = DataBase.Categories.OrderBy(category => category.Title);
        }
    }
}
