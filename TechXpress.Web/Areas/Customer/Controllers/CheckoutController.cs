using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using TechXpress.Data.Model;
using TechXpress.Data.Enums;
using TechXpress.Services.DTOs.ViewModels;
using System.Linq.Expressions;
using System.Security.Claims;

namespace YourNamespace.Controllers
{
    [Authorize]
    [Area("Customer")]
    //[Route("[area]/[controller]/[action]")]
    public class CheckoutController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<StripeSettings> _stripeSettings;
        private readonly IOptions<PayPalSettings> _paypalSettings;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            IPaymentService paymentService,
            IOrderService orderService,
            ICartService cartService,
            UserManager<User> userManager,
            IOptions<StripeSettings> stripeSettings,
            IOptions<PayPalSettings> paypalSettings,
            ILogger<CheckoutController> logger)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
            _stripeSettings = stripeSettings;
            _paypalSettings = paypalSettings;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get cart items from your existing cart service
            var cartItems = _cartService.GetCart();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var checkoutModel = new CheckoutViewModel
            {
                CartItems = cartItems,
                AvailablePaymentGateways = _paymentService.GetAvailableGateways(),
                StripePublishableKey = _stripeSettings.Value.PublishableKey,
                PayPalClientId = _paypalSettings.Value.ClientId,
                PayPalSandbox = _paypalSettings.Value.UseSandbox
            };
            ViewBag.PaymentGateways = _paymentService.GetAvailableGateways();
            return View(checkoutModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string gatewayName)
        {
            try{
                // Create a new order in pending state
                var user = await _userManager.GetUserAsync(User);
                var items = _cartService.GetCart();
                var totalAmount = _cartService.GetCartTotal();
                if (items == null || !items.Any())
                {
                    return RedirectToAction("Index", "Cart");
                }
                var orderDto = new OrderDTO
                {
                    UserId = user.Id,
                    OrderDetails = items.Select(i => new OrderDetailDTO
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList(),
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending,
                    shipping_address = "DUMP Data", // Replace with actual shipping address
                    OrderDate = DateTime.UtcNow,
                    paymentMethod = gatewayName
                };
                
                var returnUrl = Url.Action("PaymentCallback", "Checkout",
                        new { area = "Customer" }, Request.Scheme);
                var cancelUrl = Url.Action("PaymentCancelled", "Checkout",
                    new { area = "Customer" }, Request.Scheme);
                // Prepare payment request
                var paymentRequest = new PaymentRequest
                {
                    //OrderId = order.Id,
                    Amount = Math.Round((decimal)totalAmount, 2, MidpointRounding.AwayFromZero),
                    Currency = "USD",
                    Description = $"TechXpress Order - {DateTime.Now:yyyy-MM-dd}",
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl
                };

                // Process payment through selected gateway
                
                var paymentResult = await _paymentService.ProcessPaymentAsync(gatewayName, paymentRequest);
                

                if (paymentResult.Success && !string.IsNullOrEmpty(paymentResult.RedirectUrl))
                {
                    // Store order information in TempData for later use
                    TempData["PendingOrder"] = System.Text.Json.JsonSerializer.Serialize(orderDto);
                    TempData["PaymentGateway"] = gatewayName;

                    // Redirect to payment gateway
                    return Redirect(paymentResult.RedirectUrl);
                }
                else
                {
                    // Handle payment initialization errors
                    _logger.LogWarning($"Payment initialization failed: {paymentResult.ErrorMessage}");

                    return RedirectToAction("Error", "Home", new
                    {
                        area = "",
                        message = $"Failed to initialize payment: {paymentResult.ErrorMessage}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessPayment");
                return RedirectToAction("Error", "Home", new
                {
                    area = "",
                    message = "An unexpected error occurred while processing your payment."
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                // Get payment gateway name from TempData
                var gatewayName = TempData["PaymentGateway"]?.ToString();
                if (string.IsNullOrEmpty(gatewayName))
                {
                    return RedirectToAction("Error", "Home", new
                    {
                        area = "",
                        message = "Payment session expired or invalid."
                    });
                }
                if (gatewayName == "Stripe" && string.IsNullOrEmpty(Request.Query["session_id"]))
                {
                    return RedirectToAction("Error", "Home", new
                    {
                        area = "",
                        message = "Stripe session ID not found in the callback URL."
                    });
                }

                // Verify payment was successful
                var verificationResult = await _paymentService.VerifyPaymentAsync(gatewayName, HttpContext.Request);

                if (verificationResult.Success || gatewayName =="Cash on Delivery")
                {
                    // Get pending order from TempData
                    var orderJson = TempData["PendingOrder"]?.ToString();
                    if (string.IsNullOrEmpty(orderJson))
                    {
                        return RedirectToAction("Error", "Home", new
                        {
                            area = "",
                            message = "Order information not found. Please contact customer support."
                        });
                    }

                    // Deserialize order
                    var orderDto = System.Text.Json.JsonSerializer.Deserialize<OrderDTO>(orderJson);

                    try
                    {
                        // Create the order in database now that payment is confirmed
                        var orderId = await _orderService.CreateOrder(orderDto);
                        if (gatewayName != "Cash")
                        {
                            await _orderService.UpdateTransactionIdAsync(orderId, gatewayName, verificationResult.TransactionId);
                        }
                        // Clear the shopping cart
                        _cartService.ClearCart();

                        // Redirect to order confirmation
                        return RedirectToAction("OrderConfirmation", new { id = orderId, verificationResult.TransactionId });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating order after successful payment");
                        return RedirectToAction("Error", "Home", new
                        {
                            area = "",
                            message = "Payment successful but we couldn't create your order. Please contact customer support with your payment ID: " + verificationResult.TransactionId
                        });
                    }
                }
                else
                {
                    // Payment verification failed
                    if (!string.IsNullOrEmpty(verificationResult.ErrorCode) &&
                        verificationResult.ErrorCode == "COMPLIANCE_VIOLATION")
                    {
                        return RedirectToAction("Error", "Home", new
                        {
                            area = "",
                            message = "Your payment could not be processed due to security restrictions. Please try a different payment method."
                        });
                    }

                    return RedirectToAction("Error", "Home", new
                    {
                        area = "",
                        message = $"Failed to verify payment: {verificationResult.ErrorMessage}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PaymentCallback");
                return RedirectToAction("Error", "Home", new
                {
                    area = "",
                    message = "An unexpected error occurred while processing your payment callback."
                });
            }
        }
        [HttpGet]
        public IActionResult PaymentCancelled()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int id, string TransactionId)
        {
            var order = await _orderService.GetOrderById(id);
            ViewBag.TransactionId = TransactionId;
            ViewBag.OrderId = id;
            if (order == null)
            {
                return RedirectToAction("Error", "Home", new
                {
                    area = "",
                    message = "Order not found."
                });
            }

            return View(order);
        }
        [HttpGet]
        public async Task<IActionResult> Complete(int orderId, string gateway)
        {
            // Verify payment
            var paymentResponse = await _paymentService.VerifyPaymentAsync(
                gateway,Request);

            if (paymentResponse.Success)
            {
                // Complete the order
                await _orderService.CompleteOrder(orderId);

                // Clear cart
                _cartService.ClearCart();

                return View("Success", await _orderService.GetOrderById(orderId));
            }

            return RedirectToAction("Error", "Home", new { message = paymentResponse.ErrorMessage });
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int orderId)
        {
            // Cancel the order
            await _orderService.CancelOrder(orderId);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> WebhookHandler(string gateway)
        {
            // Process webhooks from payment gateways
            var paymentResponse = await _paymentService.VerifyPaymentAsync(
                gateway,
                Request);

            if (paymentResponse.Success)
            {
                // Handle successful webhook - typically this would update order status
                // ...
                return Ok();
            }

            return BadRequest(paymentResponse.ErrorMessage);
        }
    }
}