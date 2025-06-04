using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
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
        private readonly ILogger<OrderService> _logger;
        private readonly IErrorLoggingService _errorLoggingService;

        public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger, IErrorLoggingService errorLoggingService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _errorLoggingService = errorLoggingService;

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
                shipping_address = o.ShippingAddress,
                paymentMethod = o.PaymentMethod,
                TransactionId = o.TransactionId,
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
                shipping_address =  order.ShippingAddress,
                paymentMethod = order.PaymentMethod,
                TransactionId = order.TransactionId,
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
            if (orderDto == null)
                throw new ArgumentNullException(nameof(orderDto));

            if (!orderDto.OrderDetails?.Any() == true)
                throw new ArgumentException("Order must contain at least one item", nameof(orderDto));

            var strategy = _unitOfWork.GetContext().Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var orderDetails = orderDto.OrderDetails.Select(item => new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList();

                    var order = new Order
                    {
                        UserId = orderDto.UserId,
                        TotalAmount = orderDetails.Sum(item => item.Quantity * item.Price),
                        OrderDate = DateTime.UtcNow,
                        Status = OrderStatus.Pending,
                        ShippingAddress = orderDto.shipping_address,
                        PaymentMethod = orderDto.paymentMethod,
                        TransactionId = orderDto.TransactionId,
                        OrderDetails = orderDetails
                    };

                    var productIds = orderDetails.Select(item => item.ProductId).Distinct().ToList();
                    var products = await _unitOfWork.Products.GetByIds(productIds);

                    var productDict = products.ToDictionary(p => p.Id, p => p);

                    foreach (var orderItem in orderDetails)
                    {
                        if (!productDict.TryGetValue(orderItem.ProductId, out var product))
                        {
                            throw new InvalidOperationException($"Product with ID {orderItem.ProductId} not found");
                        }

                        if (product.StockQuantity < orderItem.Quantity)
                        {
                            throw new InvalidOperationException(
                                $"Insufficient stock for product '{product.Name}'. " +
                                $"Available: {product.StockQuantity}, Requested: {orderItem.Quantity}");
                        }

                        product.StockQuantity -= orderItem.Quantity;
                        await _unitOfWork.Products.Update(product, log => _logger?.LogInformation(log));
                    }

                    await _unitOfWork.Orders.Add(order, log => _logger?.LogInformation(log));

                    _logger?.LogInformation($"Creating order - Total: {order.TotalAmount:C}, Items: {order.OrderDetails.Count}");

                    foreach (var item in order.OrderDetails)
                    {
                        _logger?.LogDebug($"Order item - ProductId: {item.ProductId}, Quantity: {item.Quantity}, Price: {item.Price:C}");
                    }

                    var saveResult = await _unitOfWork.SaveAsync();

                    //if (!saveResult)
                    //{
                    //    throw new InvalidOperationException("Failed to save order to database");
                    //}

                    // Commit transaction
                    await transaction.CommitAsync();

                    _logger?.LogInformation($"Order {order.Id} created successfully for user {orderDto.UserId}");
                    await _errorLoggingService.LogInfoAsync($"Order {order.Id} with Total - {orderDto.TotalAmount} created successfully for user {orderDto.UserId}","OrderService.CreateOrder",$"{orderDto.UserId}");

                    return order.Id;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error creating order for user {UserId}: {ErrorMessage}",
                        orderDto.UserId, ex.Message);
                    await _errorLoggingService.LogErrorAsync(ex, "OrderService.CreateOrder",$"{orderDto.UserId}");
                    throw;
                }
            });
        }

        private async Task<string> GetValidationErrors()
        {
            try
            {
                var context = _unitOfWork.GetContext(); 
                var entities = context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                    .Select(e => e.Entity);

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(context);

                foreach (var entity in entities)
                {
                    var entityValidationResults = new List<ValidationResult>();
                    var isValid = Validator.TryValidateObject(entity, new ValidationContext(entity), entityValidationResults, true);

                    if (!isValid)
                    {
                        validationResults.AddRange(entityValidationResults);
                    }
                }

                return validationResults.Any()
                    ? string.Join("; ", validationResults.Select(vr => vr.ErrorMessage))
                    : "No validation errors found";
            }
            catch (Exception ex)
            {
                return $"Could not retrieve validation errors: {ex.Message}";
            }
        }

        public async Task<bool> UpdateOrderStatus(int id, string status)
        {
            /*
             * Satus should be at sequence like :  
             * Pending -> Procc -> Shipping -> Delivered  
             */
            var order = await _unitOfWork.Orders.GetById(id);
            if (order == null) return false;

            if (!Enum.TryParse<OrderStatus>(status, out OrderStatus orderStatus))
            {
                return false; // Invalid status string
            }

            if (!IsValidStatusTransition(order.Status, orderStatus))
            {
                return false; 
            }

            order.Status = orderStatus;
            return await _unitOfWork.SaveAsync();
        }
        //Helper method
        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {

            if (currentStatus == OrderStatus.Delivered || currentStatus == OrderStatus.Canceled)
                return false;

            if (currentStatus == OrderStatus.Pending && newStatus == OrderStatus.Shipped)
                return false;

            return true;
        }

        ///Fix Func
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

            if (orders == null || !orders.Any())
            {
                return new List<OrderDTO>();
            }

            return orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                UserId = o.UserId,
                TotalAmount = o.TotalAmount,
                OrderDate = o.OrderDate,
                Status = o.Status,
                shipping_address = o.ShippingAddress,
                paymentMethod = o.PaymentMethod,
                TransactionId = o.TransactionId,
                OrderDetails = o.OrderDetails?.Select(d => new OrderDetailDTO
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList() ?? new List<OrderDetailDTO>() 
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
                await _errorLoggingService.LogErrorAsync(ex, "OrderService.DeleteOrder");
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
                        await _errorLoggingService.LogErrorAsync(rollbackEx, "OrderService.DeleteOrder");
                        Console.WriteLine($"Rollback failed: {rollbackEx.Message}");
                    }
                }
            }
        }

        public async Task UpdateTransactionIdAsync(int orderId, string paymentMethod, string transactionId)
        {
            var transaction = await _unitOfWork.BeginTransactionAsync();
            bool transactionCompleted = false;

            try
            {
                var order = await _unitOfWork.Orders.GetById(orderId);
                if (order == null) return;

                if (!IsValidStatusTransition(order.Status, OrderStatus.Delivered))
                {
                    throw new Exception(" error "); 
                }

                order.PaymentMethod = paymentMethod;
                order.TransactionId = transactionId;

                bool result = await _unitOfWork.SaveAsync();
                if (!result)
                {
                    Console.WriteLine("Warning: SaveAsync returned false when completing order, but continuing with transaction");
                }

                await transaction.CommitAsync();
                transactionCompleted = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error completing order: {ex.Message}");
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

        public async Task<bool> CompleteOrder(int id)
        {
            var transaction = await _unitOfWork.BeginTransactionAsync();
            bool transactionCompleted = false;

            try
            {
                var order = await _unitOfWork.Orders.GetById(id);
                if (order == null) return false;

                if (!IsValidStatusTransition(order.Status, OrderStatus.Delivered))
                {
                    return false;
                }

                order.Status = OrderStatus.Delivered;

                await _unitOfWork.Orders.Update(order, log => Console.WriteLine(log));
                bool result = await _unitOfWork.SaveAsync();
                if (!result)
                {
                    Console.WriteLine("Warning: SaveAsync returned false when completing order, but continuing with transaction");
                }

                await transaction.CommitAsync();
                transactionCompleted = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error completing order: {ex.Message}");
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

        public async Task<bool> CancelOrder(int id)
        {
            var transaction = await _unitOfWork.BeginTransactionAsync();
            bool transactionCompleted = false;

            try
            {
                var order = await _unitOfWork.Orders.GetById(id);
                if (order == null) return false;

                if (!IsValidStatusTransition(order.Status, OrderStatus.Canceled))
                {
                    return false; 
                }

                if (order.Status != OrderStatus.Canceled && order.OrderDetails != null)
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

                order.Status = OrderStatus.Canceled;

                await _unitOfWork.Orders.Update(order, log => Console.WriteLine(log));
                bool result = await _unitOfWork.SaveAsync();

                if (!result)
                {
                    Console.WriteLine("Warning: SaveAsync returned false when canceling order, but continuing with transaction");
                }

                await transaction.CommitAsync();
                transactionCompleted = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error canceling order: {ex.Message}");
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
