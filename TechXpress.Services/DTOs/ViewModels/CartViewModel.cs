using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs.ViewModels
{
    public class CartViewModel
    {
        public List<OrderDetailDTO> Items { get; set; } = new List<OrderDetailDTO>();
        public int TotalItems => Items?.Sum(i => i.Quantity) ?? 0;
        public double TotalPrice => Items?.Sum(i => i.Quantity * i.Price) ?? 0;
    }
}
