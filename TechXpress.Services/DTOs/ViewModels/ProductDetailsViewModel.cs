﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs.ViewModels
{
    public class ProductDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;   
        public string Description { get; set; } = "No Description for this Product!";
        public double Price { get; set; }
        public List<ProductImageDTO> Images { get; set; } = new();
        public string CategoryName { get; set; } = string.Empty;
        public int StockQuantity { get; set; } = 0;
        public byte Rating { get; set; } = 5;
        public double AverageRating { get; set; } = 0;
        public List<ReviewDTO> Reviews { get; set; } = new();
        public int ReviewCount { get; set; } = 0;
        public bool IsInWishlist { get; set; } = false;
    }

}
