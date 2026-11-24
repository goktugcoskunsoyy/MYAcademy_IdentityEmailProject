using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.VievComponents.MessageLayoutViewComponents
{
    public class _MessageLayoutScriptComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
