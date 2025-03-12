using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechXpress.Data.Model;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;


namespace TechXpress.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool Success, string Message, string RedirectUrl)> RegisterAsync(RegisterDTO model)
        {
            try
            {
                if (await _userManager.Users.AnyAsync(u => u.Email == model.Email))
                {
                    return (false, "This email is already registered.", "");
                }

                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return (false, errors, "");
                }

                await AssignRoleAsync(user.Email, "Customer");
                await _signInManager.SignInAsync(user, isPersistent: false);

                return (true, "Registration successful.", "/Home/Index"); // Ensure the redirect URL is returned
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in RegisterAsync for {Email}", model.Email);
                return (false, "An error occurred while registering.", "");
            }
        }


        public async Task<(bool Success, string RedirectUrl)> LoginAsync(LoginDTO model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (!result.Succeeded)
                {
                    return (false, "");
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null) return (false, "");

                string redirectUrl = await _userManager.IsInRoleAsync(user, "Admin") ? "/Admin/" : "/";
                return (true, redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in LoginAsync for {Email}", model.Email);
                return (false, "");
            }
        }


        public async Task<bool> LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while logging out.");
                return false;
            }
        }

        public async Task<bool> AssignRoleAsync(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            await _userManager.AddToRoleAsync(user, role);
            return true;
        }
        public async Task<bool> MakeUserAdmin(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;
            await _userManager.AddToRoleAsync(user,"Admin");
            return true;
        }
        public async Task<List<UserDTO>> GetAllUsersAsync() //UserDTO
        {
            var users = await _userManager.Users.ToListAsync();
            var userDTOs = new List<UserDTO>();
            foreach(var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDTOs.Add(new UserDTO
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                });

            }

            return userDTOs;
        }
        public async Task<bool> DeleteUserAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            await _userManager.DeleteAsync(user);
            return true;
        }
    }
}