using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpringHackathon.Models;

namespace SpringHackathon.Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly UserService _userService;
        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public UserService(UserManager<User> userManager, SignInManager<User> signInManager, IUserStore<User> userStore)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<User>)userStore;
        }
        public async Task<IdentityResult> CreateUser(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }
        public async Task<bool> Authenticate(LoginModel loginModel)
        {
            User appUser = await _userManager.FindByEmailAsync(loginModel.Email);
            if (appUser != null)
            {
                var result = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, false, false);
                return result.Succeeded;
            }
            return false;
        }
    }
}
