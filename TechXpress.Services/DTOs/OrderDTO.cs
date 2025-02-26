using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Enums;

namespace TechXpress.Services.DTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public string UserId   { get; set; }
        public double TotalAmount { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public List<OrderDetailDTO> OrderDetails { get; set; } = new();
    }
}
