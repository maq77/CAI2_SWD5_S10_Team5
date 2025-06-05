using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs.ViewModels;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Area("Admin")]
    public class ErrorLogsController : Controller
    {
        private readonly IErrorLoggingService _errorLoggingService;

        public ErrorLogsController(IErrorLoggingService errorLoggingService)
        {
            _errorLoggingService = errorLoggingService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 25, string severity = "",
            DateTime? dateFrom = null, DateTime? dateTo = null, string search = "")
        {
            try
            {
                var allErrors = await _errorLoggingService.GetRecentErrorsAsync(1000); // Get more for filtering

                // filters
                var filteredErrors = allErrors.AsQueryable();

                if (!string.IsNullOrEmpty(severity))
                    filteredErrors = filteredErrors.Where(e => e.Severity == severity);

                if (dateFrom.HasValue)
                    filteredErrors = filteredErrors.Where(e => e.CreatedDate >= dateFrom.Value);

                if (dateTo.HasValue)
                    filteredErrors = filteredErrors.Where(e => e.CreatedDate <= dateTo.Value.AddDays(1));

                if (!string.IsNullOrEmpty(search))
                    filteredErrors = filteredErrors.Where(e => e.Message.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                                              (e.Source != null && e.Source.Contains(search, StringComparison.OrdinalIgnoreCase)));

                var totalCount = filteredErrors.Count();
                var errors = filteredErrors
                    .OrderByDescending(e => e.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModel = new ErrorLogIndexViewModel
                {
                    Errors = errors,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    Severity = severity,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Search = search,
                    ErrorCount = allErrors.Count(e => e.Severity == "Error"),
                    WarningCount = allErrors.Count(e => e.Severity == "Warning"),
                    InfoCount = allErrors.Count(e => e.Severity == "Info"),
                    TotalLogCount = allErrors.Count()
                };

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_ErrorLogTable", viewModel);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load error logs.";
                return View(new ErrorLogIndexViewModel());
            }
        }

        public async Task<IActionResult> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            var errors = await _errorLoggingService.GetErrorsByDateRangeAsync(startDate, endDate);
            return View("Index", errors);
        }

        /*[HttpPost]
        public async Task<IActionResult> Cleanup(int daysToKeep = 30)
        {
            await _errorLoggingService.CleanupOldErrorsAsync(daysToKeep);
            TempData["Message"] = $"Cleaned up error logs older than {daysToKeep} days.";
            return RedirectToAction("Index");
        }*/
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var error = await _errorLoggingService.GetErrorById(id);
                if (error == null) // error == null
                {
                    return NotFound();
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_ErrorDetails", error);
                }

                return View(error);
                //return Json(new { success = true, message = "Error details loaded successfully." }); // Mock response //del
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to load error details." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _errorLoggingService.DeleteErrorById(id);
                if (success) 
                {
                    return Json(new { success = true, message = "Error log deleted successfully." });
                }
                return Json(new { success = false, message = "Error log not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to delete error log." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete([FromBody] int[] ids)
        {
            try
            {
                var deletedCount = await _errorLoggingService.BulkDeleteErrors(ids);
                return Json(new { success = true, message = $"{deletedCount} error logs deleted successfully.", count = deletedCount });
                //return Json(new { success = true, message = $"{ids.Length} error logs deleted successfully.", count = ids.Length }); // Mock response //del
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to delete error logs." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cleanup(int daysToKeep = 30)
        {
            try
            {
                await _errorLoggingService.CleanupOldErrorsAsync(daysToKeep);
                TempData["Success"] = $"Successfully cleaned up error logs older than {daysToKeep} days.";
                return Json(new { success = true, message = $"Cleaned up error logs older than {daysToKeep} days." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to cleanup old error logs." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var allErrors = await _errorLoggingService.GetRecentErrorsAsync(1000);
                var stats = new
                {
                    errorCount = allErrors.Count(e => e.Severity == "Error"),
                    warningCount = allErrors.Count(e => e.Severity == "Warning"),
                    infoCount = allErrors.Count(e => e.Severity == "Info"),
                    totalCount = allErrors.Count()
                };
                return Json(stats);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to load statistics." });
            }
        }
    }
}
