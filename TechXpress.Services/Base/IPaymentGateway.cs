using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface IPaymentGateway
    {
        string Name { get; }
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentResponse> VerifyPaymentAsync(HttpRequest request);
    }
}
