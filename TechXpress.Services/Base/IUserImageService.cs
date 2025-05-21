using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface IUserImageService
    {
        public Task<bool> UploadUserImageAsync(string userId, IFormFile? image);
        public Task<bool> UpdateUserImageAsync(string? userId, IFormFile? image);
        public Task<bool> DeleteUserImageAsync(string userId);
        public Task<UserImageDTO> GetImageByUserId(string userId);
    }
}
