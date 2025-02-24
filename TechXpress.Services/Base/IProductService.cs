using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllProducts();
        Task<ProductDTO?> GetProductById(int id);
        Task<bool> AddProduct(ProductDTO model, List<IFormFile>? images);
        Task<bool> UpdateProduct(ProductDTO model, List<IFormFile>? images);
        Task<bool> DeleteProduct(int id);
    }
}
