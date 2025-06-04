using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Data.Model
{
    public class ErrorLog
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public string? StackTrace { get; set; }

        public string? Source { get; set; }

        public string? UserName { get; set; }

        public string? IpAddress { get; set; }

        public string? RequestUrl { get; set; }

        public string? Severity { get; set; } // Error, Warning, Info

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? AdditionalInfo { get; set; }
    }
}
