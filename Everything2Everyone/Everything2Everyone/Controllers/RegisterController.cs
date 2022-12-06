using Microsoft.AspNetCore.Mvc;

namespace Everything2Everyone.Controllers
{
    public class RegisterController : Controller
    {
        public IActionResult SignUp()
        {
            return View();
        }
        public IActionResult LogIn()
        {
            return View();
        }
    }
}
