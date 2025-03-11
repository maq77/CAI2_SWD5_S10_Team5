using Microsoft.AspNetCore.Mvc;

namespace TechXpress.Web.Controllers.Admin
{
    public class UserManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
