﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface IUserService
    {
        Task<(AuthResponse authResponse, string RedirectUrl)> RegisterAsync(RegisterDTO model);
        Task<(AuthResponse authResponse, string RedirectUrl)> LoginAsync(LoginDTO model);
        Task<List<Claim>> GenerateClaims(User user);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync();
        Task<bool> AssignRoleAsync(string email, string role);
        Task<bool> MakeUserAdmin(string email);
        Task<List<UserDTO>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(string email);
        Task<bool> UpdateUserProfileAsync(string userId, UserProfileDTO profile);
        Task<User> FindUserByEmail(string email);
        Task<User> FindUserById(string userId);
        Task<string> GenerateEmailConfirmationToken(User user);
        Task<AuthResponse> SendEmailConfirmation(string userId);
        Task<IdentityResult> ConfirmEmail(User user, string token);
        Task<bool> IsEmailConfirmed(User user);
        Task<string> GeneratePasswordResetToken(User user);
        Task<IdentityResult> ResetPassword(User user, string token, string password);
        Task<UserProfileDTO> GetUserProfileAsync(string userId);
    }
}
