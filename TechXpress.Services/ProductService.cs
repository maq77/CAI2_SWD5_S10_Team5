using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;


namespace TechXpress.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddProduct(ProductDTO model, List<IFormFile>? images)
        {
            var category = await _unitOfWork.Categories.GetById(model.CategoryId);
            if (category == null) return false;

            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                CategoryId = model.CategoryId
            };

            await _unitOfWork.Products.Add(product,log=>Console.WriteLine(log));
            await _unitOfWork.SaveAsync();

            if (images != null && images.Count > 0)
            {
                var productImages = new List<ProductImage>();

                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        var filePath = Path.Combine("wwwroot/images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        productImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImagePath = "/images/" + fileName
                        });
                    }
                }

                if (productImages.Any())
                {
                    await _unitOfWork.ProductImages.AddRange(productImages, log => Console.WriteLine(log));
                    await _unitOfWork.SaveAsync();
                }
            }
            return true;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProducts()
        {
            var products = await _unitOfWork.Products.GetAll();
            return products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Uncategorized", ///Projection
                Images = p.Images.Select(img => new ProductImageDTO
                {
                    Id = img.Id,
                    ImagePath = img.ImagePath
                }).ToList()
            });
        }

        public async Task<ProductDTO?> GetProductById(int id)
        {
            var product = await _unitOfWork.Products.GetById(id);
            if (product == null) return null;

            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                CategoryId = product.CategoryId,
                CategoryName = product.Category != null ? product.Category.Name : "Uncategorized",///Projection
                Images = product.Images.Select(img => new ProductImageDTO
                {
                    Id = img.Id,
                    ImagePath = img.ImagePath
                }).ToList()
            };
        }

        public async Task<bool> UpdateProduct(ProductDTO model, List<IFormFile>? images)
        {
            var product = await _unitOfWork.Products.GetById(model.Id);
            if (product == null) return false;

            product.Name = model.Name;
            product.Price = model.Price;
            product.CategoryId = model.CategoryId;

            await _unitOfWork.Products.Update(product, log => Console.WriteLine(log));
            await _unitOfWork.SaveAsync();

            if (images != null && images.Count > 0)
            {
                var productImages = new List<ProductImage>();

                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        var filePath = Path.Combine("wwwroot/images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        productImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImagePath = "/images/" + fileName
                        });
                    }
                }

                if (productImages.Any())
                {
                    await _unitOfWork.ProductImages.AddRange(productImages, log => Console.WriteLine(log));
                    await _unitOfWork.SaveAsync();
                }
            }
            return true;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            await _unitOfWork.Products.Delete(id, log => Console.WriteLine(log));
            return await _unitOfWork.SaveAsync();
        }
    }
}
