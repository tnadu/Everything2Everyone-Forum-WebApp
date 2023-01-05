using Microsoft.AspNetCore.Mvc;

namespace Everything2Everyone.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult SignUp()
        {
            return Redirect("/identity/account/register");
        }
        public IActionResult LogIn()
        {
            return Redirect("/identity/account/login");
        }
    }
}
