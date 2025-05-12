using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPaymentAsync(string gatewayName, PaymentRequest request);
        Task<PaymentResponse> VerifyPaymentAsync(string gatewayName,HttpRequest request);
        IEnumerable<string> GetAvailableGateways();
    }
}
