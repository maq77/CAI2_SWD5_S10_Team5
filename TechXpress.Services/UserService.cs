﻿using Azure.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using TechXpress.Data.Constants;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using TechXpress.Services.DTOs.ViewModels;


namespace TechXpress.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserImageService _userImageService;
        private readonly IEmailServer _emailServer;

        public UserService(IUnitOfWork unitOfWork, IEmailServer emailServer, IUserImageService userImageService, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService, ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _emailServer = emailServer;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _userImageService = userImageService;
        }
        public async Task<(AuthResponse authResponse, string RedirectUrl)> RegisterAsync(RegisterDTO model)
        {
            try
            {
                if (await _userManager.Users.AnyAsync(u => u.NormalizedEmail == model.Email.ToUpper()))
                {
                    return (new AuthResponse
                    {
                        IsSuccess = false,
                        Message = "This email is already registered."
                    }, "");
                }

                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return (new AuthResponse { IsSuccess=false,Message=errors}, "");
                }
                user = await _userManager.FindByEmailAsync(model.Email);

                await _userImageService.UploadUserImageAsync(user.Id, model.Image);

                await AssignRoleAsync(user.Email, "Customer");
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Generate tokens
                var claims = await GenerateClaims(user);
                var accessToken = await _tokenService.GenerateAccessToken(claims);
                var tokenInfo = await _tokenService.CreateToken(user.Id);

                //Email Confirmation
                var token = await GenerateEmailConfirmationToken(user);

                await _emailServer.SendVerificationEmailAsync(user.Email, token);

                return (new AuthResponse {IsSuccess= true, Token=accessToken, RefreshToken=tokenInfo.RefreshToken, Message = "User registered successfully. Please check your email to verify your account." }, "/Home/Index"); // Ensure the redirect URL is returned
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in RegisterAsync for {Email}", model.Email);
                return (new AuthResponse {IsSuccess= false,Message= "An error occurred while registering." }, "");
            }
        }
        public async Task<(AuthResponse authResponse, string RedirectUrl)> LoginAsync(LoginDTO model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (!result.Succeeded)
                {
                    _logger.LogWarning($"Login failed for {model.Email}");
                    
                    return (new AuthResponse { IsSuccess =false , Message="Invalid Credintials!"}, "");
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null) return (new AuthResponse { IsSuccess = false , Message="Cannot find user with this email" }, "");

                // Generate tokens
                var claims = await GenerateClaims(user);
                var accessToken = await _tokenService.GenerateAccessToken(claims);
                var tokenInfo = await _tokenService.CreateToken(user.Id);
                string redirectUrl = (await IsAdmin(user)) ? "/Admin/" : "/";
                return (new AuthResponse { IsSuccess=true, Token = accessToken, RefreshToken = tokenInfo.RefreshToken, Message = "Login Successfully!",}, redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in LoginAsync for {Email}", model.Email);
                return (new AuthResponse { IsSuccess = false, Message="Exception in Login could be server error, Try Again! " }, "");
            }
        }
        private async Task<bool> IsAdmin(User user) => await _userManager.IsInRoleAsync(user, "Admin");
        public async Task<bool> LogoutAsync()
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // Revoke all user tokens
                if (!string.IsNullOrEmpty(userId))
                {
                    await _tokenService.RevokeAllUserTokens(userId);
                }
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
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt
                });

            }

            return userDTOs;
        }
        public async Task<UserProfileDTO> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;
            var image = await _userImageService.GetImageByUserId(userId);
            return new UserProfileDTO
            {
                Id = user.Id,
                FirstName= user.FirstName,
                LastName= user.LastName,
                Email = user.Email,
                IsConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                Address_ = new AddressViewModel
                {
                    Street=user.Address,
                    City = user.City,
                    Country = user.Country,
                    PostalCode = user.PostalCode
                },
                UserImage = image
            };
        }
        public async Task<bool> UpdateUserProfileAsync(string userId, UserProfileDTO profile)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.FirstName = profile.FirstName;
            user.LastName = profile.LastName;
            user.PhoneNumber = profile.PhoneNumber;
            //user.Address = profile.Address;
            user.Address = profile.Address_.Street;
            user.City = profile.Address_.City;
            user.Country = profile.Address_.Country;
            user.PostalCode = profile.Address_.PostalCode;


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return false;

            var res_img = await _userImageService.UpdateUserImageAsync(userId, profile.Image);
            if (!res_img)
            {
                _logger.LogWarning($"Failed to update user image for {user.Email} or Image is null or ahven't updated image ,no new image provided");
                //return false;
            }

            return true;
        }
        public async Task<bool> DeleteUserAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            await _userManager.DeleteAsync(user);
            return true;
        }
        public async Task<User> FindUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user;
        }
        public async Task<User> FindUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found.");
                return null;
            }
            return user;
        }
        public async Task<AuthResponse> SendEmailConfirmation(string userId)
        {
            try
            {
                var user = await FindUserById(userId);
                if (user == null)
                    return (new AuthResponse { IsSuccess = false, Message = "User Not found!" });

                if (await IsEmailConfirmed(user))
                {
                    return (new AuthResponse { IsSuccess = false, Message = "Email is Already Confirmed!" });
                }

                var token = await GenerateEmailConfirmationToken(user);
                Console.WriteLine($"[SEND] UserId: {user.Email}, Token: {token.Length}");


                await _emailServer.SendVerificationEmailAsync(user.Email, token);

                return (new AuthResponse{ IsSuccess=true, Message="Sent Confirmation Email Successfully!" });
            }catch (Exception)
            {
                return (new AuthResponse { IsSuccess = false, Message = "Something went wrong!" });
            }
        }
        public async Task<string> GenerateEmailConfirmationToken(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return token;
        }
        public async Task<IdentityResult> ConfirmEmail(User user, string token)
        {
            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result;
        }
        public async Task<bool> IsEmailConfirmed(User user)
        {
            return await _userManager.IsEmailConfirmedAsync(user);
        }
        public async Task<string> GeneratePasswordResetToken(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return token;
        }
        public async Task<IdentityResult> ResetPassword(User user, string token, string password)
        {
            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Password reset successful for {user.Email}");
                return result;
            }
            else
            {
                _logger.LogWarning($"Password reset failed for {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return result;
            }
        }
        public async Task<List<Claim>> GenerateClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }
        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // Validate refresh token
                if (!await _tokenService.ValidateRefreshToken(refreshToken))
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = "Invalid token"
                    };
                }

                // Get token from database
                var tokenInfo = await _tokenService.GetTokenByRefreshToken(refreshToken);
                if (tokenInfo == null)
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = "Cannot retrieve token from Refresh Token"
                    };
                }

                // Get user
                var user = await _userManager.FindByIdAsync(tokenInfo.UserId);
                if (user == null)
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = "Cannot Find User Token"
                    };
                }

                // Revoke current token
                await _tokenService.RevokeToken(refreshToken);

                // Generate new tokens
                var claims = await GenerateClaims(user);
                var accessToken = await _tokenService.GenerateAccessToken(claims);
                var newTokenInfo = await _tokenService.CreateToken(user.Id);
                var roles = await _userManager.GetRolesAsync(user);

                return new AuthResponse
                {
                    IsSuccess = true,
                    Message = "Renewed Token Successfully!",
                    Token = accessToken,
                    RefreshToken = newTokenInfo.RefreshToken,
                    Email = user.Email,
                    Expiration = newTokenInfo.ExpiryDate,
                    UserId = user.Id,
                    Roles = roles.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in RefreshTokenAsync");
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
                //return (false, null, null);
            }
        } // For Testing
    }
}