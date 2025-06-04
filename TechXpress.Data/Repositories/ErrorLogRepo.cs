using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;

namespace TechXpress.Data.Repositories
{
    public class ErrorLogRepo : Repository<ErrorLog>, IErrorLogRepo
    {
        private readonly AppDbContext _dp;
        public ErrorLogRepo(AppDbContext dp, ILogger<Repository<ErrorLog>> logger) : base(dp, logger)
        {
            _dp = dp;
        }

        public async Task<IEnumerable<ErrorLog>> GetRecentErrorsAsync(int count = 50)
        {
            return await _dp.ErrorLogs
                .OrderByDescending(e => e.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<ErrorLog>> GetErrorsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dp.ErrorLogs
                .Where(e => e.CreatedDate >= startDate && e.CreatedDate <= endDate)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ErrorLog>> GetErrorsBySeverityAsync(string severity)
        {
            return await _dp.ErrorLogs
                .Where(e => e.Severity == severity)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public async Task CleanupOldErrorsAsync(int daysToKeep = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldErrors = await _dp.ErrorLogs
                .Where(e => e.CreatedDate < cutoffDate)
            .ToListAsync();

            if (oldErrors.Any())
            {
                _dp.ErrorLogs.RemoveRange(oldErrors);
                await _dp.SaveChangesAsync();
            }
        }
    }
}
