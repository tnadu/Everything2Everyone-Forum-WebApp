using Microsoft.AspNetCore.Mvc;

namespace Everything2Everyone.Controllers
{
    public class CommentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
