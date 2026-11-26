using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    [IgnoreAntiforgeryToken]
    public class ErrorPagesController : Controller
    {
        public IActionResult Page401(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public IActionResult Page403()
        {
            return View();
        }

        public IActionResult Page404()
        {
            return View();
        }
    }
}
