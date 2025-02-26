using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllOrders();
        Task<OrderDTO?> GetOrderById(int id);
        Task<bool> CreateOrder(OrderDTO order);
        Task<bool> UpdateOrder(OrderDTO order);
        Task<bool> DeleteOrder(int id);
    }
}
