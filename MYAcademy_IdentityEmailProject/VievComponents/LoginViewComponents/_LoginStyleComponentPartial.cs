using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.VievComponents.LoginViewComponents
{
    public class _LoginStyleComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();  
        }
    }
}
