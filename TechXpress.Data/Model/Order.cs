using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Enums;

namespace TechXpress.Data.Model
{
    public class Order
    {
        public int Id { get; set; }  // PrK
        public string UserId { get; set; }  // FK
        public User? User { get; set; }  // Navigation Property

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public double TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();  // 1-M Relation
    }
}
