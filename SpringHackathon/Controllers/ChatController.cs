using Microsoft.AspNetCore.Mvc;

namespace SpringHackathon.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Chat()
        {
            return View();
        }

        public Task<IActionResult> SendMessage()
        {
            string message = GetMessage();
        }

        public string GetMessage()
        {
            string result = Request.Form["messageHolder"];
            if(result == null)
                return null;
            return result;
        }
    }
}
