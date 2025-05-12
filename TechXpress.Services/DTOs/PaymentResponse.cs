using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs
{
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public string RedirectUrl { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorCode { get; set; }
        public string DebugId { get; set; }
    }
}
