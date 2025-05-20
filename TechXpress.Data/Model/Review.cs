using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Data.Model
{
    public class Review
    {
        [Key]
        public int Id { get; set; } // PK
        public string Comment { get; set; } = string.Empty;
        public byte Rating { get; set; } // 1 to 5
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Creation timestamp
        public int ProductId { get; set; } // FK
        public string UserId { get; set; } // FK
        public Product? Product { get; set; } // Navigation Property
        public User? User { get; set; } // Navigation Property
        public bool IsDeleted { get; set; } = false; // Soft delete
        public DateTime? DeletedAt { get; set; } // Soft delete timestamp
        public string? DeletedReason { get; set; } = string.Empty; // Soft delete reason
        public bool IsActive { get; set; } = true; // Soft delete

    }
}
