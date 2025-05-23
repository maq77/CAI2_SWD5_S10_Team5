using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TechXpress.Data.Model
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; }  = string.Empty ;
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? PostalCode { get; set; } = string.Empty;
        public ICollection<Order>? Orders { get; set; } //1-M Realation

        public UserImage UserImage { get; set; } = new(); // 1-1 Relation
    }
}
