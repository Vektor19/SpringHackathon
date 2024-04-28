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
					var result = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, false, false);

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
		
		/*[HttpPost]
        [Route("roles/add")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleModel request)
        {
            var appRole = new UserRole { Name = request.Role };
            var createRole = await _roleManager.CreateAsync(appRole);

            return Ok(new { message = "role created succesfully" });
        }
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            var result = await RegisterAsync(registerModel);

            return result.Success ? Ok(result) : BadRequest(result.Message);

        }

        private async Task<RegisterResponse> RegisterAsync(RegisterModel registerModel)
        {
            try
            {
                var userExists = await _userManager.FindByEmailAsync(registerModel.Email);
                if (userExists != null) return new RegisterResponse { Message = "User already exists", Success = false };

                //if we get here, no user with this email..

                userExists = new User
                {
                    Email = registerModel.Email,
                    //UserName = registerModel.Username,
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                };
                var createUserResult = await _userManager.CreateAsync(userExists, registerModel.Password);
                if (!createUserResult.Succeeded) return new RegisterResponse { Message = $"Create user failed {createUserResult?.Errors?.First()?.Description}", Success = false };
                //user is created...
                //then add user to a role...
                var addUserToRoleResult = await _userManager.AddToRoleAsync(userExists, "USER");
                if (!addUserToRoleResult.Succeeded) return new RegisterResponse { Message = $"Create user succeeded but could not add user to role {addUserToRoleResult?.Errors?.First()?.Description}", Success = false };

                //all is still well..
                return new RegisterResponse
                {
                    Success = true,
                    Message = "User registered successfully"
                };



            }
            catch (Exception ex)
            {
                return new RegisterResponse { Message = ex.Message, Success = false };
            }
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(LoginResponse))]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await LoginAsync(request);

            return result.Success ? Ok(result) : BadRequest(result.Message);


        }

        private async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user is null) return new LoginResponse { Message = "Invalid email/password", Success = false };

                //all is well if ew reach here
                var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
                var roles = await _userManager.GetRolesAsync(user);
                var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x));
                claims.AddRange(roleClaims);

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1swek3u4uo2u4a6e"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.Now.AddMinutes(30);

                var token = new JwtSecurityToken(
                    issuer: "https://localhost:5001",
                    audience: "https://localhost:5001",
                    claims: claims,
                    expires: expires,
                    signingCredentials: creds

                    );

                return new LoginResponse
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    Message = "Login Successful",
                    Email = user?.Email,
                    Success = true,
                    UserId = user?.Id.ToString()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new LoginResponse { Success = false, Message = ex.Message };
            }


        }*/

	}
}
