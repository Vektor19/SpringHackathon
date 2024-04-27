using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpringHackathon.Models;
using SpringHackathon.Services;
using System.ComponentModel.DataAnnotations;

namespace SpringHackathon.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;

        public AccountController(UserManager<User> userManager ,SignInManager<User> signInManager, IUserStore<User> userStore)
        {
            //_userService = new UserService(userManager, signInManager, userStore);
            _userManager = userManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<User>)userStore;
           
        }
        public IActionResult Login()
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
        public async Task<IActionResult> Login(LoginModel loginModel, string returnurl=null)
        {
            if (ModelState.IsValid)
            {
                User appUser = await _userManager.FindByEmailAsync(loginModel.Email);
                if (appUser != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, false, false);

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
                await _userManager.CreateAsync(user, registerModel.Password);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect(returnurl ?? "/");
            }
            return View();
        }
    }
}
