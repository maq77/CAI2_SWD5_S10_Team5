using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Enums;

namespace TechXpress.Services.DTOs.ViewModels
{
    public class OrderStatusUpdateViewModel
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = "Pending";
    }

}
