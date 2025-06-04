using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;

namespace TechXpress.Services.DTOs.ViewModels
{
    public class ErrorLogIndexViewModel
    {
        public IEnumerable<ErrorLog> Errors { get; set; } = new List<ErrorLog>();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public string Severity { get; set; } = "";
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Search { get; set; } = "";

        // Stats
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }
        public int TotalLogCount { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartRecord => (CurrentPage - 1) * PageSize + 1;
        public int EndRecord => Math.Min(CurrentPage * PageSize, TotalCount);
    }
}
