using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpringHackathon.Models;

namespace SpringHackathon.Controllers
{
    /// <summary>
    /// Chat controller for the chat page
    /// </summary>
    [Authorize]
    public class ChatController : Controller
    {
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor for the chat controller
        /// </summary>
        /// <param name="userManager">The user manager</param>
        public ChatController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
<<<<<<< HEAD

=======
        /// <summary>
        /// Index action for the chat controller
        /// </summary>
        /// <returns>The chat page</returns>
>>>>>>> main
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            return View(currentUser);
        }
    }
}
