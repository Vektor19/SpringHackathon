using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpringHackathon.Models;
using System.Diagnostics;

namespace SpringHackathon.Controllers
{
    public class HomeController : Controller
    {
        UserManager<User> userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager)
        {
            _logger = logger;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            User appUser = new User
            {
                UserName = "Oleh",
                Email = "test123@gmail.com"
            };

            IdentityResult result = await userManager.CreateAsync(appUser, "Lapse123!");
            if (result.Succeeded)
                ViewBag.Message = "User Created Successfully";
            else
            {
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
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
