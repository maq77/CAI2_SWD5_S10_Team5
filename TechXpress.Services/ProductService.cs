using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using X.PagedList;
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core;


namespace TechXpress.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "ProductList";

        public ProductService(IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
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
            if (!_cache.TryGetValue(CacheKey, out IEnumerable<ProductDTO> products))
            {
                var productEntities = await _unitOfWork.Products.GetAllProducts();
                products = productEntities.Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? "Uncategorized",
                    Images = p.Images.Select(img => new ProductImageDTO
                    {
                        Id = img.Id,
                        ImagePath = img.ImagePath
                    }).ToList()
                });

                _cache.Set(CacheKey, products, TimeSpan.FromMinutes(10)); // Cache for 10 minutes
            }

            return products;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsByCategory(int categoryId)
        {
            var products = await _unitOfWork.Products.GetAllProducts();

            return products
                .Where(p => p.CategoryId == categoryId) // Filter by category
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                    Images = p.Images.Select(img => new ProductImageDTO
                    {
                        Id = img.Id,
                        ImagePath = img.ImagePath
                    }).ToList()
                });
        }

        public async Task<IEnumerable<ProductDTO>> SearchProducts(string? searchTerm)
        {
            var products = await _unitOfWork.Products.GetAllProducts();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(searchTerm));
            }

            return products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                Images = p.Images.Select(img => new ProductImageDTO
                {
                    Id = img.Id,
                    ImagePath = img.ImagePath
                }).ToList()
            });
        }
        public async Task<IPagedList<ProductDTO>> GetPagedProducts(int pageNumber, int pageSize, int? categoryId, string? searchTerm)
        {
            var products = await _unitOfWork.Products.GetAllProducts();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(searchTerm));
            }

            var productDTOs = products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                Images = p.Images.Select(img => new ProductImageDTO
                {
                    Id = img.Id,
                    ImagePath = img.ImagePath
                }).ToList()
            });

            return  productDTOs.ToPagedList(pageNumber, pageSize);
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

        public async Task<bool> UpdateProduct(ProductDTO model)
        {
            var product = await _unitOfWork.Products.GetById(model.Id);
            if (product == null) return false;

            product.Name = model.Name;
            product.Price = model.Price;
            product.CategoryId = model.CategoryId;

            await _unitOfWork.Products.Update(product, log=>Console.WriteLine(log));
            var saved = await _unitOfWork.SaveAsync();

            if (saved)
            {
                _cache.Remove(CacheKey); 
            }

            return saved;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            await _unitOfWork.Products.Delete(id, log => Console.WriteLine(log));
            return await _unitOfWork.SaveAsync();
        }
    }
}
