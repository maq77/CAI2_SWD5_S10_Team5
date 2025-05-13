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
    public class CashGateway : IPaymentGateway
    {
        public string Name => "Cash";
        public CashGateway()
        {
            
        }
        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            return new PaymentResponse
            {
                Success = true,
                TransactionId = Guid.NewGuid().ToString(),
                RedirectUrl = request.ReturnUrl
            };
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(HttpRequest request)
        {
            return new PaymentResponse { 
              Success = true,
              TransactionId = Guid.NewGuid().ToString()
            };
        }
    }
}
