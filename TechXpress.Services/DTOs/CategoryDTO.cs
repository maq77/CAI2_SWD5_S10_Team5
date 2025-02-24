using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs
{
    public class CategoryDTO
    {
        //[Required]
        public int Id { get; set; } = int.MaxValue;
        //[Required]
        public string Name { get; set; } = string.Empty;
    }
}
