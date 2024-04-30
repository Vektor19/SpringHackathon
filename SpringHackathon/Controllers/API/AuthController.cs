using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpringHackathon.Models;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;

namespace SpringHackathon.Controllers.API
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Provides operations for managing user accounts.
        /// </summary>
        private readonly UserManager<User> _userManager;
        /// <summary>
        /// Provides operations for managing user roles.
        /// </summary>
        private readonly RoleManager<UserRole> _roleManager;
        /// <summary>
        /// Provides operations for signing in users.
        /// </summary>
        private readonly SignInManager<User> _signInManager;
        /// <summary>
        /// Provides storage operations for user data.
        /// </summary>
        private readonly IUserStore<User> _userStore;
        /// <summary>
        /// Provides operations for managing user email addresses.
        /// </summary>
        private readonly IUserEmailStore<User> _emailStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="userManager">The user manager for managing user accounts.</param>
        /// <param name="roleManager">The role manager for managing user roles.</param>
        /// <param name="signInManager">The sign-in manager for signing in users.</param>
        /// <param name="userStore">The user store for storing and retrieving user data.</param>
        public AuthController(UserManager<User> userManager, RoleManager<UserRole> roleManager, SignInManager<User> signInManager, IUserStore<User> userStore)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<User>)userStore;
        }

        /// <summary>
        /// Retrieves a list of all users.
        /// </summary>
        /// <returns>An ActionResult containing a list of users if successful, otherwise an error message.</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await Task.FromResult(_userManager.Users.ToList());
            return Ok(users);
        }

        /// <summary>
        /// Authenticates a user based on provided credentials.
        /// </summary>
        /// <param name="loginModel">The login model containing user credentials.</param>
        /// <returns>
        /// An ActionResult indicating success if the user is authenticated, 
        /// otherwise returns a NotFound response with an error message.
        /// </returns>
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
                        return Ok(new { Message = "User succesfully authentificated" });
                }
            }
            return NotFound(new { Message = "Email or Password is not correct!" });
        }

        /// <summary>
        /// Registers a new user with the provided credentials.
        /// </summary>
        /// <param name="registerModel">The registration model containing user credentials.</param>
        /// <returns>
        /// An ActionResult indicating success if the user is successfully registered, 
        /// otherwise returns a BadRequest response.
        /// </returns>
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterModel registerModel)
        {
            if (ModelState.IsValid)
            {
                if (_userManager.FindByEmailAsync(registerModel.Email).Result != null)
                    return BadRequest(new { Message = "User already exists" });
                var user = Activator.CreateInstance<User>();
                await _userStore.SetUserNameAsync(user, registerModel.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, registerModel.Email, CancellationToken.None);
                user.EmailConfirmed = true;
                var result = await _userManager.CreateAsync(user, registerModel.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    return Ok(new { Message = "User succesfully registered" });
                } 
            }
            return BadRequest();
        }

        /// <summary>
        /// Updates user information based on provided data.
        /// </summary>
        /// <param name="email">The email of the user to update.</param>
        /// <param name="updateUserModel">The model containing updated user information.</param>
        /// <returns>
        /// An ActionResult indicating success if the user information is successfully updated, 
        /// otherwise returns a BadRequest or NotFound response with an error message.
        /// </returns>
        [HttpPut]
        [Route("update/{email}")]
        public async Task<IActionResult> UpdateUser(string email, UpdateUserModel updateUserModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                    return NotFound(new { Message = "User not found" });

                user.UserName = updateUserModel.Username;
                user.Email = updateUserModel.NewEmail;

                var changedPasswordResult = await _userManager.ChangePasswordAsync(user, updateUserModel.OldPassword, updateUserModel.NewPassword);

                if (changedPasswordResult.Succeeded)
                {
                    await _userManager.UpdateAsync(user);
                    return Ok(new { Message = "User successfully updated" });
                }
                else return BadRequest(new { Message = "Failed to update user" });
            }
            return BadRequest();
        }

        /// <summary>
        /// Deletes a user based on the provided email.
        /// </summary>
        /// <param name="Email">The email of the user to delete.</param>
        /// <returns>
        /// An ActionResult indicating success if the user is successfully deleted, 
        /// otherwise returns a BadRequest or NotFound response with an error message.
        /// </returns>
        [HttpDelete]
        [Route("delete/{Email}")]
        public async Task<IActionResult> DeleteUser(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            var authUser = await _userManager.GetUserAsync(User);
            if (authUser.Email.Equals(user.Email))
                await _signInManager.SignOutAsync();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return Ok(new { Message = "User successfully deleted" });
            else return BadRequest(new { Message = "Failed to delete user" });
        }
    }
}
