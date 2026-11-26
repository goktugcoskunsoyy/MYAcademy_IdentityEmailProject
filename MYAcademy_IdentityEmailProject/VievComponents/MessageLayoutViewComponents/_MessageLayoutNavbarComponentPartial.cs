using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;

namespace MYAcademy_IdentityEmailProject.VievComponents.MessageLayoutViewComponents
{
    public class _MessageLayoutNavbarComponentPartial : ViewComponent
    {
        private readonly IdentityContext _context;
        private readonly UserManager<AppUser> _userManager;

        public _MessageLayoutNavbarComponentPartial(IdentityContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var notifications = _context.Messages
                .Where(x => x.ReceiverEmail == user.Email && !x.IsRead && !x.IsDraft && !x.IsTrashByReceiver && !x.IsDeletedByReceiver)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            ViewBag.NotificationCount = notifications.Count;
            ViewBag.Notifications = notifications.Take(5).ToList();
            return View(user);
        }
    }
}
