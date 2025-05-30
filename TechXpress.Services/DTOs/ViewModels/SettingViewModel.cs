using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;

namespace TechXpress.Services.DTOs.ViewModels
{
    public class SettingViewModel
    {
        IEnumerable<AppSetting> appSettings {  get; set; } = new List<AppSetting>();
    }

}
