using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface IUserService
    {
        Task<(bool, string)> RegisterAsync(RegisterDTO model);
        Task<string?> LoginAsync(LoginDTO model);
        Task LogoutAsync();
        Task<bool> AssignRoleAsync(string email, string role);
        Task<List<UserDTO>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(string email);
    }
}
