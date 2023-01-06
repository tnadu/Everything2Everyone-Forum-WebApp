using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Everything2Everyone.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext DataBase;

        public UsersController(ApplicationDbContext context)
        {
            DataBase = context;
        }


        public IActionResult MyProfile()
        {
            // The user will be the user who is currently using the app => TO DO 
            var userID = "318d855d-4d7a-4b5e-a293-40720ca8faac";

            FetchCategories();

            try
            {
                var user = DataBase.Users.Where(user => user.Id == userID).First();
                ViewBag.UserProfile = user;
                return View();
            }
            catch
            {
                TempData["ActionMessage"] = "No user with specified ID could be found!";
                return Redirect("/Articles/Index/filter-sort");
            }
        }

        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult ChangeEmail(string userID)
        {
            // if (! User.IsInRole("Admin") && userID != _userManager.GetUserById(User)) {
            //      TempData["ActionMessage"] = "You don't have permission to access this resource";
            //      return Redirect("/Articles/Index/");
            // }

            // if (User.IsInRole("Admin") && userID.IsInRole("Admin)) {
            //      TempData["ActionMessage"] = "You don't have permission to access this resource";
            //      return Redirect("/Users/Index");
            // }

            EmailChange changeEmail = new EmailChange();

            // for ADMINS
            // making sure provided ID is valid
            try
            {
                changeEmail.UserID = userID;
                changeEmail.NewEmail = DataBase.Users.Where(user => user.Id == userID).First().Email;
            }
            catch
            {
                TempData["ActionMessage"] = "No user with specified ID could be found.";
                return Redirect("/Users/Index");
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(changeEmail);
            
        }


        [HttpPost]
        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult ChangeEmail(EmailChange emailChange)
        {
            // if (! User.IsInRole("Admin") && emailChange.UserID != _userManager.GetUserById(User)) {
            //      TempData["message"] = "You don't have permission to access this resource";
            //      return Redirect("/articles/index/");
            // }

            // if (User.IsInRole("Admin") && emailChange.UserID.IsInRole("Admin)) {
            //      TempData["message"] = "You don't have permission to access this resource";
            //      return Redirect("/Users/index");
            // }

            bool correctPassword = true;

            // verifying regular-user's/editor's password
            if (!User.IsInRole("Admin"))
            {
                var hasher = new PasswordHasher<User>();

                // hashed provided password must be identical to that stored in the database
                if (hasher.VerifyHashedPassword(DataBase.Users.Where(user => user.Id == emailChange.UserID).First(),
                         DataBase.Users.Where(user => user.Id == emailChange.UserID).First().PasswordHash,
                         emailChange.Password) == 0)
                {
                    TempData["ActionMessage"] = "Wrong password.";
                    correctPassword = false;
                }
            }

            // verifying admin's password
            else
            {
                var hasher = new PasswordHasher<User>();

                // // hashed provided password must be identical to that stored in the database
                // 
                // if (hasher.VerifyHashedPassword(DataBase.Users.Where(user => user.Id == emailChange.UserID).First(),
                //                              DataBase.Users.Where(user => user.Id == _userManager.GetUserId(User)).First().PasswordHash,
                //                              emailChange.Password) == 0)
                // {
                TempData["ActionMessage"] = "Wrong password.";
                correctPassword = false;
                // }
            }

            if (ModelState.IsValid && correctPassword)
            {
                User userToChange;

                // for ADMINS
                // making sure provided ID is valid
                try
                {
                    userToChange = DataBase.Users.Find(emailChange.UserID);

                    userToChange.UserName = emailChange.NewEmail;
                    userToChange.NormalizedUserName = emailChange.NewEmail.ToUpper();
                    userToChange.Email = emailChange.NewEmail;
                    userToChange.NormalizedEmail = emailChange.NewEmail.ToUpper();

                    DataBase.SaveChanges();

                    TempData["ActionMessage"] = "Email successfully changed.";
                    return Redirect("/Users/MyProfile");
                }
                catch
                {
                    TempData["ActionMessage"] = "No user with specified ID could be found.";
                    return Redirect("/Users/Index");
                }
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(emailChange);
        }


        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult ChangePassword(string userID)
        {
            // if (! User.IsInRole("Admin") && userID != _userManager.GetUserById(User)) {
            //      TempData["message"] = "You don't have permission to access this resource";
            //      return Redirect("/articles/index/");
            // }

            // if (User.IsInRole("Admin") && userID.IsInRole("Admin)) {
            //      TempData["message"] = "You don't have permission to access this resource";
            //      return Redirect("/users/index");
            // }

            PasswordChange changePassword = new PasswordChange();

            // for ADMINS
            // making sure provided ID is valid
            try
            {
                changePassword.UserID = userID;
            }
            catch
            {
                TempData["ActionMessage"] = "No user with specified ID could be found.";
                return Redirect("/users/index");
            }

            // Fetch categories for side menu
            FetchCategories();

            if (TempData.ContainsKey("ActionMessage"))
                ViewBag.DisplayedMessage = TempData["ActionMessage"];

            return View(changePassword);
        }


        [HttpPost]
        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult ChangePassword(PasswordChange passwordChange)
        {
            // if (! User.IsInRole("Admin") && passwordChange.UserID != _userManager.GetUserById(User)) {
            //      TempData["message"] = "You don't have permission to access this resource";
            //      return Redirect("/articles/index/");
            // }

            // if (User.IsInRole("Admin") && passwordChange.UserID.IsInRole("Admin)) {
            //      TempData["message"] = "You don't have permission to access this resource";
            //      return Redirect("/users/index");
            // }

            bool correctPassword = true;

            // verifying regular-user's/editor's password
            if (!User.IsInRole("Admin"))
            {
                var hasher = new PasswordHasher<User>();

                // hashed provided password must be identical to that stored in the database
                if (hasher.VerifyHashedPassword(DataBase.Users.Where(user => user.Id == passwordChange.UserID).First(),
                         DataBase.Users.Where(user => user.Id == passwordChange.UserID).First().PasswordHash,
                         passwordChange.CurrentPassword) == 0)
                {
                    TempData["ActionMessage"] = "Wrong password.";
                    correctPassword = false;
                }
            }

            // verifying admin's password
            else
            {
                var hasher = new PasswordHasher<User>();

                // // hashed provided password must be identical to that stored in the database
                // if (hasher.VerifyHashedPassword(DataBase.Users.Where(user => user.Id == passwordChange.UserID).First(),
                //                              DataBase.Users.Where(user => user.Id == _userManager.GetUserId(User)).First().PasswordHash,
                //                              passwordChange.Password) == 0)
                // {
                TempData["ActionMessage"] = "Wrong password.";
                correctPassword = false;
                // }
            }

            if (ModelState.IsValid && correctPassword)
            {
                User userToChange;

                // for ADMINS
                // making sure provided ID is valid
                try
                {
                    userToChange = DataBase.Users.Find(passwordChange.CurrentPassword);
                }
                catch
                {
                    TempData["ActionMessage"] = "No user with specified ID could be found.";
                    return Redirect("/Users/Index");
                }

                // _userManager.ChangePassword(userToChange, passwordChange.CurrentPassword, passwordChange.NewPassword);

                TempData["ActionMessage"] = "Password successfully changed.";
                return Redirect("/Users/MyProfile");
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(passwordChange);
        }


        // [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            ViewBag.Users = DataBase.Users;

            // Fetch categories for side menu
            FetchCategories();

            if (TempData.ContainsKey("ActionMessage"))
                ViewBag.DisplayedMessage = TempData["ActionMessage"];

            return View();
        }


        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(string userID)
        {
            // if (! User.IsInRole("Admin") && userID != _userManager.GetUserById(User)) {
            //      TempData["message"] = "You don't have permission to access this resource";
            //      return Redirect("/articles/index/");
            // }

            // if (User.IsInRole("Admin") && userID.IsInRole("Admin)) {
            //      TempData["message"] = "You don't have permission to access this resource";
            //      return Redirect("/users/index");
            // }

            User user;

            // for ADMINS
            // making sure provided ID is valid
            try
            {
                user = DataBase.Users.Where(user => user.Id == userID).First();
            }
            catch
            {
                TempData["ActionMessage"] = "No user with specified ID could be found.";
                return Redirect("/Users/Index");
            }

            // Fetch categories for side menu
            FetchCategories();

            user.FetchedRoles = FetchRoles();

            if (TempData.ContainsKey("ActionMessage"))
                ViewBag.DisplayedMessage = TempData["ActionMessage"];

            return View(user);
        }


        [HttpPost]
        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(User userToBeInserted)
        {
            // if (! User.IsInRole("Admin") && userToBeInserted.ID != _userManager.GetUserById(User)) {
            //      TempData["message"] = "You don't have permission to access this resource";
            //      return Redirect("/articles/index/");
            // }

            // if (User.IsInRole("Admin") && user.IsInRole("Admin)) {
            //      TempData["message"] = "You don't have permission to access this resource";
            //      return Redirect("/users/index");
            // }

            if (ModelState.IsValid)
            {
                User user;

                // making sure provided userID and roleID are valid - ADMIN
                try
                {
                    user = DataBase.Users.Find(userToBeInserted.Id);
                    if (userToBeInserted.NewRoleID != null) 
                        DataBase.Roles.Find(userToBeInserted.NewRoleID);
                }
                catch
                {
                    TempData["ActionMessage"] = "No user or role with specified ID could be found.";
                    return Redirect("/Users/Index");
                }

                // if (User.IsInRole("Admin")) {
                //      string CurrentRoleID = DataBase.UserRoles.Where(userRole => userRole.UserId == userToBeInserted.ID).First().RoleID;
                //      _userManager.RemoveFromRole(userToBeInserted.ID, CurrentRoleID);
                //      _userManager.AddToRole(userToBeInserted.ID, userToBeInserted.NewRoleID);
                // }

                user.FirstName = userToBeInserted.FirstName;
                user.LastName = userToBeInserted.LastName;
                user.ShowPublicIdentity = userToBeInserted.ShowPublicIdentity;
                user.NickName = userToBeInserted.NickName;
                DataBase.SaveChanges();

                TempData["ActionMessage"] = "You have successfully changed your profile details!";
                return Redirect("/Users/MyProfile");
            }


            // Fetch categories for side menu
            FetchCategories();

            userToBeInserted.FetchedRoles = FetchRoles();

            return View(userToBeInserted);
        }


        [HttpPost]
        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Delete(string userID)
        {
            // if (! User.IsInRole("Admin") && userID != _userManager.GetUserById(User)) {
            //      TempData["message"] = "You don't have permission to access this resource";
            //      return Redirect("/articles/index/");
            // }

            // if (User.IsInRole("Admin") && userID.IsInRole("Admin)) {
            //      TempData["message"] = "You don't have permission to access this resource";
            //      return Redirect("/users/index");
            // }
            
            User userToBeRemoved;

            // making sure provided userID is valid - ADMIN
            try
            {
                userToBeRemoved = DataBase.Users.Find(userID);
            }
            catch
            {
                TempData["ActionMessage"] = "No user with specified ID could be found.";
                return Redirect("/Users/Index");
            }

            DataBase.Users.Remove(userToBeRemoved);
            DataBase.SaveChanges();

            // if (User.IsInRole("Admin")) {
            //      TempData["message"] = "Account successfully deleted.";
            //      return Redirect("/users/index");
            // }

            TempData["ActionMessage"] = "Your account was successfully deleted.";
            return Redirect("/sign-up");
        }


        // method which stores all categories into a viewbag item, in order to be shown in the side-menu partial view
        [NonAction]
        public void FetchCategories()
        {
            ViewBag.GlobalCategories = DataBase.Categories.OrderBy(category => category.Title);
        }


        [NonAction]
        public IEnumerable<SelectListItem> FetchRoles()
        {
            List<SelectListItem> returnedRoles = new List<SelectListItem>();

            foreach (Role role in DataBase.Roles) 
            {
                returnedRoles.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }

            return returnedRoles;
        }
    }
}
