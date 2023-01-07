using Microsoft.AspNetCore.Mvc;

namespace Everything2Everyone.Controllers
{
    // only used for forwarding requests to the Identity Controller
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
