using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface ICartService
    {
        List<OrderDetailDTO> GetCart();
        bool ClearCart();
        int GetCartItemCount();
        double GetCartTotal();
        Task<bool> AddToCart(OrderDetailDTO item);
        bool UpdateCart(List<OrderDetailDTO> updatedCart);
        bool UpdateQuantity(int productId, int quantity);
        bool RemoveFromCart(int productId);
        OrderDetailDTO? GetCartItem(int productId);
    }
}
