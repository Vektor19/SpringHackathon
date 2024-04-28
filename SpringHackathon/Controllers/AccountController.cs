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
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
		private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly EmailSenderService _emailSenderService;
		public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IUserStore<User> userStore, IAuthenticationSchemeProvider authenticationSchemeProvider, EmailSenderService emailSenderService)
        {
            //_userService = new UserService(userManager, signInManager, userStore);
            _userManager = userManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<User>)userStore;
			_authenticationSchemeProvider = authenticationSchemeProvider;
            _emailSenderService = emailSenderService;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                return View(user);
            }

            return NotFound();

        }
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = Url.Content("~/");

			return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel, string returnurl=null)
        {
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home");
			}
			returnurl =returnurl ?? Url.Content("~/");

			if (ModelState.IsValid)
            {
                User appUser = await _userManager.FindByEmailAsync(loginModel.Email);
                if (appUser != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(appUser.UserName, loginModel.Password, false, false);

                    if (result.Succeeded)
                    {
                        return Redirect(returnurl ?? "/");
                    }
                    ModelState.AddModelError(nameof(loginModel.Email), "Login failed: invalid email or password");
                }
            }
            return View();

        }
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");

        }

        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
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
                    _emailSenderService.SendMessage(registerModel.Email, EmailTemplate.Subject,EmailTemplate.Body);
                }
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect(returnurl ?? "/");
            }
            return View();
        }
		
		public IActionResult ExternalLogin(string provider, string returnUrl = null)
		{
			// Перевірте, чи вказаний провайдер існує
			var redirectUrl = Url.Action("ExternalLoginCallback", values: new { returnUrl });
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}

		public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
		{

			returnUrl = returnUrl ?? Url.Content("~/");
			if (remoteError != null)
			{
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
			}
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
			}
			var currentuser = await _userManager.FindByEmailAsync(info.Principal.FindFirstValue(ClaimTypes.Email));
			if (currentuser != null)
			{
				var userWithLoginResult = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
				if (userWithLoginResult!=null)
				{
					var result = await _userManager.AddLoginAsync(currentuser, info);
				}
					// Sign in the user with this external login provider if the user already has a login.
					/*await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
*/
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
					{
						_emailSenderService.SendMessage(info.Principal.FindFirstValue(ClaimTypes.Email), EmailTemplate.Subject, EmailTemplate.Body);
					}
					result = await _userManager.AddLoginAsync(user, info);
					if (result.Succeeded)
					{
						// Sign in the user with this external login provider if the user already has a login.
						/*await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
*/
						await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
						return LocalRedirect(returnUrl);
					}
				}
			}
			return RedirectToAction("Index", "Home");
		}

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                return View(user);
            }

            return NotFound();

        }

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
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(user);
                }

            }
            else
            {
                return View(user);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Password()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                return View();
            }

            return NotFound();
        }


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
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return (View(model));

        }


    }

}

