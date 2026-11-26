using MailKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;
using System.Net;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly IdentityContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessageController(IdentityContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Inbox(string? search, int? categoryId, string? filter, string? sort)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var values = _context.Messages.Include(x => x.Category)
                .Where(x => x.ReceiverEmail == user.Email && !x.IsDraft && !x.IsTrashByReceiver && !x.IsDeletedByReceiver);

            ViewBag.AllCount = values.Count();
            ViewBag.UnreadCount = values.Count(x => !x.IsRead);
            ViewBag.FavoriteCount = values.Count(x => x.IsFavorite);

            if (!string.IsNullOrWhiteSpace(search))
            {
                values = values.Where(x => x.Subject.Contains(search) || x.MessageDetail.Contains(search) || x.SenderEmail.Contains(search));
            }

            if (categoryId != null)
            {
                values = values.Where(x => x.CategoryId == categoryId);
            }

            if (filter == "unread")
            {
                values = values.Where(x => !x.IsRead);
            }
            else if (filter == "favorite")
            {
                values = values.Where(x => x.IsFavorite);
            }

            values = sort == "oldest" ? values.OrderBy(x => x.SendDate) : values.OrderByDescending(x => x.SendDate);

            GetFilterValues(search, categoryId, filter, sort);
            ViewBag.Email = user.Email;
            ViewBag.Users = _userManager.Users.ToList();
            return View(values.ToList());
        }

        public async Task<IActionResult> Sendbox(string? search, int? categoryId, string? sort)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var values = _context.Messages.Include(x => x.Category)
                .Where(x => x.SenderEmail == user.Email && !x.IsDraft && !x.IsTrashBySender && !x.IsDeletedBySender);

            ViewBag.AllCount = values.Count();

            if (!string.IsNullOrWhiteSpace(search))
            {
                values = values.Where(x => x.Subject.Contains(search) || x.MessageDetail.Contains(search) || x.ReceiverEmail.Contains(search));
            }

            if (categoryId != null)
            {
                values = values.Where(x => x.CategoryId == categoryId);
            }

            values = sort == "oldest" ? values.OrderBy(x => x.SendDate) : values.OrderByDescending(x => x.SendDate);

            GetFilterValues(search, categoryId, null, sort);
            ViewBag.Email = user.Email;
            ViewBag.Users = _userManager.Users.ToList();
            return View(values.ToList());
        }

        public async Task<IActionResult> Favorites()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var values = _context.Messages.Include(x => x.Category)
                .Where(x => x.ReceiverEmail == user.Email && x.IsFavorite && !x.IsDraft && !x.IsTrashByReceiver && !x.IsDeletedByReceiver)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            ViewBag.Users = _userManager.Users.ToList();
            return View(values);
        }

        public async Task<IActionResult> Drafts()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var values = _context.Messages.Include(x => x.Category)
                .Where(x => x.SenderEmail == user.Email && x.IsDraft && !x.IsTrashBySender && !x.IsDeletedBySender)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            return View(values);
        }

        public async Task<IActionResult> Trash()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var values = _context.Messages.Include(x => x.Category)
                .Where(x => (x.ReceiverEmail == user.Email && x.IsTrashByReceiver && !x.IsDeletedByReceiver && !x.IsDraft) || (x.SenderEmail == user.Email && x.IsTrashBySender && !x.IsDeletedBySender))
                .OrderByDescending(x => x.SendDate)
                .ToList();

            ViewBag.Email = user.Email;
            ViewBag.Users = _userManager.Users.ToList();
            return View(values);
        }

        public async Task<IActionResult> MessageDetail(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var value = _context.Messages.Include(x => x.Category).FirstOrDefault(x => x.MessageId == id);
            if (value == null)
            {
                return NotFound();
            }

            bool isSender = value.SenderEmail == user.Email;
            bool isReceiver = value.ReceiverEmail == user.Email && !value.IsDraft;
            if (!isSender && !isReceiver)
            {
                return Forbid();
            }

            if ((!isSender || value.IsDeletedBySender) && (!isReceiver || value.IsDeletedByReceiver))
            {
                return NotFound();
            }

            if (value.IsDraft && !value.IsTrashBySender)
            {
                return RedirectToAction("EditDraft", new { id = id });
            }

            if (isReceiver && !value.IsRead)
            {
                value.IsRead = true;
                _context.SaveChanges();
            }

            var sender = await _userManager.FindByEmailAsync(value.SenderEmail);
            var receiver = await _userManager.FindByEmailAsync(value.ReceiverEmail);

            ViewBag.IsSender = isSender;
            ViewBag.IsReceiver = isReceiver;
            ViewBag.IsTrash = (isReceiver && value.IsTrashByReceiver) || (isSender && value.IsTrashBySender);
            ViewBag.SenderNameSurname = sender != null ? sender.Name + " " + sender.Surname : value.SenderEmail;
            ViewBag.SenderImageUrl = sender?.ImageUrl;
            ViewBag.ReceiverNameSurname = receiver != null ? receiver.Name + " " + receiver.Surname : value.ReceiverEmail;
            ViewBag.ReceiverImageUrl = receiver?.ImageUrl;
            ViewBag.UserImageUrl = user.ImageUrl;
            ViewBag.UserNameSurname = user.Name + " " + user.Surname;
            return View(value);
        }

        [HttpGet]
        public async Task<IActionResult> CreateMessage(string? receiverEmail)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            GetCreateMessageValues(user);

            CreateMessageViewModel createMessageViewModel = new CreateMessageViewModel();
            createMessageViewModel.ReceiverEmail = receiverEmail;
            return View(createMessageViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(CreateMessageViewModel createMessageViewModel, string? submitType)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            Message message;
            if (createMessageViewModel.MessageId != 0)
            {
                message = _context.Messages.FirstOrDefault(x => x.MessageId == createMessageViewModel.MessageId && x.SenderEmail == user.Email && x.IsDraft);
                if (message == null)
                {
                    return NotFound();
                }
            }
            else
            {
                message = new Message();
                message.SenderEmail = user.Email;
            }

            if (submitType == "draft")
            {
                message.ReceiverEmail = createMessageViewModel.ReceiverEmail?.Trim() ?? "";
                message.Subject = createMessageViewModel.Subject?.Trim() ?? "";
                message.MessageDetail = createMessageViewModel.MessageDetail ?? "";
                message.CategoryId = createMessageViewModel.CategoryId;
                message.SendDate = DateTime.Now;
                message.IsDraft = true;
                message.IsTrashBySender = false;

                if (message.MessageId == 0)
                {
                    _context.Messages.Add(message);
                }
                _context.SaveChanges();

                TempData["Success"] = "Mesajınız taslak olarak kaydedildi.";
                return RedirectToAction("Drafts");
            }

            if (createMessageViewModel.CategoryId != null && !_context.Categories.Any(x => x.CategoryId == createMessageViewModel.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Lütfen geçerli bir kategori seçin.");
            }

            if (!ModelState.IsValid)
            {
                GetCreateMessageValues(user);
                return View(createMessageViewModel);
            }

            message.ReceiverEmail = createMessageViewModel.ReceiverEmail.Trim();
            message.Subject = createMessageViewModel.Subject.Trim();
            message.MessageDetail = createMessageViewModel.MessageDetail;
            message.CategoryId = createMessageViewModel.CategoryId;
            message.SendDate = DateTime.Now;
            message.IsDraft = false;
            message.IsRead = false;
            message.IsTrashBySender = false;

            var receiver = await _userManager.FindByEmailAsync(message.ReceiverEmail);
            if (receiver == null)
            {
                try
                {
                    MimeMessage mimeMessage = new MimeMessage();
                    MailboxAddress mailboxAddressFrom = new MailboxAddress(user.Name + " " + user.Surname + " (Mailora)", "talhacardd@gmail.com");
                    mimeMessage.From.Add(mailboxAddressFrom);
                    mimeMessage.ReplyTo.Add(new MailboxAddress(user.Name + " " + user.Surname, user.Email));

                    MailboxAddress mailboxAddressTo = new MailboxAddress(message.ReceiverEmail, message.ReceiverEmail);
                    mimeMessage.To.Add(mailboxAddressTo);

                    mimeMessage.Subject = message.Subject;
                    var bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = message.MessageDetail;
                    mimeMessage.Body = bodyBuilder.ToMessageBody();

                    SmtpClient smtpClient = new SmtpClient();
                    smtpClient.Connect("smtp.gmail.com", 587, false);
                    smtpClient.Authenticate("talhacardd@gmail.com", "dmop rsvw ykds uyqe");
                    smtpClient.Send(mimeMessage);
                    smtpClient.Disconnect(true);
                }
                catch
                {
                    ModelState.AddModelError("", "E-posta gönderilirken bir hata oluştu. Lütfen alıcı adresini kontrol edip tekrar deneyin.");
                    GetCreateMessageValues(user);
                    return View(createMessageViewModel);
                }
            }

            if (message.MessageId == 0)
            {
                _context.Messages.Add(message);
            }
            _context.SaveChanges();

            TempData["Success"] = receiver == null ? "Mesajınız " + message.ReceiverEmail + " adresine e-posta olarak gönderildi." : "Mesajınız " + receiver.Name + " " + receiver.Surname + " kişisine gönderildi.";
            return RedirectToAction("Sendbox");
        }

        public async Task<IActionResult> EditDraft(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var value = _context.Messages.FirstOrDefault(x => x.MessageId == id);
            if (value == null || value.IsDeletedBySender)
            {
                return NotFound();
            }

            if (value.SenderEmail != user.Email)
            {
                return Forbid();
            }

            if (!value.IsDraft)
            {
                return RedirectToAction("MessageDetail", new { id = id });
            }

            CreateMessageViewModel createMessageViewModel = new CreateMessageViewModel();
            createMessageViewModel.MessageId = value.MessageId;
            createMessageViewModel.ReceiverEmail = value.ReceiverEmail;
            createMessageViewModel.Subject = value.Subject;
            createMessageViewModel.MessageDetail = value.MessageDetail;
            createMessageViewModel.CategoryId = value.CategoryId;

            GetCreateMessageValues(user);
            ViewBag.PageTitle = "Taslağı Düzenle";
            return View("CreateMessage", createMessageViewModel);
        }

        public async Task<IActionResult> ReplyMessage(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var value = _context.Messages.FirstOrDefault(x => x.MessageId == id && !x.IsDraft);
            if (value == null)
            {
                return NotFound();
            }

            if (value.ReceiverEmail != user.Email && value.SenderEmail != user.Email)
            {
                return Forbid();
            }

            CreateMessageViewModel createMessageViewModel = new CreateMessageViewModel();
            createMessageViewModel.ReceiverEmail = value.ReceiverEmail == user.Email ? value.SenderEmail : value.ReceiverEmail;
            createMessageViewModel.Subject = value.Subject.StartsWith("Re:") ? value.Subject : "Re: " + value.Subject;
            createMessageViewModel.CategoryId = value.CategoryId;
            createMessageViewModel.MessageDetail = "<p><br></p><blockquote><p><b>" + value.SendDate.ToString("dd.MM.yyyy HH:mm") + "</b> tarihinde <b>" + WebUtility.HtmlEncode(value.SenderEmail) + "</b> yazdı:</p>" + value.MessageDetail + "</blockquote>";

            GetCreateMessageValues(user);
            ViewBag.PageTitle = "Mesajı Yanıtla";
            return View("CreateMessage", createMessageViewModel);
        }

        public async Task<IActionResult> ForwardMessage(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var value = _context.Messages.FirstOrDefault(x => x.MessageId == id && !x.IsDraft);
            if (value == null)
            {
                return NotFound();
            }

            if (value.ReceiverEmail != user.Email && value.SenderEmail != user.Email)
            {
                return Forbid();
            }

            CreateMessageViewModel createMessageViewModel = new CreateMessageViewModel();
            createMessageViewModel.Subject = value.Subject.StartsWith("Fwd:") ? value.Subject : "Fwd: " + value.Subject;
            createMessageViewModel.CategoryId = value.CategoryId;
            createMessageViewModel.MessageDetail = "<p><br></p><p>---------- İletilen mesaj ----------<br>" +
                                                   "<b>Kimden:</b> " + WebUtility.HtmlEncode(value.SenderEmail) + "<br>" +
                                                   "<b>Kime:</b> " + WebUtility.HtmlEncode(value.ReceiverEmail) + "<br>" +
                                                   "<b>Tarih:</b> " + value.SendDate.ToString("dd.MM.yyyy HH:mm") + "<br>" +
                                                   "<b>Konu:</b> " + WebUtility.HtmlEncode(value.Subject) + "</p>" + value.MessageDetail;

            GetCreateMessageValues(user);
            ViewBag.PageTitle = "Mesajı İlet";
            return View("CreateMessage", createMessageViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int id, string? returnUrl)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var value = _context.Messages.FirstOrDefault(x => x.MessageId == id && x.ReceiverEmail == user.Email && !x.IsDraft);
            if (value == null)
            {
                return NotFound();
            }

            value.IsFavorite = !value.IsFavorite;
            _context.SaveChanges();

            TempData["Success"] = value.IsFavorite ? "Mesaj yıldızlı mesajlara eklendi." : "Mesajın yıldızı kaldırıldı.";
            return RedirectToReturnUrl(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(List<int> ids, string? returnUrl)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var values = _context.Messages.Where(x => ids.Contains(x.MessageId) && x.ReceiverEmail == user.Email && !x.IsDraft).ToList();
            foreach (var item in values)
            {
                item.IsRead = true;
            }
            _context.SaveChanges();

            TempData["Success"] = values.Count + " mesaj okundu olarak işaretlendi.";
            return RedirectToReturnUrl(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsUnread(List<int> ids, string? returnUrl)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var values = _context.Messages.Where(x => ids.Contains(x.MessageId) && x.ReceiverEmail == user.Email && !x.IsDraft).ToList();
            foreach (var item in values)
            {
                item.IsRead = false;
            }
            _context.SaveChanges();

            TempData["Success"] = values.Count + " mesaj okunmadı olarak işaretlendi.";
            return RedirectToReturnUrl(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> MoveToTrash(List<int> ids, string? returnUrl)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var values = _context.Messages.Where(x => ids.Contains(x.MessageId) && (x.ReceiverEmail == user.Email || x.SenderEmail == user.Email)).ToList();
            int count = 0;
            foreach (var item in values)
            {
                bool isChanged = false;
                if (item.SenderEmail == user.Email)
                {
                    item.IsTrashBySender = true;
                    isChanged = true;
                }
                if (item.ReceiverEmail == user.Email && !item.IsDraft)
                {
                    item.IsTrashByReceiver = true;
                    isChanged = true;
                }
                if (isChanged)
                {
                    count++;
                }
            }
            _context.SaveChanges();

            TempData["Success"] = count + " mesaj çöp kutusuna taşındı.";
            return RedirectToReturnUrl(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> RestoreMessage(List<int> ids, string? returnUrl)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var values = _context.Messages.Where(x => ids.Contains(x.MessageId) && (x.ReceiverEmail == user.Email || x.SenderEmail == user.Email)).ToList();
            int count = 0;
            foreach (var item in values)
            {
                bool isChanged = false;
                if (item.SenderEmail == user.Email && item.IsTrashBySender && !item.IsDeletedBySender)
                {
                    item.IsTrashBySender = false;
                    isChanged = true;
                }
                if (item.ReceiverEmail == user.Email && item.IsTrashByReceiver && !item.IsDeletedByReceiver)
                {
                    item.IsTrashByReceiver = false;
                    isChanged = true;
                }
                if (isChanged)
                {
                    count++;
                }
            }
            _context.SaveChanges();

            TempData["Success"] = count + " mesaj geri yüklendi.";
            return RedirectToReturnUrl(returnUrl ?? Url.Action("Trash"));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(List<int> ids, string? returnUrl)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var values = _context.Messages.Where(x => ids.Contains(x.MessageId) && (x.ReceiverEmail == user.Email || x.SenderEmail == user.Email)).ToList();
            int count = 0;
            foreach (var item in values)
            {
                bool isChanged = false;
                if (item.SenderEmail == user.Email && item.IsTrashBySender && !item.IsDeletedBySender)
                {
                    item.IsDeletedBySender = true;
                    isChanged = true;
                }
                if (item.ReceiverEmail == user.Email && item.IsTrashByReceiver && !item.IsDeletedByReceiver)
                {
                    item.IsDeletedByReceiver = true;
                    isChanged = true;
                }
                if (isChanged)
                {
                    count++;
                }

                if ((item.IsDraft && item.IsDeletedBySender) || (item.IsDeletedBySender && item.IsDeletedByReceiver))
                {
                    _context.Messages.Remove(item);
                }
            }
            _context.SaveChanges();

            TempData["Success"] = count + " mesaj kalıcı olarak silindi.";
            return RedirectToReturnUrl(returnUrl ?? Url.Action("Trash"));
        }

        private void GetFilterValues(string? search, int? categoryId, string? filter, string? sort)
        {
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Filter = filter;
            ViewBag.Sort = sort == "oldest" ? "oldest" : "newest";
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName", categoryId);
        }

        private void GetCreateMessageValues(AppUser user)
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName");
            ViewBag.SenderNameSurname = user.Name + " " + user.Surname;
            ViewBag.SenderEmail = user.Email;
        }

        private IActionResult RedirectToReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Inbox");
        }
    }
}
