using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using static System.Net.Mime.MediaTypeNames;

namespace TechXpress.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IUserService _userService;

        public AnalyticsService(IOrderService orderService, IProductService productService, IUserService userService)
        {
            _orderService = orderService;
            _productService = productService;
            _userService = userService;
        }

        public async Task<List<SalesChartDTO>> GetSalesChartDataAsync(DateTime? from, DateTime? to)
        {
            var orders = await _orderService.GetAllOrders();

            if (from.HasValue)
                orders = orders.Where(o => o.OrderDate >= from.Value).ToList();

            if (to.HasValue)
                orders = orders.Where(o => o.OrderDate <= to.Value).ToList();

            var result = orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new SalesChartDTO
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Sales = g.Sum(o => o.TotalAmount),
                    Revenue = g.Sum(o => o.TotalAmount * 0.8) // adjust multiplier as needed
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToList();

            return result;
        }


        public async Task<List<CountChartDTO>> GetProductChartDataAsync()
        {
            var products = await _productService.GetAllProducts();

            return Enumerable.Range(1, 12).Select(month => new CountChartDTO
            {
                Month = month,
                Count = products.Count(p => p.CreatedAt.Month == month)
            }).ToList();
        }

        public async Task<List<CountChartDTO>> GetUserChartDataAsync()
        {
            var users = await _userService.GetAllUsersAsync();

            return Enumerable.Range(1, 12).Select(month => new CountChartDTO
            {
                Month = month,
                Count = users.Count(u => u.CreatedAt.Month == month)
            }).ToList();
        }

        public async Task<List<CategoryChartDTO>> GetCategoryChartDataAsync()
        {
            var products = await _productService.GetAllProducts();
            return products
                .GroupBy(p => p.CategoryName)
                .Select(g => new CategoryChartDTO
                {
                    Category = g.Key,
                    Count = g.Count()
                }).ToList();
        }

        public async Task<List<CountChartDTO>> GetOrderChartDataAsync()
        {
            var orders = await _orderService.GetAllOrders();
            return Enumerable.Range(1, 12).Select(month => new CountChartDTO
            {
                Month = month,
                Count = orders.Count(o => o.OrderDate.Month == month)
            }).ToList();
        }

        public byte[] ExportSalesDataAsCsv(List<SalesChartDTO> salesData)
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Month,Sales,Revenue");
            foreach (var data in salesData)
            {
                csvBuilder.AppendLine($"{data.Month},{data.Sales},{data.Revenue}");
            }
            return Encoding.UTF8.GetBytes(csvBuilder.ToString());
        }

        public byte[] ExportSalesDataAsPdf(List<SalesChartDTO> salesData)
        {
            using var stream = new MemoryStream();
            var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 12);

            int y = 40;
            gfx.DrawString("Sales Report", new XFont("Verdana", 16, XFontStyle.Bold), XBrushes.Black, new XRect(0, 10, page.Width, 30), XStringFormats.TopCenter);
            gfx.DrawString("Month       Sales       Revenue", font, XBrushes.Black, new XPoint(40, y));
            y += 25;

            foreach (var item in salesData)
            {
                gfx.DrawString($"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month),-10} {item.Sales,10:C} {item.Revenue,12:C}", font, XBrushes.Black, new XPoint(40, y));
                y += 20;
                if (y > page.Height - 40)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            doc.Save(stream, false);
            return stream.ToArray();
        }
    }

}
