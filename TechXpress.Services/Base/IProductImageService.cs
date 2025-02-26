using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface IProductImageService
    {
        public Task<List<ProductImageDTO>> UploadImages(int productId, List<IFormFile> images);
        public Task<bool> DeleteImage(int imageId);
    }
}
