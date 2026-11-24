using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.VievComponents.MessageLayoutViewComponents
{
    public class _MessageLayoutStyleComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
