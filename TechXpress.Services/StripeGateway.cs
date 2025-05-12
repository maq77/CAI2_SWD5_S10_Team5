using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Services
{
    public class StripeGateway : IPaymentGateway
    {
        private readonly IStripeClient _stripeClient;
        private readonly string _webhookSecret;

        public string Name => "Stripe";

        public StripeGateway(IStripeClient stripeClient, IOptions<StripeSettings> stripeSettings)
        {
            _stripeClient = stripeClient;
            _webhookSecret = stripeSettings.Value.WebhookSecret;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)((request.Amount) * 100), // Convert to cents
                        Currency = request.Currency.ToLower(),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = request.Description
                        }
                    },
                    Quantity = 1
                }
            },
                Mode = "payment",
                SuccessUrl = request.ReturnUrl,
                CancelUrl = request.CancelUrl,
                Metadata = new Dictionary<string, string>
            {
                { "OrderId", $"{request.OrderId}" }
            }
            };

            var service = new SessionService(_stripeClient);
            Session session = await service.CreateAsync(options);

            return new PaymentResponse
            {
                Success = true,
                TransactionId = session.Id,
                RedirectUrl = session.Url
            };
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(HttpRequest request)
        {
            // Handle webhook verification
            try
            {
                // Read the request body directly as a stream
                string json;
                request.EnableBuffering();
                using (var reader = new StreamReader(request.Body, leaveOpen: true))
                {
                    json = await reader.ReadToEndAsync();
                    // Reset the position for potential future reads
                    request.Body.Position = 0;
                }

                // Get the Stripe signature from headers
                // Extract the raw signature header value - this is critical
                string signatureHeader = request.Headers["Stripe-Signature"];

                if (string.IsNullOrEmpty(signatureHeader))
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Stripe-Signature header is missing"
                    };
                }

                // Verify the event
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signatureHeader,
                    _webhookSecret
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;

                    return new PaymentResponse
                    {
                        Success = true,
                        TransactionId = session.Id
                    };
                }

                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Event type '{stripeEvent.Type}' not supported"
                };
            }
            catch (StripeException ex)
            {
                // Log the specific Stripe exception
                Console.WriteLine($"Stripe Error: {ex.StripeError?.Message ?? ex.Message}");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing webhook: {ex.Message}");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }

}
