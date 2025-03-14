﻿using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface ICartService
    {
        List<OrderDetailDTO> GetCart();
        void AddToCart(OrderDetailDTO item);
        void UpdateCart(List<OrderDetailDTO> updatedCart);
        void RemoveFromCart(int productId);
        void ClearCart();
    }
}
