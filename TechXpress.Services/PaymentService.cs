using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly Dictionary<string, IPaymentGateway> _gateways;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IEnumerable<IPaymentGateway> paymentGateways,
            ILogger<PaymentService> logger)
        {
            _gateways = new Dictionary<string, IPaymentGateway>();
            foreach (var gateway in paymentGateways)
            {
                _gateways[gateway.Name.ToLower()] = gateway;
            }
            _logger = logger;
        }

        public IEnumerable<string> GetAvailableGateways()
        {
            return _gateways.Keys;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(string gatewayName, PaymentRequest request)
        {
            if (string.IsNullOrEmpty(gatewayName))
            {
                _logger.LogWarning("Payment gateway name not provided");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Payment method not specified"
                };
            }

            gatewayName = gatewayName.ToLower();
            if (!_gateways.ContainsKey(gatewayName))
            {
                _logger.LogWarning($"Unknown payment gateway requested: {gatewayName}");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Unsupported payment method"
                };
            }
            try
            {
                return await _gateways[gatewayName].ProcessPaymentAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing payment with {gatewayName}");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while processing your payment"
                };
            }
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(string gatewayName, HttpRequest request)
        {
            if (string.IsNullOrEmpty(gatewayName))
            {
                _logger.LogWarning("Payment gateway name not provided for verification");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Payment method not specified"
                };
            }

            gatewayName = gatewayName.ToLower();
            if (!_gateways.ContainsKey(gatewayName))
            {
                _logger.LogWarning($"Unknown payment gateway for verification: {gatewayName}");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Unsupported payment method"
                };
            }

            try
            {
                return await _gateways[gatewayName].VerifyPaymentAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying payment with {gatewayName}");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while verifying your payment"
                };
            }
        }
    }
}
