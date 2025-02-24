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
        Task<IEnumerable<Product>> GetAllProducts();
        Task<Product?> GetProductById(int id);
        Task<IEnumerable<Product>> GetProductsByCategoryId(int categoryId);
        Task AddProduct(ProductDTO product, List<IFormFile>? image);
        Task UpdateProduct(Product product);
        Task DeleteProduct(int id);
    }
}
