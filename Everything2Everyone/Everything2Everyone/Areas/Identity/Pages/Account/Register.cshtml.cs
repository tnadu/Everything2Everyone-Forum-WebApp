// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Everything2Everyone.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Everything2Everyone.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email:")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password:")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password:")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            // CUSTOM FIELDS FOR OUR USER CLASS
            // First name:
            [Required(ErrorMessage = "First name is required. Length must be between 1 and 30 characters.")]
            [MinLength(1, ErrorMessage = "First name is required. Length must be between 1 and 30 characters.")]
            [MaxLength(30, ErrorMessage = "First name is required. Length must be between 1 and 30 characters.")]
            [Display(Name = "First name:")]
            public string FirstName { get; set; }

            // Last name:
            [Required(ErrorMessage = "Last name is required. Length must be between 1 and 30 characters.")]
            [MinLength(1, ErrorMessage = "Last name is required. Length must be between 1 and 30 characters.")]
            [MaxLength(30, ErrorMessage = "Last name is required. Length must be between 1 and 30 characters.")]
            [Display(Name = "Last name:")]
            public string LastName { get; set; }

            // Nick name:
            [Required(ErrorMessage = "Nickname is required. Length must be between 3 and 30 characters.")]
            [MinLength(3, ErrorMessage = "Nickname is required. Length must be between 3 and 30 characters.")]
            [MaxLength(30, ErrorMessage = "Nickname is required. Length must be between 3 and 30 characters.")]
            [Display(Name = "Nick name:")]
            public string NickName { get; set; }

            // JoinDate & ShowPublicIdentity - implicit values
            public DateTime JoinDate { get; set; } = DateTime.Now;

            public bool ShowPublicIdentity { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = "/articles/index";
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            returnUrl = "/articles/index";
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // Set the properties received from Input class
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.NickName = Input.NickName;
                user.JoinDate = DateTime.Now;
                user.ShowPublicIdentity = false;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                               
                var result = await _userManager.CreateAsync(user, Input.Password);
                user.EmailConfirmed = true;


                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _userManager.AddToRoleAsync(user, "User");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}
