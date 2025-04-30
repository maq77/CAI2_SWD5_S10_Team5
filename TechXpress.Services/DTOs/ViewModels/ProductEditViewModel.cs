using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs.ViewModels
{
    public class ProductEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public double Price { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 0 and 100")]
        public int StockQuantity { get; set; }

        public int? CategoryId { get; set; }  // Optional category update
        public List<SelectListItem>? Categories { get; set; } // Dropdown list

        public List<IFormFile>? NewImages { get; set; }  // Optional new image uploads
        public List<ProductImageDTO>? ExistingImages { get; set; } // Existing image paths
    }
}
