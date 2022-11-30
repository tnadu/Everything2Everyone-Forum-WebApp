using Microsoft.AspNetCore.Mvc;

namespace Everything2Everyone.Controllers
{
    public class ChaptersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
