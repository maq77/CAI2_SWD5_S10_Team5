using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using static System.Net.Mime.MediaTypeNames;

namespace TechXpress.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _imageFolder = "wwwroot/images";

        public ProductImageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProductImageDTO>> UploadImages(int productId, List<IFormFile> images)
        {
            var uploadedImages = new List<ProductImage>();

            foreach (var image in images)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine("wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                uploadedImages.Add(new ProductImage
                {
                    ProductId = productId,
                    ImagePath = "/images/" + fileName
                });
            }

            await _unitOfWork.ProductImages.AddRange(uploadedImages,log=>Console.WriteLine(log));
            await _unitOfWork.SaveAsync();

            return uploadedImages.Select(img => new ProductImageDTO
            {
                Id = img.Id,
                ImagePath = img.ImagePath
            }).ToList();
        }

        public async Task<bool> DeleteImage(int imageId)
        {
            var image = await _unitOfWork.ProductImages.GetById(imageId);
            if (image == null) return false;

            var filePath = Path.Combine(_imageFolder, image.ImagePath.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            await _unitOfWork.ProductImages.Delete(imageId,log=>Console.WriteLine(log));
            return await _unitOfWork.SaveAsync();
        }
    }
}
