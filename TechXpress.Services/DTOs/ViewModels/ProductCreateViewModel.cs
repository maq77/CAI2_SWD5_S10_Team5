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
    public class ProductCreateViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public double Price { get; set; }
        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 0 and 100")]
        public int StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public List<SelectListItem>? Categories { get; set; } //  Dropdown for category selection

        public List<IFormFile>? Images { get; set; } //  Allow multiple image uploads
    }
}
