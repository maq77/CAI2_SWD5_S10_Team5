using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Services
{
    public class ProductService : IProductService
    {
        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        private readonly IUnitOfWork _unitOfWork;
        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await _unitOfWork.Products.GetAll();
        }

        public async Task<Product?> GetProductById(int id)
        {
            return await _unitOfWork.Products.GetById(id);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryId(int categoryId)
        {
            return await _unitOfWork.Products.GetProductsByCategoryId(categoryId);
        }
    
        public async Task AddProduct(ProductDTO product, List<IFormFile>? image)
        {
            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                CategoryId = model.CategoryId
            };
            //Save Image and Store Path in DB
            if (image != null && image.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine("wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                product.ImagePath = "/images/" + fileName; //  Save only image path in DB
            }
            await _unitOfWork.Products.Add(product, log => Console.WriteLine(log));
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateProduct(Product product)
        {
            await _unitOfWork.Products.Update(product, log=>Console.WriteLine(log));
        }
        public async Task DeleteProduct(int id)
        {
            await _unitOfWork.Products.Delete(id,log=>Console.WriteLine(log));
        }
    }
}
