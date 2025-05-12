using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs.ViewModels
{
    public class CheckoutViewModel
    {
        public List<OrderDetailDTO> CartItems { get; set; }
        public double TotalAmount => CartItems?.Sum(i => i.Price * i.Quantity) ?? 0;
        public IEnumerable<string> AvailablePaymentGateways { get; set; }
        public string StripePublishableKey { get; set; }
        public string PayPalClientId { get; set; }
        public bool PayPalSandbox { get; set; }
    }
}
