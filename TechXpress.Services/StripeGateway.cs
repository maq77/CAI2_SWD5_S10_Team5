﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Services
{
    public class StripeGateway : IPaymentGateway
    {
        private  IStripeClient _stripeClient;
        private readonly IDynamicSettingsService _dynamicSettings;

        public string Name => "Stripe";

        public StripeGateway(IDynamicSettingsService dynamicSettings)
        {
            _dynamicSettings = dynamicSettings ?? throw new ArgumentNullException(nameof(dynamicSettings));
        }

        private async Task<IStripeClient> GetStripeClientAsync()
        {
            if (_stripeClient == null)
            {
                var stripeSettings = await _dynamicSettings.GetSectionAsync<StripeSettings>("StripeSettings");
                _stripeClient = new StripeClient(stripeSettings.SecretKey);
            }
            return _stripeClient;
        }
        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                _stripeClient = await GetStripeClientAsync();
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(request.Amount * 100), // amount in cents
                                Currency = request.Currency?.ToLower() ?? "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = request.Description ?? "TechXpress Order"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = $"{request.ReturnUrl}?session_id={{CHECKOUT_SESSION_ID}}", // Stripe will append ?session_id={CHECKOUT_SESSION_ID}
                    CancelUrl = request.CancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        { "OrderId", (request.OrderId.ToString()=="0") ? Guid.NewGuid().ToString() : request.OrderId.ToString()}
                    }
                };

                var service = new SessionService(_stripeClient);
                var session = await service.CreateAsync(options);

                return new PaymentResponse
                {
                    Success = true,
                    TransactionId = session.Id,
                    RedirectUrl = session.Url
                };
            }
            catch (StripeException ex)
            {
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Stripe error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(HttpRequest request)
        {
            try
            {
                _stripeClient = await GetStripeClientAsync();
                var sessionId = request.Query["session_id"].ToString();

                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Session ID not found in callback."
                    };
                }

                var service = new SessionService(_stripeClient);
                var session = await service.GetAsync(sessionId);

                if (session.PaymentStatus == "paid")
                {
                    return new PaymentResponse
                    {
                        Success = true,
                        TransactionId = session.Id
                    };
                }

                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Payment was not successful."
                };
            }
            catch (StripeException ex)
            {
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Stripe error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Unexpected error: {ex.Message}"
                };
            }
        }
    }
}
