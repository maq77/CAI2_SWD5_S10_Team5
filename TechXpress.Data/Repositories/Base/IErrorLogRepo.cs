using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;

namespace TechXpress.Data.Repositories.Base
{
    public interface IErrorLogRepo : IRepository<ErrorLog>
    {
        Task<IEnumerable<ErrorLog>> GetRecentErrorsAsync(int count = 50);
        Task<IEnumerable<ErrorLog>> GetErrorsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<ErrorLog>> GetErrorsBySeverityAsync(string severity);
        Task CleanupOldErrorsAsync(int daysToKeep = 30);
    }
}
