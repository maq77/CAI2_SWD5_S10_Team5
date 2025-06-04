using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private const string CartSessionKey = "CartItems";
        private readonly ILogger<CartService> _logger;

        public CartService(IHttpContextAccessor httpContextAccessor, ILogger<CartService> logger , IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor; 
            _logger = logger;
            _unitOfWork = unitOfWork; 
        }

        public OrderDetailDTO? GetCartItem(int productId)
        {
            var cart = GetCart();
            return cart.Find(i => i.ProductId == productId);
        }
        public async Task<bool> AddToCart(OrderDetailDTO item)
        {
            _logger.LogInformation("Received productId: {ProductId}", item.ProductId);
            if (item.ProductId == 0)
            {
                _logger.LogError("Invalid productId received.");
                return false;
            }
            var cart = GetCart();
            _logger.LogInformation("Checking if product {ProductId} exists before adding to Cart.", item.ProductId);
            /*var productExists = await _unitOfWork.Products.Find_First(p => p.Id == item.ProductId);
            if (productExists is null)
            {
                _logger.LogError("Product {ProductId} does not exist. Cannot add to Cart.", item.ProductId);
                return false;
            }*/
            var existingItem = cart.Find(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                _logger.LogInformation($"Product {item.ProductId} already Exist in cart. Adding Quantity {item.Quantity}");
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                _logger.LogInformation($"Product {item.ProductId} added to cart. Adding Quantity {item.Quantity}");
                cart.Add(item);
            }
            _logger.LogInformation($"Product {item.ProductId} added to cart. Saved Successfully!");
            return SaveCart(cart);
        }

        public bool UpdateCart(List<OrderDetailDTO> updatedCart)
        {
            return SaveCart(updatedCart);
        }
        public bool UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                RemoveFromCart(productId);
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity = quantity;
                return SaveCart(cart);
            }
            else
            {
                return false;
            }
        }

        public bool RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.ProductId == productId);
            return SaveCart(cart);
            
        }


        ///Private methods
        public int GetCartItemCount()
        {
            return GetCart().Sum(item => item.Quantity);
        }
        public double GetCartTotal()
        {
            return GetCart().Sum(item => item.Quantity * item.Price);
        }
        public List<OrderDetailDTO> GetCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var cartJson = session?.GetString(CartSessionKey);
            if (cartJson != null)
            {
                return JsonConvert.DeserializeObject<List<OrderDetailDTO>>(cartJson);
            }
            else
            {
                return new List<OrderDetailDTO>();
            }
        }
        private bool SaveCart(List<OrderDetailDTO> cart)
        {
            _httpContextAccessor.HttpContext?.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
            return true;
        }
        public bool ClearCart()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(CartSessionKey);
            return true;
        }

    }
}
