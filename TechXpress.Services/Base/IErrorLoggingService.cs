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
        Task LogErrorAsync(Exception exception, string? source, string? userName,
            string? ipAddress, string? requestUrl, string? additionalInfo);
        Task LogWarningAsync(string message, string? source, string? userName,
            string? additionalInfo);
        Task LogInfoAsync(string message, string? source, string? userName,
            string? additionalInfo);
        Task<IEnumerable<ErrorLog>> GetRecentErrorsAsync(int count = 50);
        Task<IEnumerable<ErrorLog>> GetErrorsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task CleanupOldErrorsAsync(int daysToKeep = 30);
    }
}
