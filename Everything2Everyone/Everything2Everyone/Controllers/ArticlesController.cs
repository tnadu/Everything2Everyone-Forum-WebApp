using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Everything2Everyone.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext DataBase;

        ArticlesController (ApplicationDbContext context)
        {
            DataBase = context;
        }


        // Index method which returns articles as paged, either unfiltered or filtered (by category),
        // either unsorted or sorted (chronologically/reverse chronologically/alphabetically/reverse
        // alphabetically). It is accesibile only to signed-in users of all kinds.
        [HttpGet("filter-sort")]
        public IActionResult Index(int? categoryID, int? filter)
        {

            var returnedArticles = DataBase.Articles.Include("Category");


            // filter (if specified)
            if (categoryID!=null)
            {
                try 
                {
                    returnedArticles = returnedArticles.Where(article => article.CategoryID == categoryID);
                }
                catch(Exception ex)
                {
                    returnedArticles = DataBase.Articles.Include("Category");
                }
            }

            // sort (if specified)
            if (filter == 0)
            {
                returnedArticles = returnedArticles.OrderBy(article => article.PublicationDate);
            }
            else if (filter == 1)
            {
                returnedArticles = returnedArticles.OrderByDescending(article => article.PublicationDate);
            }
            else if (filter == 2)
            {
                returnedArticles = returnedArticles.OrderBy(article => article.Title);
            }
            else if (filter == 3)
            {
                returnedArticles = returnedArticles.OrderByDescending(article => article.Title);
            }


            ViewBag.CurrentArticleQuery = returnedArticles;

            return View();
        }


        // Index method which returns all articles published by the request's sender (editor or admin)
        [HttpGet("my-articles")]
        public IActionResult Index()
        {
            // will be implemented when roles and permissions are decided upon
            var returnedArticles = DataBase.Articles.Include("Categories").Where(article => article.UserID == _userM.GetUserId(User));

            ViewBag.CurrentArticleQuery = returnedArticles;

            if (TempData.ContainsKey("deleteMessage"))
            {
                ViewBag.DisplayedMessage = TempData["deleteMessage"];
            }

            return View();
        }


        public IActionResult Show(int articleID)
        {
            Article returnedArtice = DataBase.Articles.Include("Category").Include("Chapters")
                                                      .Include("Comments").Include("User").Where(article => article.ArticleID == articleID).First();

            // mechanism to show/hide edit-delete buttons on article and comments based on accessing user

            return View(returnedArtice);
        }


        public IActionResult New()
        {
            ArticleBundle articleToBeInserted = new ArticleBundle();
            articleToBeInserted.Categories = StoreCategories();

            return View(articleToBeInserted);
        }


        [HttpPost]
        public IActionResult New(ArticleBundle articleBundle)
        {
            articleBundle.Article.PublicationDate = DateTime.Now;
            articleBundle.Article.CommitDate = DateTime.Now;
            articleBundle.Article.IsRestricted = false;

            if (ModelState.IsValid)
            {
                DataBase.Articles.Add(articleBundle.Article);
                DataBase.SaveChanges();

                foreach (Chapter chapter in articleBundle.Chapters)
                {
                    DataBase.Chapters.Add(chapter);
                    DataBase.SaveChanges();
                }

                TempData["newMessage"] = "Article added successfully.";
                return Redirect("/Articles/Show/" + articleBundle.Article.ArticleID);
            }
            else
            {
                return View(articleBundle);
            }

            
        }


        [HttpPost]
        public IActionResult Delete(int articleID)
        {
            Article article = DataBase.Articles.Find(articleID);

            if (article == null)
            {
                TempData["deleteMessage"] = "No article with specified ID could be found.";
                return Redirect("/Articles/Index/my-articles");
            }

            DataBase.Articles.Remove(article);
            DataBase.SaveChanges();

            TempData["deleteMessage"] = "Article deleted successfully.";
            return Redirect("/Articles/Index/my-articles");
        }


        [NonAction]
        public IEnumerable<SelectListItem> StoreCategories()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            var categories = DataBase.Categories;

            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryID.ToString(),
                    Text = category.Title.ToString()
                });
            }

            return selectList;
        }
    }
}
