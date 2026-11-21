using Microsoft.AspNetCore.Mvc;

namespace MYAcademy_IdentityEmailProject.VievComponents.LoginViewComponents
{
    public class _LoginBrandComponentPartial : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();  
        }
    }
}
