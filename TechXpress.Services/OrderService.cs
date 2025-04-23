using Microsoft.EntityFrameworkCore;
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

        public async Task<int> CreateOrder(OrderDTO orderDto)
        {
            var transaction = await _unitOfWork.BeginTransactionAsync();
            bool transactionCompleted = false;

            try
            {
                if (orderDto.OrderDetails.Any())
                {
                    var order_details = orderDto.OrderDetails.Select(i => new OrderDetail
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList();

                    var order = new Order
                    {
                        UserId = orderDto.UserId,
                        TotalAmount = orderDto.OrderDetails.Sum(i => i.Quantity * i.Price),
                        OrderDate = DateTime.UtcNow,
                        Status = OrderStatus.Pending,
                        OrderDetails = order_details
                    };

                    foreach (var order_item in order_details)
                    {
                        var prod = await _unitOfWork.Products.GetById(order_item.ProductId);
                        if (prod != null)
                        {
                            if (prod.StockQuantity < order_item.Quantity)
                            {
                                throw new Exception($"Not enough stock for product {prod.Name}. Available: {prod.StockQuantity}");
                            }

                            prod.StockQuantity -= order_item.Quantity;
                            await _unitOfWork.Products.Update(prod, log => Console.WriteLine(log));
                        }
                    }

                    await _unitOfWork.Orders.Add(order, log => Console.WriteLine(log));

                    Console.WriteLine($"Order Total: {order.TotalAmount}");
                    Console.WriteLine($"Order Items Count: {order.OrderDetails.Count}");
                    foreach (var item in order.OrderDetails)
                    {
                        Console.WriteLine($"Item: ProductId={item.ProductId}, Qty={item.Quantity}, Price={item.Price}");
                    }


                    bool result = await _unitOfWork.SaveAsync();
                    /////////////////////// Error Creating
                    //if (!result)
                    //{
                    //    throw new Exception("Error saving order");
                    //}

                    await transaction.CommitAsync();
                    transactionCompleted = true;

                    return order.Id;
                }
                else
                {
                    throw new Exception("No Order Details");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order: {ex.Message}");
                throw; // Rethrow the original exception
            }
            finally
            {
                if (!transactionCompleted)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception rollbackEx)
                    {
                        Console.WriteLine($"Rollback failed: {rollbackEx.Message}");
                    }
                }
            }
        }


        public async Task<bool> UpdateOrder(OrderDTO orderDto)
        {
            var order = await _unitOfWork.Orders.GetById(orderDto.Id);
            if (order == null) return false;

            order.Status = orderDto.Status;
            return await _unitOfWork.SaveAsync();
        }
        public async Task<bool> UpdateOrderStatus(int id , OrderStatus status)
        {
            var order = await _unitOfWork.Orders.GetById(id);
            if (order == null) return false;

            order.Status = status;
            return await _unitOfWork.SaveAsync();
        }
        public async Task<bool> UpdateOrderQuantity(OrderDTO orderDto)
        {
            var order = await _unitOfWork.Orders.GetById(orderDto.Id);
            if (order == null) return false;

            //order.OrderDetails = orderDto.Status;
            return await _unitOfWork.SaveAsync();
        }

        public async Task<List<OrderDTO>> GetOrdersByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            var orders = await _unitOfWork.Orders.GetAllOrders(
                o => o.UserId == userId,
                includes: new[] { "OrderDetails", "OrderDetails.Product" }
            );

            //  Ensure we always return a valid list, preventing NullReferenceExceptions
            if (orders == null || !orders.Any())
            {
                return new List<OrderDTO>();
            }

            //  Use `.AsNoTracking()` in `GetAllOrders()` to improve performance if not modifying data
            return orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                UserId = o.UserId,
                TotalAmount = o.TotalAmount,
                OrderDate = o.OrderDate,
                Status = o.Status,
                OrderDetails = o.OrderDetails?.Select(d => new OrderDetailDTO
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList() ?? new List<OrderDetailDTO>() // Ensure list is never null
            }).ToList();
        }


        public async Task<bool> DeleteOrder(int id)
        {
            var transaction = await _unitOfWork.BeginTransactionAsync();
            bool transactionCompleted = false;

            try
            {
                var order = await _unitOfWork.Orders.GetById(id);
                if (order == null)
                {
                    return false;
                }
                if (order.OrderDetails != null)
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        var product = await _unitOfWork.Products.GetById(detail.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += detail.Quantity;
                            await _unitOfWork.Products.Update(product, log => Console.WriteLine(log));
                        }
                    }
                }

                await _unitOfWork.Orders.Delete(id, log => Console.WriteLine(log));
                bool result = await _unitOfWork.SaveAsync();
                  ////////////////////////////////Error saving
                //if (!result)
                //{
                //    throw new Exception("Error deleting order.");
                //}

                await transaction.CommitAsync();
                transactionCompleted = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting order: {ex.Message}");
                throw;
            }
            finally
            {
                if (!transactionCompleted)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception rollbackEx)
                    {
                        Console.WriteLine($"Rollback failed: {rollbackEx.Message}");
                    }
                }
            }
        }

    }
}
