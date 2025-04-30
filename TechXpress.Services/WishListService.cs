using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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
    public class WishListItemService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<WishListItemService> _logger;
        private const string WishListCacheKey = "WishList_{0}";

        public WishListItemService(IUnitOfWork unitOfWork, IMemoryCache cache, ILogger<WishListItemService> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> AddToWishlistAsync(string userId, int productId)

        {

            _logger.LogInformation("Received productId: {ProductId}", productId);

            if (productId == 0)
            {
                _logger.LogError("Invalid productId received.");
                return false;
            }

            _logger.LogInformation("Checking if product {ProductId} exists before adding to wishlist.", productId);

            var productExists = await _unitOfWork.Products.Find_First(p => p.Id == productId);
            if (productExists is null)
            {
                _logger.LogError("Product {ProductId} does not exist. Cannot add to wishlist.", productId);
                return false;
            }

            bool exists = await IsInWishlistAsync(userId, productId);
            if (exists)
            {
                _logger.LogWarning("Product {ProductId} is already in wishlist.", productId);
                return false;
            }

            _logger.LogInformation("Adding product {ProductId} to wishlist for user {UserId}.", productId, userId);


            var wishlistItem = new WishListItem
            {
                UserId = userId,
                ProductId = productId,
                DateAdded = DateTime.UtcNow
            };
            await _unitOfWork.WishListItems.Add(wishlistItem, log => _logger.LogInformation(log));
            await _unitOfWork.SaveAsync();

            _cache.Remove(string.Format(WishListCacheKey, userId));
            _logger.LogInformation($"Product {productId} added to the wishlist of user {userId}.");
            return true;
        }

        public async Task<bool> RemoveFromWishlistAsync(string userId, int productId)
        {
            var item = await _unitOfWork.WishListItems.Find_First(w => w.UserId == userId && w.ProductId == productId);
            if (item == null)
            {
                _logger.LogWarning($"Product {productId} not found in the wishlist of user {userId}.");
                return false;
            }

            await _unitOfWork.WishListItems.Delete(item.Id , log => _logger.LogInformation(log));
            await _unitOfWork.SaveAsync();

            _cache.Remove(string.Format(WishListCacheKey, userId));
            _logger.LogInformation($"Product {productId} removed from the wishlist of user {userId}.");
            return true;
        }

        public async Task<IEnumerable<WishListItemDTO>> GetWishlistAsync(string userId)
        {
            if (!_cache.TryGetValue(string.Format(WishListCacheKey, userId), out IEnumerable<WishListItemDTO>? wishlist) || wishlist == null)
            {
                var wishlistItems = await _unitOfWork.WishListItems
                    .GetAll(w => w.UserId == userId, include: q => q.Include(w => w.Product)); // Ensure Product is included

                wishlist = wishlistItems.Select(w => new WishListItemDTO
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    ProductId = w.ProductId,
                    ProductName = w.Product?.Name ?? "Unknown Product", // Avoid null reference
                    AddedDate = w.DateAdded
                }).ToList();

                _cache.Set(string.Format(WishListCacheKey, userId), wishlist, TimeSpan.FromMinutes(1));
                _logger.LogInformation($"Cached wishlist for user {userId}.");
            }

            return wishlist;
        }

        public async Task<bool> IsInWishlistAsync(string userId, int productId)
        {
            var exists = await _unitOfWork.WishListItems.Find_First(w => w.UserId == userId && w.ProductId == productId);
            if (exists == null) { return false; }
            return true;
        }
    }
}
