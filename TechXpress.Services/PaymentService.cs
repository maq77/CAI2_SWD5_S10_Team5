using Microsoft.AspNetCore.Http;
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
        private readonly IEnumerable<IPaymentGateway> _gateways;

        public PaymentService(IEnumerable<IPaymentGateway> gateways)
        {
            _gateways = gateways;
        }

        public IEnumerable<string> GetAvailableGateways()
        {
            return _gateways.Select(g => g.Name);
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(string gatewayName, PaymentRequest request)
        {
            var gateway = _gateways.FirstOrDefault(g => g.Name.Equals(gatewayName, StringComparison.OrdinalIgnoreCase));

            if (gateway == null)
            {
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Payment gateway '{gatewayName}' not found"
                };
            }

            return await gateway.ProcessPaymentAsync(request);
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(string gatewayName,HttpRequest request)
        {
            var gateway = _gateways.FirstOrDefault(g => g.Name.Equals(gatewayName, StringComparison.OrdinalIgnoreCase));

            if (gateway == null)
            {
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Payment gateway '{gatewayName}' not found"
                };
            }
            return await gateway.VerifyPaymentAsync(request);
        }
    }
}
