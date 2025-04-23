using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Data.Model
{
    public class OrderDetail
    {
        public int Id { get; set; }  // PK
        public int OrderId { get; set; }  // FK
        public Order Order { get; set; }  // Navigation Property

        public int ProductId { get; set; }  // FK
        public Product Product { get; set; }  // Navigation Property

        public int Quantity { get; set; }
        public double Price { get; set; }  ///UnitPrice
    }
}
