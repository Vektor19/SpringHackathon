using Microsoft.AspNetCore.Mvc;

namespace SpringHackathon.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Chat()
        {
            return View();
        }
    }
}
