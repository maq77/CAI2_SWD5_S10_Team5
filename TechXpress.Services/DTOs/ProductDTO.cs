using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs
{
    public class ProductDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public double Price { get; set; } = double.MaxValue;
        [Required]
        public int CategoryId { get; set; } = int.MaxValue;
    }
}
