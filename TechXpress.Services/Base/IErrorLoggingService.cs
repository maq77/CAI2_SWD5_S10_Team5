using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;

namespace TechXpress.Services.Base
{
    public interface IErrorLoggingService
    {
        Task LogErrorAsync(Exception exception, string? source = null, string? userName = null,
            string? ipAddress = null, string? requestUrl = null, string? additionalInfo = null);
        Task LogWarningAsync(string message, string? source = null, string? userName = null,
            string? additionalInfo = null);
        Task LogInfoAsync(string message, string? source = null, string? userName = null,
            string? additionalInfo = null);
        Task<IEnumerable<ErrorLog>> GetRecentErrorsAsync(int count = 50);
        Task<IEnumerable<ErrorLog>> GetErrorsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task CleanupOldErrorsAsync(int daysToKeep = 30);
    }
}
