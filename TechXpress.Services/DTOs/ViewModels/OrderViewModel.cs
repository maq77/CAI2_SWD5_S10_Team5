using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Enums;

namespace TechXpress.Services.DTOs.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public double TotalAmount { get; set; }
        public OrderStatus SelectedStatus { get; set; } = OrderStatus.Pending; 
        public List<SelectListItem>? Status { get; set; } = new();
        public string CustomerName { get; set; } = string.Empty; 
        public string CustomerEmail { get; set; } = string.Empty;
        public string shipping_address { get; set; } = string.Empty;
        public string paymentMethod { get; set; } = string.Empty;
        public string transactionId { get; set; } = string.Empty;
        public List<OrderDetailViewModel> Items { get; set; } = new List<OrderDetailViewModel>();

        // Constructor to populate the Status SelectList
        public OrderViewModel()
        {
            Status = Enum.GetValues(typeof(OrderStatus))
                         .Cast<OrderStatus>()
                         .Select(status => new SelectListItem
                         {
                             Value = status.ToString(),
                             Text = status.ToString()
                         })
                         .ToList();
        }
    }
}
