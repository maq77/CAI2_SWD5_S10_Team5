using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs
{
    public class SalesChartDTO
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public double Sales { get; set; }
        public double Revenue { get; set; }
    }
}
