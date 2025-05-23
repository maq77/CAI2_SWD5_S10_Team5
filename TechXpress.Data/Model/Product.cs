using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public int StockQuantity { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string Description { get; set; } = string.Empty;
        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; } // Navigation Property
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
