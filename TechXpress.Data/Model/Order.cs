using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Data.Model
{
    public class Order
    {
        public int Id { get; set; }  // PrK
        public string UserId { get; set; }  // FK
        public User? User { get; set; }  // Navigation Property

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }  // 1-M Relation
    }
}
