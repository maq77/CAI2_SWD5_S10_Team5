﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ProductService> _logger;
        private const string ProductCacheKey = "ProductList";
        private const string PopularProductCacheKey = "PopularProducts";
        private const string CategoryCacheKey = "Categories";

        public ProductService(IUnitOfWork unitOfWork, IMemoryCache cache, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> AddProduct(ProductDTO model, List<IFormFile>? images)
        {
            var category = await _unitOfWork.Categories.GetById(model.CategoryId);
            if (category == null)
            {
                _logger.LogWarning($"Category with ID {model.CategoryId} not found.");
                return false;
            }

            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                CategoryId = model.CategoryId
            };

            await _unitOfWork.Products.Add(product, log => _logger.LogInformation(log));
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
                    await _unitOfWork.ProductImages.AddRange(productImages, log => _logger.LogInformation(log));
                    await _unitOfWork.SaveAsync();
                }
            }
            _cache.Remove(ProductCacheKey);
            _cache.Remove(PopularProductCacheKey);
            _logger.LogInformation($"Product '{product.Name}' added successfully.");
            return true;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProducts()
        {
            _logger.LogInformation("Fetching all products.");
            if (!_cache.TryGetValue(ProductCacheKey, out IEnumerable<ProductDTO>? products)|| products == null)
            {
                _logger.LogInformation("Cache miss for all products. Fetching from database.");
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
                        ImagePath = img.ImagePath ?? "/images/default-product.png"
                    }).ToList()
                });

                // Cache for 10 minutes
                _cache.Set(ProductCacheKey, products, TimeSpan.FromMinutes(10));
                _logger.LogInformation("Cached all products for 10 minutes.");
            }
            else
            {
                _logger.LogInformation("Cache hit for all products.");
            }

            return products;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsByCategory(int categoryId)
        {
            _logger.LogInformation($"Fetching products for category ID: {categoryId}");
            var products = await _unitOfWork.Products.GetAllProducts();

            var filtered_products = products
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
            _logger.LogInformation($"Retrieved {filtered_products.Count()} products for category ID: {categoryId}");
            return filtered_products;
        }

        public async Task<IEnumerable<ProductDTO>> SearchProducts(string? searchTerm)
        {
            _logger.LogInformation($"Searching products with term: {searchTerm}");
            var products = await _unitOfWork.Products.GetAllProducts();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(searchTerm));
            }

            var result = products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "Uncategorized",
                Images = p.Images.Select(img => new ProductImageDTO
                {
                    Id = img.Id,
                    ImagePath = img.ImagePath ?? "/images/default-product.png"
                }).ToList()
            });

            _logger.LogInformation($"Found {result.Count()} products matching search term: {searchTerm}");
            return result;
        }
        public async Task<IPagedList<ProductDTO>> GetPagedProducts(int pageNumber, int pageSize, int? categoryId, string? searchTerm)
        {
            _logger.LogInformation($"Fetching paged products: Page {pageNumber}, Page Size {pageSize}, Category ID: {categoryId}, Search Term: {searchTerm}");

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
            _logger.LogInformation($"Returning paged product list with {productDTOs.Count()} items.");
            return  productDTOs.ToPagedList(pageNumber, pageSize);
        }

        public async Task<IEnumerable<ProductDTO>> GetPopularProducts(int top = 6)
        {
            _logger.LogInformation($"Fetching top {top} popular products.");
            if (!_cache.TryGetValue(PopularProductCacheKey, out IEnumerable<ProductDTO>? popularProducts)||popularProducts==null)
            {
                _logger.LogInformation("Cache miss for popular products. Fetching from database.");
                var products = await _unitOfWork.Products.GetAllProducts();
                var orderDetails = await _unitOfWork.OrderDetails.GetAll();

                var salesData = orderDetails
                    .GroupBy(o => o.ProductId)
                    .Select(g => new { ProductId = g.Key, TotalSales = g.Sum(o => o.Quantity) })
                    .ToDictionary(x => x.ProductId, x => x.TotalSales);

                popularProducts = products
                    .OrderByDescending(p => salesData.ContainsKey(p.Id) ? salesData[p.Id] : 0)
                    .Take(top)
                    .Select(p => new ProductDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category?.Name ?? "Uncategorized",
                        SalesCount = salesData.ContainsKey(p.Id) ? salesData[p.Id] : 0,
                        Images = p.Images.Select(img => new ProductImageDTO
                        {
                            Id = img.Id,
                            ImagePath = img.ImagePath ?? "/images/default-product.png"
                        }).ToList()
                    });

                //  Cache for 30 minutes
                _cache.Set(PopularProductCacheKey, popularProducts, TimeSpan.FromMinutes(30));
                _logger.LogInformation("Cached popular products for 30 minutes.");
            }
            else
            {
                _logger.LogInformation("Cache hit for popular products.");
            }

            return popularProducts;
        }
        public async Task<ProductDTO?> GetProductById(int id)
        {
            _logger.LogInformation($"Fetching product with ID: {id}");
            var product = await _unitOfWork.Products.GetById(id);
            if (product == null)
            {
                _logger.LogWarning($"Product with ID {id} not found.");
                return null;
            }
            _logger.LogInformation($"Product with ID {id} found.");
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
            if (product == null)
            {
                _logger.LogWarning($"Product with ID {model.Id} not found.");
                return false;
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.CategoryId = model.CategoryId;
            

            await _unitOfWork.Products.Update(product, log=> _logger.LogInformation(log));
            await _unitOfWork.SaveAsync();

            _cache.Remove(ProductCacheKey);
            _cache.Remove(PopularProductCacheKey);
            _logger.LogInformation($"Product '{product.Name}' updated successfully.");


            return true;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            await _unitOfWork.Products.Delete(id, log => _logger.LogInformation(log));
            var saved = await _unitOfWork.SaveAsync();
            if (saved)
            {
                _cache.Remove(ProductCacheKey);
                _cache.Remove(PopularProductCacheKey);
                _logger.LogInformation($"Product with ID {id} deleted successfully.");
            }
            else
            {
                _logger.LogError($"Failed to delete product with ID {id}.");
            }
            return saved;
        }



    }
}
