using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using TechXpress.Services.Base;

namespace TechXpress.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async  Task<IActionResult> GetSalesChartData(DateTime? from, DateTime? to)
        {
            var data = await _analyticsService.GetSalesChartDataAsync(from, to);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryChartData()
        {
            var data = await _analyticsService.GetCategoryChartDataAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserChartData()
        {
            var data = await _analyticsService.GetUserChartDataAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductChartData()
        {
            var data = await _analyticsService.GetCategoryChartDataAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportSalesData(string type, DateTime? from, DateTime? to)
        {
            var salesData = await _analyticsService.GetSalesChartDataAsync(from, to);

            byte[] content;
            string mime;
            string fileName = $"sales_report_{DateTime.Now:yyyyMMdd_HHmm}.{type}";

            switch (type.ToLower())
            {
                case "csv":
                    content = _analyticsService.ExportSalesDataAsCsv(salesData);
                    mime = "text/csv";
                    break;
                case "pdf":
                    content = _analyticsService.ExportSalesDataAsPdf(salesData);
                    mime = "application/pdf";
                    break;
                default:
                    return BadRequest("Invalid export type. Supported: csv, pdf.");
            }

            return File(content, mime, fileName);
        }
    }
}
