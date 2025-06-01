using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.Base
{
    public interface IDynamicSettingsService
    {
        Task<string?> GetValueAsync(string key);
        Task<T?> GetSectionAsync<T>(string sectionName) where T : class, new();
    }

}
