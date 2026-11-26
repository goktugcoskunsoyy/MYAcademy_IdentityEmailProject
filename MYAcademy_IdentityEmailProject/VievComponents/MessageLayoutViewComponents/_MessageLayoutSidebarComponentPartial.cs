using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;

namespace MYAcademy_IdentityEmailProject.VievComponents.MessageLayoutViewComponents
{
    public class _MessageLayoutSidebarComponentPartial : ViewComponent
    {
        private readonly IdentityContext _context;
        private readonly UserManager<AppUser> _userManager;

        public _MessageLayoutSidebarComponentPartial(IdentityContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.UnreadCount = _context.Messages.Count(x => x.ReceiverEmail == user.Email && !x.IsRead && !x.IsDraft && !x.IsTrashByReceiver && !x.IsDeletedByReceiver);
            ViewBag.DraftCount = _context.Messages.Count(x => x.SenderEmail == user.Email && x.IsDraft && !x.IsTrashBySender && !x.IsDeletedBySender);
            ViewBag.FavoriteCount = _context.Messages.Count(x => x.ReceiverEmail == user.Email && x.IsFavorite && !x.IsDraft && !x.IsTrashByReceiver && !x.IsDeletedByReceiver);
            ViewBag.TrashCount = _context.Messages.Count(x => (x.ReceiverEmail == user.Email && x.IsTrashByReceiver && !x.IsDeletedByReceiver && !x.IsDraft) || (x.SenderEmail == user.Email && x.IsTrashBySender && !x.IsDeletedBySender));
            ViewBag.SendboxCount = _context.Messages.Count(x => x.SenderEmail == user.Email && !x.IsDraft && !x.IsTrashBySender && !x.IsDeletedBySender);
            ViewBag.InboxCount = _context.Messages.Count(x => x.ReceiverEmail == user.Email && !x.IsDraft && !x.IsTrashByReceiver && !x.IsDeletedByReceiver);

            var values = _context.Categories.ToList();
            return View(values);
        }
    }
}
