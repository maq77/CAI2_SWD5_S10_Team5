using Microsoft.AspNetCore.Mvc;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    public class CheckoutController : Controller
    {
        [Area("Customer")]
        public IActionResult Index()
        {
            return View();
        }
        #region Private Helper Methods

        //
        protected void SetPageMeta()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            ViewData["PageTitle"] = FormatTitle(action);
            ViewData["BreadcrumbPath"] = new List<(string, string)>
        {
            ("/", "Home"),
            ($"/{controller}", FormatTitle(controller))
        };
        }

        private string FormatTitle(string text) =>
            System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.Replace("_", " ").ToLower());
        #endregion
    }
}
