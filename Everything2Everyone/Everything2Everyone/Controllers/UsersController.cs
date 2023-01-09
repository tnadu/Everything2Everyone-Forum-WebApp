using Everything2Everyone.Data;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Everything2Everyone.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext DataBase;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            DataBase = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult ChangeEmail(string userID)
        {
            if (!User.IsInRole("Administrator") && userID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                TempData["ActionMessage"] = "You don't have permission to access this resource";
                return Redirect("/users/edit/" + User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

            EmailChange changeEmail = new EmailChange();

            // when the provided userID doesn't exist, the IsInRole() method throws an exception;
            // if an Administrator's account was compromised and the bad actor intends to do reconnaissance
            // by trying random ID's, they won't be prompted an error message which would clearly
            // indicate that the user doesn't exist, making it more difficult for them to script
            // such a technique (they have to also some browser automation to fill in the inputs,
            // before finally being prompted with the error message)
            try
            {
                bool isTheRequestedUserAnAdmin = DataBase.UserRoles.Where(u => u.UserId == userID).Select(u => u.RoleId).First() ==
                                                 DataBase.Roles.Where(r => r.Name == "Administrator").Select(r => r.Id).First();

                if (User.IsInRole("Administrator") && isTheRequestedUserAnAdmin && userID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                {
                    TempData["ActionMessage"] = "You don't have permission to access this resource";
                    return Redirect("/users/index");
                }

                // userID exists and the Administrator/Editor passed authorization
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
        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult ChangeEmail(EmailChange emailChange)
        {
            if (!User.IsInRole("Administrator") && emailChange.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                TempData["ActionMessage"] = "You don't have permission to access this resource";
                return Redirect("/users/edit/" + User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

            try
            {
                bool isTheRequestedUserAnAdmin = DataBase.UserRoles.Where(u => u.UserId == emailChange.UserID).Select(u => u.RoleId).First() ==
                                                 DataBase.Roles.Where(r => r.Name == "Administrator").Select(r => r.Id).First();

                if (User.IsInRole("Administrator") && isTheRequestedUserAnAdmin && emailChange.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                {
                    TempData["ActionMessage"] = "You don't have permission to access this resource";
                    return Redirect("/users/index");
                }
            }
            catch
            {
                TempData["ActionMessage"] = "No user with the specified ID could be found.";
                return Redirect("/users/index");
            }

            bool correctPassword = true;

            var hasher = new PasswordHasher<User>();

            User requestingUser = DataBase.Users.Where(user => user.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value).First();
            // the hash of the provided password must be identical to that stored in the database
            if (hasher.VerifyHashedPassword(requestingUser, requestingUser.PasswordHash, emailChange.Password) == PasswordVerificationResult.Failed)
            {
                TempData["ActionMessage"] = "Wrong password.";
                correctPassword = false;
            }

            if (ModelState.IsValid && correctPassword)
            {
                User userToChange;

                userToChange = DataBase.Users.Find(emailChange.UserID);

                userToChange.UserName = emailChange.NewEmail;
                userToChange.NormalizedUserName = emailChange.NewEmail.ToUpper();
                userToChange.Email = emailChange.NewEmail;
                userToChange.NormalizedEmail = emailChange.NewEmail.ToUpper();

                DataBase.SaveChanges();

                TempData["ActionMessage"] = "Email successfully changed.";
                return Redirect("/users/edit/" + emailChange.UserID);
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(emailChange);
        }


        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult ChangePassword(string userID)
        {
            if (!User.IsInRole("Administrator") && userID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                TempData["ActionMessage"] = "You don't have permission to access this resource";
                return Redirect("/users/edit/" + User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

            try
            {
                bool isTheRequestedUserAnAdmin = DataBase.UserRoles.Where(u => u.UserId == userID).Select(u => u.RoleId).First() ==
                                                 DataBase.Roles.Where(r => r.Name == "Administrator").Select(r => r.Id).First();

                if (User.IsInRole("Administrator") && isTheRequestedUserAnAdmin && userID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                {
                    TempData["ActionMessage"] = "You don't have permission to access this resource";
                    return Redirect("/users/index");
                }
            }
            catch
            {
                TempData["ActionMessage"] = "No user with the specified ID could be found.";
                return Redirect("/users/index");
            }

            PasswordChange changePassword = new PasswordChange();

            changePassword.UserID = userID;

            // Fetch categories for side menu
            FetchCategories();

            if (TempData.ContainsKey("ActionMessage"))
                ViewBag.DisplayedMessage = TempData["ActionMessage"];

            return View(changePassword);
        }


        [HttpPost]
        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult ChangePassword(PasswordChange passwordChange)
        {
            if (!User.IsInRole("Administrator") && passwordChange.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                TempData["ActionMessage"] = "You don't have permission to access this resource";
                return Redirect("/users/edit/" + User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

            try
            {
                bool isTheRequestedUserAnAdmin = DataBase.UserRoles.Where(u => u.UserId == passwordChange.UserID).Select(u => u.RoleId).First() ==
                                                 DataBase.Roles.Where(r => r.Name == "Administrator").Select(r => r.Id).First();

                if (User.IsInRole("Administrator") && isTheRequestedUserAnAdmin && passwordChange.UserID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                {
                    TempData["ActionMessage"] = "You don't have permission to access this resource";
                    return Redirect("/users/index");
                }
            }
            catch
            {
                TempData["ActionMessage"] = "No user with the specified ID could be found.";
                return Redirect("/users/index");
            }

            bool correctPassword = true;

            var hasher = new PasswordHasher<User>();

            User requestingUser = DataBase.Users.Where(user => user.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value).First();
            // the hash of the provided password must be identical to that stored in the database
            if (hasher.VerifyHashedPassword(requestingUser, requestingUser.PasswordHash, passwordChange.CurrentPassword) == PasswordVerificationResult.Failed)
            {
                TempData["ActionMessage"] = "Wrong password.";
                correctPassword = false;
            }

            if (ModelState.IsValid && correctPassword)
            {
                string hashedNewPassword = hasher.HashPassword(requestingUser, passwordChange.NewPassword);

                User userToChange = DataBase.Users.Find(passwordChange.UserID);
                userToChange.PasswordHash = hashedNewPassword;
                DataBase.SaveChanges();

                TempData["ActionMessage"] = "Password successfully changed.";
                return Redirect("/users/edit/" + passwordChange.UserID);
            }

            // Fetch categories for side menu
            FetchCategories();

            return View(passwordChange);
        }


        [Authorize(Roles = "Administrator")]
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


        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult Edit(string userID)
        {
            if (!User.IsInRole("Administrator") && userID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                TempData["ActionMessage"] = "You don't have permission to access this resource";
                return Redirect("/users/edit/" + User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

            User user;

            // when the provided userID doesn't exist, the IsInRole() method throws an exception;
            // if an Administrator's account was compromised and the bad actor intends to do reconnaissance
            // by trying random ID's, they won't be prompted an error message which would clearly
            // indicate that the user doesn't exist, making it more difficult for them to script
            // such a technique (they have to also some browser automation to fill in the inputs,
            // before finally being prompted with the error message)
            try
            {
                bool isTheRequestedUserAnAdmin = DataBase.UserRoles.Where(u => u.UserId == userID).Select(u => u.RoleId).First() ==
                                                 DataBase.Roles.Where(r => r.Name == "Administrator").Select(r => r.Id).First();

                if (User.IsInRole("Administrator") && isTheRequestedUserAnAdmin && userID != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                {
                    TempData["ActionMessage"] = "You don't have permission to access this resource";
                    return Redirect("/users/index");
                }

                // userID exists and the Administrator/Editor passed authorization
                user = DataBase.Users.Where(user => user.Id == userID).First();
                var roleID = DataBase.UserRoles.Where(u => u.UserId == userID).Select(u => u.RoleId).First();
                ViewBag.CurrentUserRoleName = DataBase.Roles.Where(r => r.Id == roleID).Select(r => r.Name).FirstOrDefault();
                ViewBag.CurrentUserRoleId = roleID;
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
                string[] roles = DataBase.Roles.Select(r => r.Name).ToArray();
                ViewBag.CurrentUserRoleName = roles[random.Next(roles.Length)];

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
        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult Edit(User userToBeInserted)
        {
            if (!User.IsInRole("Administrator") && userToBeInserted.Id != User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                TempData["ActionMessage"] = "You don't have permission to access this resource";
                return Redirect("/users/edit/" + User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

            try
            {
                bool isTheRequestedUserAnAdmin = DataBase.UserRoles.Where(u => u.UserId == userToBeInserted.Id).Select(u => u.RoleId).First() ==
                                                 DataBase.Roles.Where(r => r.Name == "Administrator").Select(r => r.Id).First();

                if (User.IsInRole("Administrator") && isTheRequestedUserAnAdmin && userToBeInserted.Id != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                {
                    TempData["ActionMessage"] = "You don't have permission to access this resource";
                    return Redirect("/users/index");
                }
            }
            catch
            {
                TempData["ActionMessage"] = "No user with the specified ID could be found.";
                return Redirect("/users/index");
            }

            // making sure provided roleID is valid            
            if (DataBase.Roles.Find(userToBeInserted.NewRoleID) == null)
            {
                TempData["ActionMessage"] = "No user or role with specified ID could be found.";
                return Redirect("/users/index");
            }


            if (ModelState.IsValid)
            {
                User user = DataBase.Users.Find(userToBeInserted.Id);

                if (User.IsInRole("Administrator")) {
                    string CurrentRoleId = DataBase.UserRoles.Where(userRole => userRole.UserId == userToBeInserted.Id).Select(u => u.RoleId).First();

                    if (userToBeInserted.NewRoleID == null)
                        userToBeInserted.NewRoleID = DataBase.Roles.Where(r => r.Name == "User").Select(r => r.Id).First();

                    // removing previous role
                    IdentityUserRole<string> userRole = DataBase.UserRoles.Find(userToBeInserted.Id, CurrentRoleId);
                    DataBase.UserRoles.Remove(userRole);
                    DataBase.SaveChanges();

                    // adding new role
                    userRole = new IdentityUserRole<string>();
                    userRole.UserId = userToBeInserted.Id;
                    userRole.RoleId = userToBeInserted.NewRoleID;
                    DataBase.UserRoles.Add(userRole);
                    DataBase.SaveChanges();
                }

                user.FirstName = userToBeInserted.FirstName;
                user.LastName = userToBeInserted.LastName;
                user.ShowPublicIdentity = userToBeInserted.ShowPublicIdentity;
                user.NickName = userToBeInserted.NickName;
                DataBase.SaveChanges();

                TempData["ActionMessage"] = "You have successfully changed the profile details!";
            }

            // Fetch categories for side menu
            FetchCategories();
            string roleId = DataBase.UserRoles.Where(userRole => userRole.UserId == userToBeInserted.Id).Select(u => u.RoleId).First();
            userToBeInserted.JoinDate = DataBase.Users.Where(u => u.Id == userToBeInserted.Id).Select(u => u.JoinDate).First();
            userToBeInserted.FetchedRoles = FetchRoles();

            ViewBag.CurrentUserRoleName = DataBase.Roles.Where(r => r.Id == roleId).Select(r => r.Name).FirstOrDefault();
            ViewBag.CurrentUserRoleId = roleId;          

            return View(userToBeInserted);
        }


        [HttpPost]
        [Authorize(Roles = "User,Editor,Administrator")]
        public IActionResult Delete(string userID)
        {
            //Authorize(userID);

            User userToBeRemoved;

            // making sure provided userID is valid - Administrator
            userToBeRemoved = DataBase.Users.Find(userID);
            
            if (userToBeRemoved==null)
            {
                TempData["ActionMessage"] = "No user with specified ID could be found.";
                return Redirect("/users/index");
            }

            DataBase.Users.Remove(userToBeRemoved);
            DataBase.SaveChanges();
            Console.WriteLine("Success!!!!!!!!!!!!!!");

            // admins won't be prompted to create a new account, unless they deleted their own account
            if (User.FindFirst(ClaimTypes.NameIdentifier).Value != userID) {
                TempData["message"] = "Account successfully deleted.";
                return Redirect("/users/index");
            }

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

            foreach (IdentityRole role in DataBase.Roles) 
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
