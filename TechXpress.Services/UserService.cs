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

        public UserService(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<(bool, string)> RegisterAsync(RegisterDTO model)
        {
            try
            {
                if (await _userManager.Users.AnyAsync(u => u.Email == model.Email))
                {
                    _logger.LogWarning("Registration failed: Email {Email} is already in use.", model.Email);
                    return (false, "This email is already registered.");
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
                    _logger.LogWarning("User registration failed: {Errors}", errors);
                    return (false, errors);
                }

                await AssignRoleAsync(user.Email, "Customer"); 

                await _signInManager.SignInAsync(user, isPersistent: false);

                _logger.LogInformation("User {Email} registered successfully", user.Email);
                return (true, "Registration successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in RegisterAsync for {Email}", model.Email);
                return (false, "An error occurred while registering.");
            }
        }

        public async Task<string?> LoginAsync(LoginDTO model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed login attempt for {Email}", model.Email);
                    return null;
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null) return null;

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return "/Admin/Index";
                }

                return "/Home/Index";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in LoginAsync for {Email}", model.Email);
                return null;
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
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
        public async Task<List<UserDTO>> GetAllUsersAsync() //UserDTO
        {
            return await _userManager.Users
                .Select(u => new UserDTO
                {
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Roles = _userManager.GetRolesAsync(u).Result.ToList()
                }).ToListAsync();
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