using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data;
using TechXpress.Services.Base;
using TechXpress.Data.Repositories.Base;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TechXpress.Services
{
    public class AppSettingService : IAppSettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AppSettingService> _logger;

        public AppSettingService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AppSettingService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<IEnumerable<AppSetting>> GetAllAsync()
        {
            return await _unitOfWork.AppSettings.GetAll();
        }
        public async Task<T?> GetValueAsync<T>(string key)
        {
            var val = await GetValueAsync(key);
            return val is not null ? JsonConvert.DeserializeObject<T>(val) : default;
        }
        public async Task<string?> GetValueAsync(string key)
        {
            var setting = await _unitOfWork.AppSettings
                .Find_First(x => x.Key == key);
            var val = setting?.Value;
            if (string.IsNullOrEmpty(val))
            {
                return _configuration[key]; // appsettings.json or environment
            }
            _logger.LogWarning($" GET Key = {key}, Val = {val}, Conf = {_configuration[key]} ");
            return val;
        }
        public async Task<bool> UpdateAppSettingAsync(AppSetting setting)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            if (string.IsNullOrEmpty(setting.Value))
                setting.Value = "";  // default

            await _unitOfWork.AppSettings.Update(setting, log => Console.WriteLine(log));
            var result = await _unitOfWork.SaveAsync();
            return result;
        }
        public async Task SetValueAsync(string key, string value)
        {
            var setting = await _unitOfWork.AppSettings.Find_First(x => x.Key == key);
            if (setting != null)
            {
                setting.Value = value;
            }
            else
            {
                setting = new AppSetting { Key = key, Value = value };
                await  _unitOfWork.AppSettings.Add(setting, log => Console.WriteLine(log));
            }
            _logger.LogWarning($" POST Key = {key}, Val = {setting.Value}, Conf = {_configuration[key]} ");
            await _unitOfWork.SaveAsync();
        }
    }

}
