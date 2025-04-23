using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "CartItems";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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

        public void AddToCart(OrderDetailDTO item)
        {
            var cart = GetCart();
            var existingItem = cart.Find(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                cart.Add(item);
            }
            SaveCart(cart);
        }

        public void UpdateCart(List<OrderDetailDTO> updatedCart)
        {
            SaveCart(updatedCart);
        }
        public void UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                RemoveFromCart(productId);
                return;
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.ProductId == productId);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(CartSessionKey);
        }

        private void SaveCart(List<OrderDetailDTO> cart)
        {
            _httpContextAccessor.HttpContext?.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
        }

        public int GetCartItemCount()
        {
            return GetCart().Sum(item => item.Quantity);
        }
    }
}
