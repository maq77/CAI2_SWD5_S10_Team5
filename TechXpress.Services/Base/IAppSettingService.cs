using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;

namespace TechXpress.Services.Base
{
    public interface IAppSettingService
    {
        Task<string?> GetValueAsync(string key);
        Task<T?> GetValueAsync<T>(string key);
        Task SetValueAsync(string key, string value);
        Task<IEnumerable<AppSetting>> GetAllAsync();
        Task<bool> UpdateAppSettingAsync(AppSetting setting);
    }

}
