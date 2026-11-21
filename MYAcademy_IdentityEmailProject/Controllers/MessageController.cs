using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Context;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly IdentityContext _context;

        public MessageController(IdentityContext context)
        {
            _context = context;
        }

        public IActionResult Inbox()
        {
            var values = _context.Messages.Where(x => x.RecieverEmail == "aaa").ToList();
            return View();
        }
    }
}
