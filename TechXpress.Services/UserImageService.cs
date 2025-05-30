using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Services
{
    public class UserImageService : IUserImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _imageFolder;
        private const string DefaultImageName = "default-pic.png";
        private const string RelativeImagePath = "/images/ProfilePics/";

        public UserImageService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _imageFolder = Path.Combine(webHostEnvironment.WebRootPath, "images", "ProfilePics");

            if (!Directory.Exists(_imageFolder))
            {
                Directory.CreateDirectory(_imageFolder);
            }

            EnsureDefaultImageExists();
        }

        private void EnsureDefaultImageExists()
        {
            var defaultImagePath = Path.Combine(_imageFolder, DefaultImageName);
            if (!File.Exists(defaultImagePath))
            {
                using var fs = new FileStream(defaultImagePath, FileMode.Create);
            }
        }

        public async Task<bool> UploadUserImageAsync(string userId, IFormFile? image)
        {
            try
            {
                if (string.IsNullOrEmpty(userId)) return false;

                var newImagePath = await SaveImageToDiskAsync(image);

                var userImage = new UserImage
                {
                    UserId = userId,
                    ImagePath = newImagePath
                };

                await _unitOfWork.UsersImages.Add(userImage, log => Console.WriteLine(log));
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UploadUserImageAsync] {ex.Message}");
                return false;
            }
        }

        public async Task<UserImageDTO> GetImageByUserId(string userId)
        {
            var image = await _unitOfWork.UsersImages.Find_First(p => p.UserId == userId);
            if (image != null)
            {
                return new UserImageDTO
                {
                    Id = image.Id,
                    ImagePath = image.ImagePath,
                    UserId = image.UserId
                };
            }

            return new UserImageDTO
            {
                UserId = userId,
                ImagePath = RelativeImagePath + DefaultImageName
            };
        }

        public async Task<bool> UpdateUserImageAsync(string? userId, IFormFile? image)
        {
            try
            {
                if (image == null || string.IsNullOrEmpty(userId))
                    return false;

                var userImage = await _unitOfWork.UsersImages.Find_First(p => p.UserId == userId);

                if (userImage != null && !string.IsNullOrWhiteSpace(userImage.ImagePath) &&
                    !userImage.ImagePath.EndsWith(DefaultImageName))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                        userImage.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }
                }

                var newImagePath = await SaveImageToDiskAsync(image);

                if (userImage != null)
                {
                    userImage.ImagePath = newImagePath;
                    await _unitOfWork.UsersImages.Update(userImage, log => Console.WriteLine(log));
                }
                else
                {
                    return await UploadUserImageAsync(userId, image);
                }

                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateUserImageAsync] {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserImageAsync(string userId)
        {
            var userImage = await _unitOfWork.UsersImages.Find_First(p => p.UserId == userId);
            if (userImage != null && !userImage.ImagePath.EndsWith(DefaultImageName))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath,
                    userImage.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                await _unitOfWork.UsersImages.Delete(userImage.Id, log => Console.WriteLine(log));
                return await _unitOfWork.SaveAsync();
            }

            return false;
        }

        private async Task<string> SaveImageToDiskAsync(IFormFile? image)
        {
            if (image == null)
                return RelativeImagePath + DefaultImageName;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                throw new Exception("Unsupported image format.");

            var fileName = Guid.NewGuid() + ext;
            var fullPath = Path.Combine(_imageFolder, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await image.CopyToAsync(stream);

            return RelativeImagePath + fileName;
        }
    }
}