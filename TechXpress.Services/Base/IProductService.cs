using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Services.DTOs;
using TechXpress.Services.DTOs.ViewModels;
using X.PagedList;

namespace TechXpress.Services.Base
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllProducts();
        Task<IEnumerable<ProductDTO>> GetProductsByCategory(int categoryId);
        Task<IEnumerable<ProductDTO>> SearchProducts(string? searchTerm);
        public Task<IPagedList<ProductDTO>> GetPagedProducts(int pageNumber, int pageSize, int? categoryId, string? searchTerm);
        public Task<IEnumerable<ProductDTO>> GetPopularProducts(int top = 6);
        Task<ProductDetailsViewModel> GetProductDetailsAsync(int productId);
        Task<ProductDTO?> GetProductById(int id);
        Task<bool> AddProduct(ProductDTO model, List<IFormFile>? images);
        Task<bool> UpdateProduct(ProductDTO model);
        Task<bool> DeleteProduct(int id);
    }
}
