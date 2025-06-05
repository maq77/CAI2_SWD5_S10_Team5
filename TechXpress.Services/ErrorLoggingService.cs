using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TechXpress.Services
{
    public class ErrorLoggingService : IErrorLoggingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ErrorLoggingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogErrorAsync(Exception exception, string? source, string? userName,
            string? ipAddress, string? requestUrl, string? additionalInfo)
        {
            try
            {
                var errorLog = new ErrorLog
                {
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    Source = source ?? exception.Source,
                    UserName = userName,
                    IpAddress = ipAddress,
                    RequestUrl = requestUrl,
                    Severity = "Error",
                    CreatedDate = DateTime.UtcNow,
                    AdditionalInfo = additionalInfo
                };

                await _unitOfWork.ErrorLogs.Add(errorLog, log => Console.WriteLine(log));
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                // Fail silently to avoid cascading errors
            }
        }

        public async Task LogWarningAsync(string message, string? source, string? userName,
            string? additionalInfo)
        {
            try
            {
                var errorLog = new ErrorLog
                {
                    Message = message,
                    Source = source,
                    UserName = userName,
                    Severity = "Warning",
                    CreatedDate = DateTime.UtcNow,
                    AdditionalInfo = additionalInfo
                };

                await _unitOfWork.ErrorLogs.Add(errorLog, log => Console.WriteLine(log));
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                // Fail silently
            }
        }

        public async Task LogInfoAsync(string message, string? source, string? userName,
            string? additionalInfo)
        {
            try
            {
                var errorLog = new ErrorLog
                {
                    Message = message,
                    Source = source,
                    UserName = userName,
                    Severity = "Info",
                    CreatedDate = DateTime.UtcNow,
                    AdditionalInfo = additionalInfo
                };

                await _unitOfWork.ErrorLogs.Add(errorLog, log => Console.WriteLine(log));
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                // Fail silently
            }
        }

        public async Task<IEnumerable<ErrorLog>> GetRecentErrorsAsync(int count = 50)
        {
           return await _unitOfWork.ErrorLogs.GetRecentErrorsAsync(count);
        }

        public async Task<IEnumerable<ErrorLog>> GetErrorsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
           return await _unitOfWork.ErrorLogs.GetErrorsByDateRangeAsync(startDate, endDate);
        }
        public async Task<ErrorLogDTO> GetErrorById(int Id)
        {
            var error = await _unitOfWork.ErrorLogs.Find_First(e => e.Id == Id);
            var errrsDTO = new ErrorLogDTO { 
                 Id = error.Id,
                 Message = error.Message,
                 Source = error.Source,
                 StackTrace = error.StackTrace,
                 Severity = error.Severity,
                 UserName = error.UserName,
                 RequestUrl = error.RequestUrl,
                 IpAddress = error.IpAddress,
                 AdditionalInfo = error.AdditionalInfo,
                 CreatedDate = error.CreatedDate
            };
            return errrsDTO;
        }
        public async Task<List<ActivityItem>> GetRecentActivityAsync(int count = 10)
        {
            var recentErrors = await _unitOfWork.ErrorLogs.GetRecentErrorsAsync(count);
            return recentErrors.Select(error => new ActivityItem
            {
                Title = GetErrorTitle(error.Source, error.Severity),
                Description = error.Message?.Length > 50 ? error.Message.Substring(0, 50) + "..." : error.Message,
                Icon = GetIconBySource(error.Source),
                GradientClass = GetGradientBySeverity(error.Severity),
                Timestamp = error.CreatedDate
            }).ToList();
        }
        #region private methods 
        private string GetIconBySource(string source)
        {
            if (string.IsNullOrEmpty(source)) return "fas fa-exclamation-triangle";

            var lowerSource = source.ToLower();

            return lowerSource switch
            {
                var s when s.Contains("order") => "fas fa-shopping-cart",
                var s when s.Contains("user") => "fas fa-user-plus",
                var s when s.Contains("product") => "fas fa-box",
                var s when s.Contains("review") => "fas fa-star",
                var s when s.Contains("database") || s.Contains("sql") => "fas fa-database",
                var s when s.Contains("database") || s.Contains("sql") => "fas fa-database",
                var s when s.Contains("api") || s.Contains("web") => "fas fa-globe",
                var s when s.Contains("auth") || s.Contains("login") => "fas fa-user-shield",
                var s when s.Contains("payment") || s.Contains("billing") => "fas fa-credit-card",
                var s when s.Contains("email") || s.Contains("mail") => "fas fa-envelope",
                var s when s.Contains("file") || s.Contains("upload") => "fas fa-file-alt",
                var s when s.Contains("network") || s.Contains("connection") => "fas fa-wifi",
                var s when s.Contains("validation") || s.Contains("input") => "fas fa-check-circle",
                var s when s.Contains("security") || s.Contains("firewall") => "fas fa-shield-alt",
                var s when s.Contains("cache") || s.Contains("redis") => "fas fa-memory",
                var s when s.Contains("controller") => "fas fa-cogs",
                var s when s.Contains("service") => "fas fa-server",
                _ => "fas fa-bug"
            };
        }
        private string GetGradientBySeverity(string severity)
        {
            if (string.IsNullOrEmpty(severity)) return "var(--gradient-1)";

            return severity.ToLower() switch
            {
                "critical" or "fatal" => "var(--gradient-4)", // Red gradient
                "error" => "var(--gradient-3)", // Orange gradient  
                "warning" => "var(--gradient-2)", // Yellow gradient
                "info" or "information" => "var(--gradient-1)", // Blue gradient
                _ => "var(--gradient-1)"
            };
        }

        private string GetErrorTitle(string source, string severity)
        {
            var severityText = !string.IsNullOrEmpty(severity) ? severity : "Error";
            var sourceText = !string.IsNullOrEmpty(source) ? source : "System";

            return $"{severityText} in {sourceText}";
        }
        #endregion
        public async Task<bool> DeleteErrorById(int Id)
        {
            try
            {
                await _unitOfWork.ErrorLogs.Delete(Id, log => Console.WriteLine(log));
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting log: {ex.Message}");
                await LogErrorAsync(ex, "ErrorLoggingService.DeleteErrorById", "Admin", null, null, null);
                return false;
            }
        }

        public async Task<bool> BulkDeleteErrors(int[] ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    await _unitOfWork.ErrorLogs.Delete(id, log => Console.WriteLine(log));
                }
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting logs: {ex.Message}");
                await LogErrorAsync(ex, "ErrorLoggingService.BulkDeleteErrors", "Admin", null, null, null);
                return false;
            }
        }
        public async Task CleanupOldErrorsAsync(int daysToKeep = 30)
        {
           await _unitOfWork.ErrorLogs.CleanupOldErrorsAsync(daysToKeep);
        }
    }
}
