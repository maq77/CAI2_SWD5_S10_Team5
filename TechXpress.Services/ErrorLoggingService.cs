using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services.Base;

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

        public async Task CleanupOldErrorsAsync(int daysToKeep = 30)
        {
           await _unitOfWork.ErrorLogs.CleanupOldErrorsAsync(daysToKeep);
        }
    }
}
