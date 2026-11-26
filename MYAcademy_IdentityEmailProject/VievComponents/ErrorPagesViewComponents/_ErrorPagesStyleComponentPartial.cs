using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.VievComponents.ErrorPagesViewComponents
{
    public class _ErrorPagesStyleComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
