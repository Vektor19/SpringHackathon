using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpringHackathon.Models;

namespace SpringHackathon.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly UserManager<User> _userManager;

        public ChatController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> Chat()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            return View(currentUser);
        }
    }
}
