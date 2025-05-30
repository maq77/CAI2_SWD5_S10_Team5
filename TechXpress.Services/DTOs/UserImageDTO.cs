using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs
{
    public class UserImageDTO
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } = "/images/default-user.png";
        public string? UserId { get; set; }
    }
}
