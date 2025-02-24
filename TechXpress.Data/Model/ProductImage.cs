using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Data.Model
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string ImagePath { get; set; } = string.Empty;
    }
}
