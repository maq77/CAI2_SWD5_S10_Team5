using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs
{
    public class OrderDetailDTO
    {
        public ProductDTO Product { get; set; } = new ProductDTO();
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; } //UnitPrice
    }
}
