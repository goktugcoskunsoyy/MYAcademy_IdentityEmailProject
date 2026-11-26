using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using System.Globalization;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IdentityContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(IdentityContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var inboxMessages = _context.Messages.Where(x => x.ReceiverEmail == user.Email && !x.IsDraft && !x.IsTrashByReceiver && !x.IsDeletedByReceiver);
            var sendboxMessages = _context.Messages.Where(x => x.SenderEmail == user.Email && !x.IsDraft && !x.IsTrashBySender && !x.IsDeletedBySender);

            ViewBag.Name = user.Name;
            ViewBag.InboxCount = inboxMessages.Count();
            ViewBag.SendboxCount = sendboxMessages.Count();
            ViewBag.UnreadCount = inboxMessages.Count(x => !x.IsRead);
            ViewBag.FavoriteCount = inboxMessages.Count(x => x.IsFavorite);
            ViewBag.FavoriteUnreadCount = inboxMessages.Count(x => x.IsFavorite && !x.IsRead);
            ViewBag.DraftCount = _context.Messages.Count(x => x.SenderEmail == user.Email && x.IsDraft && !x.IsTrashBySender && !x.IsDeletedBySender);
            ViewBag.TrashCount = _context.Messages.Count(x => (x.ReceiverEmail == user.Email && x.IsTrashByReceiver && !x.IsDeletedByReceiver && !x.IsDraft) || (x.SenderEmail == user.Email && x.IsTrashBySender && !x.IsDeletedBySender));
            ViewBag.TodayCount = inboxMessages.Count(x => x.SendDate.Date == DateTime.Today);
            ViewBag.WeekSendboxCount = sendboxMessages.Count(x => x.SendDate >= DateTime.Today.AddDays(-6));

            var categoryValues = _context.Messages
                .Where(x => !x.IsDraft && ((x.ReceiverEmail == user.Email && !x.IsTrashByReceiver && !x.IsDeletedByReceiver) || (x.SenderEmail == user.Email && !x.IsTrashBySender && !x.IsDeletedBySender)))
                .GroupBy(x => x.CategoryId)
                .Select(x => new { CategoryId = x.Key, Count = x.Count() })
                .ToList();

            var categories = _context.Categories.ToList();
            ViewBag.CategoryIds = categoryValues.OrderByDescending(x => x.Count).Select(x => x.CategoryId).ToList();
            ViewBag.CategoryNames = categoryValues.OrderByDescending(x => x.Count).Select(x => categories.FirstOrDefault(y => y.CategoryId == x.CategoryId)?.CategoryName ?? "Kategorisiz").ToList();
            ViewBag.CategoryCounts = categoryValues.OrderByDescending(x => x.Count).Select(x => x.Count).ToList();

            var startDate = DateTime.Today.AddDays(-29);
            var inboxDaily = inboxMessages.Where(x => x.SendDate >= startDate).Select(x => x.SendDate).ToList();
            var sendboxDaily = sendboxMessages.Where(x => x.SendDate >= startDate).Select(x => x.SendDate).ToList();

            var culture = new CultureInfo("tr-TR");
            List<string> dayLabels = new List<string>();
            List<int> inboxCounts = new List<int>();
            List<int> sendboxCounts = new List<int>();
            for (int i = 0; i < 30; i++)
            {
                var day = startDate.AddDays(i);
                dayLabels.Add(day.ToString("d MMM", culture));
                inboxCounts.Add(inboxDaily.Count(x => x.Date == day));
                sendboxCounts.Add(sendboxDaily.Count(x => x.Date == day));
            }
            ViewBag.DayLabels = dayLabels;
            ViewBag.InboxDailyCounts = inboxCounts;
            ViewBag.SendboxDailyCounts = sendboxCounts;

            ViewBag.LastSendboxMessages = sendboxMessages.Include(x => x.Category).OrderByDescending(x => x.SendDate).Take(5).ToList();
            ViewBag.Users = _userManager.Users.ToList();

            var values = inboxMessages.Include(x => x.Category).OrderByDescending(x => x.SendDate).Take(5).ToList();
            return View(values);
        }
    }
}
