using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SpringHackathon.Models;
using SpringHackathon.Services;
using System.Diagnostics;

namespace SpringHackathon.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private UserService _userService;
        private SignInManager<User> _signInManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager, SignInManager<User> signInManager, IUserStore<User> userStore)
        {
            _logger = logger;
            this._userService = new UserService(userManager);
            this._signInManager = signInManager;
            _userStore = userStore;
            _emailStore= (IUserEmailStore<User>)userStore;
        }

        public async Task<IActionResult> Index()
        {
            var user = Activator.CreateInstance<User>();
            await _userStore.SetUserNameAsync(user, "test123@gmail.com", CancellationToken.None);
            await _emailStore.SetEmailAsync(user, "test123@gmail.com", CancellationToken.None);
            user.EmailConfirmed= true;

            await _userService.CreateUser(user, "Lapse123!");
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
