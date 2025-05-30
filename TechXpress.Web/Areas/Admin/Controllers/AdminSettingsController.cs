using Microsoft.AspNetCore.Mvc;
using TechXpress.Data.Model;
using TechXpress.Services.Base;

[Area("Admin")]
public class AdminSettingsController : Controller
{
    private readonly IAppSettingService _appSettingService;

    public AdminSettingsController(IAppSettingService appSettingService)
    {
        _appSettingService = appSettingService;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _appSettingService.GetAllAsync();
        return View(settings);
    }

    [HttpPost]
    public async Task<IActionResult> Update(List<AppSetting> settings)
    {
        foreach (var setting in settings)
        {
           var res = await _appSettingService.UpdateAppSettingAsync(setting);
        }

        TempData["Success"] = "Settings updated successfully.";
        return RedirectToAction("Index");
    }

}
