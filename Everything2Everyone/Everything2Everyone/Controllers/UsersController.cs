using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace Everything2Everyone.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext DataBase;

        public UsersController(ApplicationDbContext context)
        {
            DataBase = context;
        }


        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult ChangeEmail(string userID)
        {
            Authorize(userID);

            EmailChange changeEmail = new EmailChange();

            // for ADMINS
            // making sure provided ID is valid
            try
            {
                changeEmail.UserID = userID;
                changeEmail.NewEmail = DataBase.Users.Where(user => user.Id == userID).First().Email;
            }
            // generating mock email
            catch
            {
                var random = new Random();

                string MockEmail;

                string[] nickNames = {"johndoe123", "jane_smith2000", "robert.doe", "sarahm33", "mike.jones", "samuel.brown", "lisa_taylor", "william.johnson",
                    "emmam41", "michael_baker", "david.taylor", "sarah_lee", "susan.brown", "mike_taylor", "john.lee", "davidm42", "sarah.jones", "robert_lee",
                    "lisa.smith", "michael.brown", "jane_doe", "samuel_taylor", "johnm34", "david.johnson", "susan_lee", "william_taylor", "emma.jones", "mike.brown",
                    "sarah.doe", "robert_smith", "lisa.taylor", "michael_johnson", "jane.lee", "samuel.doe", "john.smith", "david_taylor", "susan.jones", "william.brown", "emma_lee"};

                MockEmail = nickNames[random.Next(nickNames.Length)] + "@";

                string[] emailDomains = {"aol.com", "gmail.com", "hotmail.com", "yahoo.com", "outlook.com", "zoho.com", "icloud.com", 
                    "protonmail.com", "gmx.com", "mail.com", "inbox.com", "hushmail.com", "tutanota.com", "posteo.de", "openmailbox.org",
                    "yandex.com", "keemail.me", "mymail.com", "mac.com"};

                MockEmail += emailDomains[random.Next(emailDomains.Length)];

                changeEmail.NewEmail = MockEmail;
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(changeEmail);
            
        }


        [HttpPost]
        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult ChangeEmail(EmailChange emailChange)
        {
            Authorize(emailChange.UserID);

            bool correctPassword = true;

            var hasher = new PasswordHasher<User>();

            // the hash of the provided password must be identical to that stored in the database
            //if (hasher.VerifyHashedPassword(DataBase.Users.Where(user => user.Id == User.Id).First().PasswordHash,
            //            emailChange.Password) == PasswordVerificationResult.Failed)
            //{
            //    TempData["ActionMessage"] = "Wrong password.";
            //    correctPassword = false;
            //}

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
                    return Redirect("/users/edit/" + emailChange.UserID);
                }
                catch
                {
                    TempData["ActionMessage"] = "No user with specified ID could be found.";
                    return Redirect("/users/index");
                }
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(emailChange);
        }


        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult ChangePassword(string userID)
        {
            Authorize(userID);

            PasswordChange changePassword = new PasswordChange();

            changePassword.UserID = userID;

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
            Authorize(passwordChange.UserID);

            bool correctPassword = true;

            var hasher = new PasswordHasher<User>();

            // the hash of the provided password must be identical to that stored in the database
            //if (hasher.VerifyHashedPassword(DataBase.Users.Where(user => user.Id == User.Id).First().PasswordHash,
            //            emailChange.Password) == PasswordVerificationResult.Failed)
            //{
            //    TempData["ActionMessage"] = "Wrong password.";
            //    correctPassword = false;
            //}

            if (ModelState.IsValid && correctPassword)
            {
                User userToChange;

                // for ADMINS
                // making sure provided ID is valid
                try
                {
                    userToChange = DataBase.Users.Find(passwordChange.UserID);
                    // _userManager.ChangePassword(userToChange, passwordChange.CurrentPassword, passwordChange.NewPassword);

                    TempData["ActionMessage"] = "Password successfully changed.";
                    return Redirect("/users/edit" + passwordChange.UserID);
                }
                catch
                {
                    TempData["ActionMessage"] = "No user with specified ID could be found.";
                    return Redirect("/users/index");
                }
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(passwordChange);
        }


        // [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            // Fetch categories for side menu
            FetchCategories();

            if (TempData.ContainsKey("ActionMessage"))
                ViewBag.DisplayedMessage = TempData["ActionMessage"];

            var currentUsers = DataBase.Users;

            // pagination
            int usersPerPage = 10;
            int numberOfUsers = currentUsers.Count();
            var currentPageNumber = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var lastPage = Convert.ToInt32(Math.Ceiling((float)numberOfUsers / (float)usersPerPage));

            // <=1st page: offset = 0
            // 2nd page: offset = 10
            // ...
            // >=last page: offset = 10 * (last page - 1)
            var offset = 0;
            if (lastPage > 1)
            {
                if (currentPageNumber >= lastPage)
                    offset = (lastPage - 1) * usersPerPage;
                else if (currentPageNumber > 1)
                    offset = (currentPageNumber - 1) * usersPerPage;
            }
            ViewBag.Users = currentUsers.Skip(offset).Take(usersPerPage);
            ViewBag.lastPage = lastPage;

            return View();
        }


        // [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(string userID)
        {
            Authorize(userID);

            User user;

            // for ADMINS
            // making sure provided ID is valid
            try
            {
                user = DataBase.Users.Where(user => user.Id == userID).First();
                //ViewBag.CurrentUserRole = ((RolePrincipal)User).GetRoles()[0];
            }
            // generating mock user
            catch
            {
                user = new User();
                user.Id = userID;

                var random = new Random();

                // first name generation
                string[] firstNames = { "Emily", "Olivia", "Sophia", "Abigail", "Madison", "Elizabeth", "Charlotte", "Avery", "Chloe", "Ella",
                    "Harper", "Isabella", "Ava", "Mia", "Emily", "Elizabeth", "Sophia", "Olivia", "Ava", "Isabella", "Liam", "Noah", "William",
                    "James", "Oliver", "Benjamin", "Elijah", "Lucas", "Mason", "Logan", "Jacob", "Michael", "Alexander", "Ethan", "Daniel", "Matthew", "Aiden", "Henry" };

                user.FirstName = firstNames[random.Next(firstNames.Length)];

                // last name generation
                string[] lastNames = { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson",
                    "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez", "Robinson", "Clark", "Rodriguez", "Lewis",
                    "Lee", "Walker", "Hall", "Allen", "Young", "King", "Wright", "Scott", "Green", "Baker", "Adams", "Nelson", "Carter", "Mitchell" };

                user.LastName = lastNames[random.Next(lastNames.Length)];

                // random public identity
                user.ShowPublicIdentity = random.Next(2) == 1 ? true : false;

                // nickname
                string[] nickNames = {"johndoe123", "jane_smith2000", "robert.doe", "sarahm33", "mike.jones", "samuel.brown", "lisa_taylor", "william.johnson",
                    "emmam41", "michael_baker", "david.taylor", "sarah_lee", "susan.brown", "mike_taylor", "john.lee", "davidm42", "sarah.jones", "robert_lee",
                    "lisa.smith", "michael.brown", "jane_doe", "samuel_taylor", "johnm34", "david.johnson", "susan_lee", "william_taylor", "emma.jones", "mike.brown",
                    "sarah.doe", "robert_smith", "lisa.taylor", "michael_johnson", "jane.lee", "samuel.doe", "john.smith", "david_taylor", "susan.jones", "william.brown", "emma_lee"};

                user.NickName = nickNames[random.Next(nickNames.Length)];

                // random role picker
                //string[] roles = DataBase.Roles.Select(r => r.Name).ToArray();
                //ViewBag.CurrentUserRole = roles[random.Next(roles.Length)];

                // join date generation
                DateTime firstRegisterDay = new DateTime(2022, 10, 1);                              // the date the platform went public and registration was open
                int validJoinDateInterval = (DateTime.Now - firstRegisterDay).Days;                 // computing the number of days that have passed since
                user.JoinDate = firstRegisterDay.AddDays(random.Next(validJoinDateInterval + 1));   // the join date will be generated by adding a random number of
                                                                                                    // days, with a max value equal to that generated earlier
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
            Authorize(userToBeInserted.Id);

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
                    return Redirect("/users/index");
                }

                // if (User.IsInRole("Admin")) {
                //      string CurrentRoleID = DataBase.UserRoles.Where(userRole => userRole.UserId == userToBeInserted.ID).First().RoleID;
                //      
                //      if (userToBeInserted.NewRoleID == null)
                //          userToBeInserted.NewRoleID = Database.Roles.Where(r => r.Name == "User").First().Id;
                //      
                //      _userManager.RemoveFromRole(userToBeInserted.ID, CurrentRoleID);
                //      _userManager.AddToRole(userToBeInserted.ID, userToBeInserted.NewRoleID);
                // }

                user.FirstName = userToBeInserted.FirstName;
                user.LastName = userToBeInserted.LastName;
                user.ShowPublicIdentity = userToBeInserted.ShowPublicIdentity;
                user.NickName = userToBeInserted.NickName;
                DataBase.SaveChanges();

                TempData["ActionMessage"] = "You have successfully changed the profile details!";
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
            Authorize(userID);

            User userToBeRemoved;

            // making sure provided userID is valid - ADMIN
            try
            {
                userToBeRemoved = DataBase.Users.Find(userID);
            }
            catch
            {
                TempData["ActionMessage"] = "No user with specified ID could be found.";
                return Redirect("/users/index");
            }

            DataBase.Users.Remove(userToBeRemoved);
            DataBase.SaveChanges();

            // admins won't be prompted to create a new account, unless they deleted their own account
            // if (User.Id != userID) {
            //      TempData["message"] = "Account successfully deleted.";
            //      return Redirect("/users/index");
            // }

            TempData["ActionMessage"] = "Account successfully deleted.";
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


        [NonAction]
        public IActionResult Authorize(string userID)
        {
            // if (! User.IsInRole("Admin") && userID != User.FindFirst(ClaimTypes.NameIdentifier)) {
            //      TempData["ActionMessage"] = "You don't have permission to access this resource";
            //      return Redirect("/users/edit/" + userID);
            // }

            // when the provided userID doesn't exist, the IsInRole() method throws an exception;
            // if an admin's account was compromised and the bad actor intends to do reconnaissance
            // by trying random ID's, they won't be prompted an error message which would clearly
            // indicate that the user doesn't exist, making it more difficult for them to script
            // such a technique (they have to also some browser automation to fill in the inputs,
            // before finally being prompted with the error message)
            try
            {
                // if (User.IsInRole("Admin") && _userManager.IsInRole(userID, "Admin) && User.FindFirst(ClaimTypes.NameIdentifier)) {
                //      TempData["ActionMessage"] = "You don't have permission to access this resource";
                //      return Redirect("/users/index");
                // }
            }
            catch
            {
            }

            // the user was successfully authorized
            return new StatusCodeResult(200);
        }
    }
}
