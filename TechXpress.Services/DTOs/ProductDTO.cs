using Microsoft.AspNetCore.Http;
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
        public int Id { get; set; }
        //[Required]
        public string Name { get; set; } = string.Empty;
        //[Required]
        public string? Description { get; set; } = string.Empty;
        [Required]
        public double Price { get; set; } = double.MaxValue;
        //[Required]
        public int CategoryId { get; set; } = int.MaxValue;
        public string? CategoryName { get; set; } = "UnCategorized";
        public int SalesCount { get; set; } = 0;
        public List<ProductImageDTO> Images { get; set; } = new();
        public List<IFormFile>? UploadedImages { get; set; }
    }
}
