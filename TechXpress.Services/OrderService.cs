using TechXpress.Data.Enums;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrders()
        {
            var orders = await _unitOfWork.Orders.GetAllOrders();
            return orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                UserId = o.UserId,
                TotalAmount = o.TotalAmount,
                OrderDate = o.OrderDate,
                Status = o.Status,
                OrderDetails = o.OrderDetails.Select(d => new OrderDetailDTO
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList()
            });
        }

        public async Task<OrderDTO?> GetOrderById(int id)
        {
            var order = await _unitOfWork.Orders.GetById(id);
            if (order == null) return null;

            return new OrderDTO
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,
                Status = order.Status,
                OrderDetails = order.OrderDetails.Select(d => new OrderDetailDTO
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList()
            };
        }

        public async Task<bool> CreateOrder(OrderDTO orderDto)
        {
            var order = new Order
            {
                UserId = orderDto.UserId,
                TotalAmount = orderDto.OrderDetails.Sum(i => i.Quantity * i.Price),
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                OrderDetails = orderDto.OrderDetails.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            await _unitOfWork.Orders.Add(order,log=>Console.WriteLine(log));
            return await _unitOfWork.SaveAsync();
        }

        public async Task<bool> UpdateOrder(OrderDTO orderDto)
        {
            var order = await _unitOfWork.Orders.GetById(orderDto.Id);
            if (order == null) return false;

            order.Status = orderDto.Status;
            return await _unitOfWork.SaveAsync();
        }

        public async Task<bool> DeleteOrder(int id)
        {
            await _unitOfWork.Orders.Delete(id, log => Console.WriteLine(log));
            return await _unitOfWork.SaveAsync();
        }
    }
}
