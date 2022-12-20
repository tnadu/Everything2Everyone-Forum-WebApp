using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Everything2Everyone.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext DataBase;

        public ArticlesController (ApplicationDbContext context)
        {
            DataBase = context;
        }


        // intermediary method intended to guide the user through the process
        // of choosing a version based on which to modify their article
        public IActionResult ChooseVersion(int articleID)
        {
            // Fetch categories for side menu
            FetchCategories();

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
                return Redirect("/Articles/Index/filter-sort");
            }

            // fetching all previous article versions from the database
            var articleVersions = DataBase.ArticleVersions.Include("Category").Where(articleVersion => articleVersion.ArticleID == articleID)
                                                                              .OrderBy(articleVersion => articleVersion.VersionID);
            ViewBag.articleVersions = articleVersions;
            return View(currentArticleVersion);
        }


        // when a request is issued, the 'IsRestricted' attribute
        // of the specified article is set to its opposite, acting
        // like an on/off switch
        public IActionResult Restrict(int articleID)
        {
            // Fetch categories for side menu
            FetchCategories();

            Article article;

            // making sure provided articleID is valid
            try
            {
                article = DataBase.Articles.Find(articleID);
            }
            catch
            {
                TempData["ActionMessage"] = "No article with specified ID could be found.";
                return Redirect("/Articles/Index/filter-sort");
            }

            article.IsRestricted = ! article.IsRestricted;
            DataBase.SaveChanges();

            return Redirect("/Articles/Show/" + articleID);
        }


        // Index method which returns most recently published articles, when category is unspecified.
        // When category is specified, the list of articles can be either unsorted or sorted
        // (chronologically/reverse chronologically/alphabetically/reverse alphabetically).
        // It is accesibile only to signed-in users of all kinds.
        
        public IActionResult Index(int? categoryID, int? sort)
        {
            // Fetch categories for side menu
            FetchCategories();

            // query returns a list of all the articles in the database
            var returnedArticles = DataBase.Articles.Include("Category").Include("Chapters"); // Include("Users");

            // filter - category is specified
            if (categoryID != null)
            {
                // making sure provided categoryID is valid
                try
                {
                    returnedArticles = returnedArticles.Where(article => article.CategoryID == categoryID);
                }
                // when provided categoryID is invalid, all articles are returned
                catch
                {
                    returnedArticles = DataBase.Articles.Include("Category").Include("Chapters");
                }
            }

            // sort (if specified)
            if (sort == 0)
            {
                returnedArticles = returnedArticles.OrderBy(article => article.PublicationDate);
            }
            else if (sort == 1)
            {
                returnedArticles = returnedArticles.OrderByDescending(article => article.PublicationDate);
            }
            else if (sort == 2)
            {
                returnedArticles = returnedArticles.OrderBy(article => article.Title);
            }
            else if (sort == 3)
            {
                returnedArticles = returnedArticles.OrderByDescending(article => article.Title);
            }
            // when invalid or null sort value is provided, the default behaviour occurs
            else
            {
                returnedArticles = returnedArticles.OrderByDescending(article => article.PublicationDate);
            }

            // FOR PAGINATION
            /////////////////
            // chose how much articles we want to display
            int _articlesPerPage = 3;
            // because the number of articles is variable, we need to check how many exist
            int totalArticles = returnedArticles.Count();
            // take the current page of articles from the View
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            // 0th page: offset = 0
            // 1st page: offset = 10
            // 2nd page: offset = 20
            // offset = number of articles already displayed
            var offset = 0;
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _articlesPerPage;
            }

            // take the corresponding articles depending on what page are we on
            var paginatedArticles = returnedArticles.Skip(offset).Take(_articlesPerPage);
            // take the number of the last page
            ViewBag.lastPage = Math.Ceiling((float)totalArticles /(float)_articlesPerPage);

            ViewBag.CurrentArticleQuery = paginatedArticles;
            // necessary to distinguish the action which returns the view when implementing FE logic
            ViewBag.Source = "filter-sort";
            // storing chosen category's title, when categoryID is not null, else storing null            
            ViewBag.CategoryName = categoryID != null ? DataBase.Categories.Where(category => category.CategoryID == categoryID).First().Title : null;
            ViewBag.CategoryID = categoryID;
            ViewBag.Sorting = sort;

            // message received
            if (TempData.ContainsKey("ActionMessage"))
            {
                ViewBag.DisplayedMessage = TempData["ActionMessage"];
            }

            return View();
        }


        //// Index method which returns all articles published by the request's sender (editor or admin)
        //[HttpGet("my-articles")]
        //public IActionResult Index()
        //{
        //    Fetch categories for side menu
        //    FetchCategories();
        //
        //    // will be implemented when roles and permissions are decided upon
        //    var returnedArticles = DataBase.Articles.Include("Categories").Where(article => article.UserID == _userManager.GetUserId(User));

        //    ViewBag.CurrentArticleQuery = returnedArticles;
        //    // the same view will be used for both Index actions, which
        //    // makes this variable necessary in the FE
        //    ViewBag.Source = "my-articles";

        //    // message received
        //    if (TempData.ContainsKey("message"))
        //    {
        //        ViewBag.DisplayedMessage = TempData["message"];
        //    }

        //    return View();
        //}


        // Method which returns the specified article's content and details, along with its
        // associated comments, category and user. From this view, the restrict, edit and
        // delete actions are accessible
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
                return Redirect("/Articles/Index/filter-sort");
            }

            // message received
            if (TempData.ContainsKey("ActionMessage"))
            {
                ViewBag.DisplayedMessage = TempData["ActionMessage"];
            }

            // mechanism to show/hide edit-delete buttons on article and comments depending on the user who made the request

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
        public IActionResult Show([FromForm] Comment commentToBeInserted)
        {
            // DEFAULT - TO BE DELETED
            commentToBeInserted.UserID = "318d855d-4d7a-4b5e-a293-40720ca8faac";
            // at first, the dates are identical
            commentToBeInserted.DateAdded = DateTime.Now;
            commentToBeInserted.DateEdited = DateTime.Now;

            if (ModelState.IsValid)
            {
                DataBase.Comments.Add(commentToBeInserted);
                DataBase.SaveChanges();
                TempData["ActionMessage"] = "Comment added successfully.";
            }

            return Redirect("/Articles/Show/" + commentToBeInserted.ArticleID);
        }

        // action returning the associated View
        public IActionResult New()
        {
            // Fetch categories for side menu
            FetchCategories();

            ArticleBundle articleToBeInserted = new ArticleBundle();
            articleToBeInserted.Categories = StoreCategories();
            articleToBeInserted.Article = new Article();

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
        public IActionResult New(ArticleBundle articleBundle)
        {
            // Fetch categories for side menu
            FetchCategories();

            // when article is firstly published, both dates are identical
            articleBundle.Article.PublicationDate = DateTime.Now;
            articleBundle.Article.CommitDate = DateTime.Now;
            // and it is not restricted by an administrator
            articleBundle.Article.IsRestricted = false;

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
                return Redirect("/Articles/Show/" + articleBundle.Article.ArticleID);
            }
            // provided article and chapters are invalid, so the 'New' View
            // is returned and populated with the said article and chapters

            articleBundle.Categories = StoreCategories();
            return View(articleBundle);
        }


        public IActionResult Edit(int articleID, int versionID)
        {
            // Fetch categories for side menu
            FetchCategories();

            ArticleVersionBundle articleVersionBundle = new ArticleVersionBundle();

            // the request involves editing a version other than the most recent version of the article with the provided articleID
            if (versionID != -1)
            {
                // making sure provided articleID and versionID are valid
                try
                {
                    // storing chosen article version into the bundled object
                    articleVersionBundle.Article = DataBase.ArticleVersions.Include("Category").Where(article => article.ArticleID == articleID)
                                                                                    .Where(article => article.VersionID == versionID).First();
                }
                catch
                {
                    TempData["ActionMessage"] = "No article with specified ID and version ID could be found.";
                    return Redirect("/Articles/Index/my-articles");
                }

                // storing chosen article's chapters in to bundled object
                articleVersionBundle.Chapters = DataBase.ChapterVersions.Where(chapter => chapter.ArticleID == articleID)
                                                                        .Where(chapter => chapter.VersionID == versionID).ToList();
            }
            // the request involves editing the most recent version of the article with the provided articleID
            else
            {
                // the most recent version is stored in the ARTICLES table,
                // which means that, in order to access the info within, an
                // 'Article' object is required
                Article article = new Article();
                // making sure provided articleID is valid
                try
                {
                    article = DataBase.Articles.Include("Category").Where(article => article.ArticleID == articleID).First();
                }
                catch
                {
                    TempData["ActionMessage"] = "No article with specified ID and version ID could be found.";
                    return Redirect("/Articles/Index/my-articles");
                }

                // copying values into the newly created 'ArticleVersionBundle' object
                articleVersionBundle.Article.ArticleID = article.ArticleID;
                articleVersionBundle.Article.CategoryID = article.CategoryID;
                articleVersionBundle.Article.Title = article.Title;
                articleVersionBundle.Article.CommitTitle = article.CommitTitle;
                articleVersionBundle.Article.CommitDate = article.CommitDate;

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

            // storing categories which will be sent in the front
            // for the dropdown selector
            articleVersionBundle.Categories = StoreCategories();

            // message received
            if (TempData.ContainsKey("ActionMessage"))
            {
                ViewBag.DisplayedMessage = TempData["ActionMessage"];
            }

            return View(articleVersionBundle);
        }


        [HttpPost]
        public IActionResult Edit(ArticleVersionBundle articleVersionBundle)
        {
            // Fetch categories for side menu
            FetchCategories();

            Article currentArticle;

            // making sure provided bundled object stores a valid articleID
            try
            {
                currentArticle = DataBase.Articles.Find(articleVersionBundle.Article.ArticleID);
            }
            catch
            {
                TempData["ActionMessage"] = "No article with specified ID could be found.";
                return Redirect("/Articles/Index/filter-sort");
            }

            if (ModelState.IsValid)
            {
                // there must be at least one chapter associated with every article
                if (articleVersionBundle.Chapters.Count == 0)
                {
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

                // computing the currentVersionID, based on entries of ARTICLE_VERSIONS
                var versionIDs = from articleVersion in DataBase.ArticleVersions
                                       where articleVersion.ArticleID == articleVersionBundle.Article.ArticleID
                                       select articleVersion.VersionID;

                int currentVersionID = versionIDs.Max();

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
                currentArticle.CommitTitle = articleVersionBundle.Article.Title;
                currentArticle.CommitDate = DateTime.Now;

                DataBase.SaveChanges();

                // storing current chapter versions, based on the 'ArticleVersionBundle' received through the form
                foreach(ChapterVersion newChapterVersion in articleVersionBundle.Chapters)
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
                return View(articleVersionBundle);
            }
            
            // bundled object wasn't valid, so it is sent back
            // to the view for the necessary modifications
            articleVersionBundle.Categories = StoreCategories();
            return View(articleVersionBundle);
        }


        [HttpPost]
        public IActionResult Delete(int articleID)
        {
            Article article;

            // making sure provided articleID is valid
            try
            {
                article = DataBase.Articles.Find(articleID);
            }
            catch
            {
                TempData["ActionMessage"] = "No article with specified ID could be found.";
                return Redirect("/Articles/Index/my-articles");
            }

            DataBase.Articles.Remove(article);
            DataBase.SaveChanges();

            TempData["ActionMessage"] = "Article deleted successfully.";
            return Redirect("/Articles/Index/my-articles");
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
