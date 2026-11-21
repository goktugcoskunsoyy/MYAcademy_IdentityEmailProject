using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace MYAcademy_IdentityEmailProject.VievComponents.LoginViewComponents
{
    public class _LoginScriptComponentPartial : ViewComponent
    {
       public IViewComponentResult Invoke() 
        {  
            return View(); 
        }
    }
}
