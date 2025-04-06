using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs
{
    public class WishListItemDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; }
    }
}
