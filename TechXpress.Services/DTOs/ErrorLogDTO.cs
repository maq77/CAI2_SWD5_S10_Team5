using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs
{
    public class ErrorLogDTO
    {
        public int Id { get; set; }

        public string Message { get; set; } = string.Empty;

        public string StackTrace { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;

        public string RequestUrl { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;    // Error, Warning, Info

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string AdditionalInfo { get; set; } = string.Empty;
    }
}
