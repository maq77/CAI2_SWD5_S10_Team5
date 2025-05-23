using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Services.DTOs;

namespace TechXpress.Services.Base
{
    public interface IAnalyticsService
    {
        Task<List<SalesChartDTO>> GetSalesChartDataAsync(DateTime? from, DateTime? to);
        Task<List<CountChartDTO>> GetProductChartDataAsync();
        Task<List<CountChartDTO>> GetUserChartDataAsync();
        Task<List<CategoryChartDTO>> GetCategoryChartDataAsync();
        Task<List<CountChartDTO>> GetOrderChartDataAsync();
        byte[] ExportSalesDataAsCsv(List<SalesChartDTO> salesData);
        byte[] ExportSalesDataAsPdf(List<SalesChartDTO> salesData);

    }
}
