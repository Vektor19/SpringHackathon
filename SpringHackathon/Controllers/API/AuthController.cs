using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using System.Net;
using System.Security.Claims;
using System.Text;
using SpringHackathon.Models;
using SpringHackathon.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SpringHackathon.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Internal;
using System.Linq.Expressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SpringHackathon.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // GET: api/<ValuesController>
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<UserRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
		public AuthController(UserManager<User> userManager, RoleManager<UserRole> roleManager, SignInManager<User> signInManager, IUserStore<User> userStore)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userStore = userStore;
			_emailStore = (IUserEmailStore<User>)userStore;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
			var users = await Task.FromResult(_userManager.Users.ToList());
			return Ok(users);
		}
		//[HttpPost]
		//[Route("register")]
		//public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
		//{
		//	var result = await RegisterAsync(registerModel);

		//	return result.Success ? Ok(result) : BadRequest(result.Message);

		//} 
		[HttpPost]
		[Route("login")]
		public async Task<IActionResult> Login(LoginModel loginModel)
		{

			if (ModelState.IsValid)
			{
				User appUser = await _userManager.FindByEmailAsync(loginModel.Email);
				if (appUser != null)
				{
					var result = await _signInManager.PasswordSignInAsync(appUser.UserName, loginModel.Password, false, false);

					if (result.Succeeded)
					{
						return Ok(new { Message = "User succesfully authentificated" });
					}	
				}
			}
			return NotFound(new { Message = "Email or Password is not correct!" });
		}
		[HttpPost]
		[Route("register")]
		public async Task<IActionResult> Register(RegisterModel registerModel)
        {
            if (ModelState.IsValid)
            {
                if(_userManager.FindByEmailAsync(registerModel.Email).Result != null)
                {
					return BadRequest(new { Message = "User already exists" });
				}
                var user = Activator.CreateInstance<User>();
                await _userStore.SetUserNameAsync(user, registerModel.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, registerModel.Email, CancellationToken.None);
                user.EmailConfirmed = true;
                var result = await _userManager.CreateAsync(user, registerModel.Password);
                if (result.Succeeded)
                {
                    return Ok(new { Message = "User succesfully registered" });
                }
            }
            return BadRequest();
        }
		[HttpPut]
		[Route("update/{email}")]
		public async Task<IActionResult> UpdateUser(string email, UpdateUserModel updateUserModel)
		{
			if (ModelState.IsValid)
            {
				var user = await _userManager.FindByEmailAsync(email);

				// Check if the user exists
				if (user == null)
				{
					return NotFound(new { Message = "User not found" });
				}
                user.UserName = updateUserModel.Username;
				user.Email = updateUserModel.NewEmail; 

				var changedPasswordResult = await _userManager.ChangePasswordAsync(user, updateUserModel.OldPassword, updateUserModel.NewPassword);

				// Check if the update was successful
				if (changedPasswordResult.Succeeded)
				{
					await _userManager.UpdateAsync(user);
					return Ok(new { Message = "User successfully updated" });
				}
				else
				{
					// If update failed, return an error message
					return BadRequest(new { Message = "Failed to update user" });
				}
			}
		    return BadRequest();
		}
		[HttpDelete]
		[Route("delete/{Email}")]
		public async Task<IActionResult> DeleteUser(string Email)
		{
			// Find the user by ID
			var user = await _userManager.FindByEmailAsync(Email);
			// Check if the user exists
			if (user == null)
			{
				return NotFound(new { Message = "User not found" });
			}
            var authUser = await _userManager.GetUserAsync(User);
            if (authUser.Email.Equals(user.Email))
            {
                await _signInManager.SignOutAsync();
            }
			// Delete the user
			var result = await _userManager.DeleteAsync(user);

			// Check if the deletion was successful
			if (result.Succeeded)
			{
				return Ok(new { Message = "User successfully deleted" });
			}
			else
			{
				// If deletion failed, return an error message
				return BadRequest(new { Message = "Failed to delete user" });
			}
		}
	}
}
