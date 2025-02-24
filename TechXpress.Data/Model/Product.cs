using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Data.Model
{
    public class Product
    {
        public int Id { get; set; }  //PK
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public int CategoryId { get; set; } //FK
        public Category? Category { get; set; }  //Navigation Property
    }
}
