using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Enums;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllOrders();
        Task<OrderDTO?> GetOrderById(int id);
        Task UpdateTransactionIdAsync(int orderId, string paymentMethod, string transactionId);
        Task<int> CreateOrder(OrderDTO order);
        Task<bool> UpdateOrderStatus(int id, string status);
        Task<bool> UpdateOrderQuantity(OrderDTO order);
        Task<bool> DeleteOrder(int id);

        Task<List<OrderDTO>> GetOrdersByUserIdAsync(string userId);
        Task<bool> CompleteOrder(int id);
        Task<bool> CancelOrder(int id);
        

    }
}
