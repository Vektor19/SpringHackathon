using Microsoft.AspNetCore.Mvc;
using SpringHackathon.Models;
using System.Diagnostics;

namespace SpringHackathon.Controllers
{
	/// <summary>
	/// Controller for handling home-related requests.
	/// </summary>
	public class HomeController : Controller
	{

		public HomeController()
		{
			
		}

		/// <summary>
		/// Action method for the Index page.
		/// </summary>
		/// <returns>An asynchronous task that represents the operation and produces an IActionResult.</returns>
		public async Task<IActionResult> Index()
		{
			return View();
		}

		/// <summary>
		/// Action method for the Privacy page.
		/// </summary>
		/// <returns>An IActionResult representing the Privacy view.</returns>
		public IActionResult Rules()
		{
			return View();
		}

		/// <summary>
		/// Action method for handling errors.
		/// </summary>
		/// <returns>An IActionResult representing the Error view.</returns>
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}