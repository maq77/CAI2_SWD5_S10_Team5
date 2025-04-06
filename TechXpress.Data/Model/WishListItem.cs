using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Data.Model
{
    public class WishListItem
    {
        public int Id { get; set; }  // Primary Key
        public int ProductId { get; set; }  // Foreign Key
        public string UserId { get; set; }  // User who added to Wishlist
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public virtual Product Product { get; set; }
    }
}
