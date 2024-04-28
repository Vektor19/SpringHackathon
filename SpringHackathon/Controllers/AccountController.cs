using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SpringHackathon.Models;
using SpringHackathon.Services;
using SpringHackathon.Utils;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SpringHackathon.Controllers
{
    /// <summary>
    /// Controller responsible for managing user accounts.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Manages user creation, deletion, and searching.
        /// </summary>
        private readonly UserManager<User> _userManager;
        /// <summary>
        /// Manages signing in a user and signing out a user.
        /// </summary>
        private readonly SignInManager<User> _signInManager;
        /// <summary>
        /// Represents a storage system for user information.
        /// </summary>
        private readonly IUserStore<User> _userStore;
        /// <summary>
        /// Provides functionality to store and retrieve user email information.
        /// </summary>
        private readonly IUserEmailStore<User> _emailStore;
        /// <summary>
        /// Provides access to authentication schemes supported by the application.
        /// </summary>
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        /// <summary>
        /// Service for sending emails to users.
        /// </summary>
        private readonly EmailSenderService _emailSenderService;

        private RoleManager<UserRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="userManager">User manager instance for managing user accounts.</param>
        /// <param name="signInManager">Sign-in manager instance for user authentication.</param>
        /// <param name="userStore">User store instance for storing user information.</param>
        /// <param name="authenticationSchemeProvider">Provider for accessing authentication schemes.</param>
        /// <param name="emailSenderService">Service for sending emails to users.</param>
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IUserStore<User> userStore, IAuthenticationSchemeProvider authenticationSchemeProvider, EmailSenderService emailSenderService, RoleManager<UserRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<User>)userStore;
			_authenticationSchemeProvider = authenticationSchemeProvider;
            _emailSenderService = emailSenderService;
            _roleManager = roleManager;
        }
        /// <summary>
        /// Displays the user's account details if authenticated.
        /// </summary>
        /// <returns>An asynchronous task that returns an action result.</returns>
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login");

            var user = await _userManager.GetUserAsync(User);

            if (user != null) 
                return View(user);
            return NotFound();
        }
        /// <summary>
        /// Displays the login view.
        /// </summary>
        /// <returns>The login view.</returns>
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = Url.Content("~/");
			return View();
        }

        /// <summary>
        /// Handles the login form submission.
        /// </summary>
        /// <param name="loginModel">The login model containing user credentials.</param>
        /// <param name="returnurl">Optional return URL after successful login.</param>
        /// <returns>The appropriate action result based on the login attempt.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel, string returnurl=null)
        {
			if (User.Identity.IsAuthenticated)
				return RedirectToAction("Index", "Home");
			returnurl =returnurl ?? Url.Content("~/");

			if (ModelState.IsValid)
            {
                User appUser = await _userManager.FindByEmailAsync(loginModel.Email);
                if (appUser != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(appUser.UserName, loginModel.Password, false, false);
                    if (result.Succeeded)
                        return Redirect(returnurl ?? "/");
                    ModelState.AddModelError(nameof(loginModel.Email), "Login failed: invalid email or password");
                }
            }
            return View();
        }

        /// <summary>
        /// Logs out the currently authenticated user.
        /// </summary>
        /// <returns>A redirect to the home page after successful logout.</returns>
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Displays the registration form.
        /// </summary>
        /// <returns>The registration view if the user is not authenticated; otherwise, redirects to the home page.</returns>
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        /// <summary>
        /// Handles the registration form submission.
        /// </summary>
        /// <param name="registerModel">The registration model containing user information.</param>
        /// <param name="returnurl">Optional return URL after successful registration.</param>
        /// <returns>The appropriate action result based on the registration attempt.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel registerModel,string returnurl=null)
        {
            if (ModelState.IsValid)
            {

                var user = Activator.CreateInstance<User>();
                await _userStore.SetUserNameAsync(user, registerModel.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, registerModel.Email, CancellationToken.None);
                user.EmailConfirmed = true;
                var result = await _userManager.CreateAsync(user, registerModel.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    _emailSenderService.SendMessage(registerModel.Email, EmailTemplate.Subject, EmailTemplate.Body);
                }
                    
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect(returnurl ?? "/");
            }
            return View();
        }

        /// <summary>
        /// Initiates an external login request.
        /// </summary>
        /// <param name="provider">The external authentication provider.</param>
        /// <param name="returnUrl">Optional return URL after successful external login.</param>
        /// <returns>A challenge result to start the external authentication process.</returns>
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
		{
			var redirectUrl = Url.Action("ExternalLoginCallback", values: new { returnUrl });
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}

        /// <summary>
        /// Handles the callback after an external login attempt.
        /// </summary>
        /// <param name="returnUrl">Optional return URL after successful login.</param>
        /// <param name="remoteError">Error message from the external login provider.</param>
        /// <returns>The appropriate action result based on the external login attempt.</returns>
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
		{
			returnUrl = returnUrl ?? Url.Content("~/");
			if (remoteError != null)
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });

			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });

			var currentuser = await _userManager.FindByEmailAsync(info.Principal.FindFirstValue(ClaimTypes.Email));
			if (currentuser != null)
			{
				var userWithLoginResult = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
				if (userWithLoginResult != null)
				{
					var result = await _userManager.AddLoginAsync(currentuser, info);
				}
                await _signInManager.SignInAsync(currentuser, isPersistent: false, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
			if (ModelState.IsValid)
			{
				var user = Activator.CreateInstance<User>();
				await _userStore.SetUserNameAsync(user, info.Principal.FindFirstValue(ClaimTypes.Email), CancellationToken.None);
				await _emailStore.SetEmailAsync(user, info.Principal.FindFirstValue(ClaimTypes.Email), CancellationToken.None);
				user.EmailConfirmed = true;

				var result = await _userManager.CreateAsync(user);

				if (result.Succeeded)
				{
					if (result.Succeeded)
						_emailSenderService.SendMessage(info.Principal.FindFirstValue(ClaimTypes.Email), EmailTemplate.Subject, EmailTemplate.Body);

					result = await _userManager.AddLoginAsync(user, info);
					if (result.Succeeded)
					{
						await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
						return LocalRedirect(returnUrl);
					}
				}
			}
			return RedirectToAction("Index", "Home");
		}

        /// <summary>
        /// Displays the user edit page.
        /// </summary>
        /// <returns>The view for editing user information.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login");

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
                return View(user);
            return NotFound();
        }

        /// <summary>
        /// Handles the POST request to edit user information.
        /// </summary>
        /// <param name="model">The model containing the edited user information.</param>
        /// <returns>
        /// If the model state is valid, returns the edited user information view.
        /// If the model state is invalid, returns the view with model errors.
        /// If the user is not found, returns the view with the user information.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Edit(EditModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                ViewBag.ModelError = "Validation failed. Please check your inputs.";
                return View(user);
            }

            if (user != null)
            {
                user.UserName = model.Username;
                user.Email = model.Email;

                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    ViewBag.IsSuccess = true;
                    return View(user);
                }
                else
                {
                    foreach (var error in updateResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(user);
                }
            }
            else return View(user);
        }

        /// <summary>
        /// Displays the page for changing the user's password.
        /// </summary>
        /// <returns>
        /// If the user is authenticated, returns the password change view.
        /// If the user is not authenticated, redirects to the login page.
        /// If the user is not found, returns a 404 Not Found error.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Password()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login");

            var user = await _userManager.GetUserAsync(User);

            if (user != null)
                return View();
            return NotFound();
        }

        /// <summary>
        /// Handles the POST request for changing the user's password.
        /// </summary>
        /// <param name="model">The model containing the old and new passwords.</param>
        /// <returns>
        /// If the model state is valid and the password change is successful, displays the password change view with a success message.
        /// If the model state is not valid, redisplays the password change view with validation errors.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Password(PasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _userManager.UpdateAsync(user);
                    ViewBag.IsSuccess = true;
                    ModelState.Clear();
                    return View();
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return (View(model));
        }
    }
}

