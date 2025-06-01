using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Services.Base;

namespace TechXpress.Services
{
    public class DynamicSettingsService : IDynamicSettingsService
    {
        private readonly IConfiguration _configuration;
        private readonly IAppSettingService _appSettingService;

        public DynamicSettingsService(IConfiguration configuration, IAppSettingService appSettingService)
        {
            _configuration = configuration;
            _appSettingService = appSettingService;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            // First try to get from database
            var setting = await _appSettingService.GetValueAsync(key);
            return setting ?? _configuration[key];
        }

        public async Task<T?> GetSectionAsync<T>(string sectionName) where T : class, new()
        {
            var properties = typeof(T).GetProperties();
            var obj = new T();
            bool foundInDb = false;

            foreach (var prop in properties)
            {
                string fullKey = $"{sectionName}:{prop.Name}";

                // Get the raw string value first
                var dbSetting = await _appSettingService.GetValueAsync(fullKey);
                string? value = dbSetting ?? _configuration[fullKey];

                if (value != null)
                {
                    try
                    {
                        // Handle nullable types
                        var targetType = prop.PropertyType;
                        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                        object? converted = Convert.ChangeType(value, underlyingType);
                        prop.SetValue(obj, converted);

                        if (dbSetting != null) foundInDb = true;
                    }
                    catch (Exception ex)
                    {
                        // Log the conversion error but continue
                        // You might want to inject ILogger here too
                        Console.WriteLine($"Failed to convert {fullKey}: {value} to {prop.PropertyType}. Error: {ex.Message}");
                    }
                }
            }

            return foundInDb ? obj : _configuration.GetSection(sectionName).Get<T>();
        }
    }
}
