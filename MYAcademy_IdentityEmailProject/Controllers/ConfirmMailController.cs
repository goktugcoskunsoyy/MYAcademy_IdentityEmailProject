using MailKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class ConfirmMailController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public ConfirmMailController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("SignUp", "Register");
            }

            ConfirmMailViewModel confirmMailViewModel = new ConfirmMailViewModel();
            confirmMailViewModel.Email = email;
            return View(confirmMailViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ConfirmMailViewModel confirmMailViewModel)
        {
            var user = await _userManager.FindByEmailAsync(confirmMailViewModel.Email ?? "");
            if (user == null)
            {
                ModelState.AddModelError("", "Bu e-posta adresine ait bir hesap bulunamadı.");
                return View(confirmMailViewModel);
            }

            if (user.EmailConfirmed)
            {
                ViewBag.IsConfirmed = true;
                return View(confirmMailViewModel);
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError("", "Çok fazla hatalı deneme yaptınız. Lütfen birkaç dakika sonra tekrar deneyin.");
                return View(confirmMailViewModel);
            }

            if (user.ValidCode != 0 && user.ValidCode == confirmMailViewModel.ValidCode)
            {
                user.EmailConfirmed = true;
                user.ValidCode = 0;
                await _userManager.UpdateAsync(user);
                await _userManager.ResetAccessFailedCountAsync(user);
                ViewBag.IsConfirmed = true;
                return View(confirmMailViewModel);
            }

            await _userManager.AccessFailedAsync(user);
            ModelState.AddModelError("", "Girdiğiniz kod hatalı. Lütfen tekrar deneyin.");
            return View(confirmMailViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ResendCode(string email)
        {
            var user = await _userManager.FindByEmailAsync(email ?? "");
            if (user != null && !user.EmailConfirmed)
            {
                Random random = new Random();
                int validCode = random.Next(100000, 1000000);
                user.ValidCode = validCode;
                await _userManager.UpdateAsync(user);

                MimeMessage mimeMessage = new MimeMessage();
                MailboxAddress mailboxAddressFrom = new MailboxAddress("Mailora Yönetim Paneli", "talhacardd@gmail.com");
                mimeMessage.From.Add(mailboxAddressFrom);

                MailboxAddress mailboxAddressTo = new MailboxAddress("User", user.Email);
                mimeMessage.To.Add(mailboxAddressTo);

                mimeMessage.Subject = "Email Doğrulama Kodu";
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = "Sisteme giriş yapabilmek için gerekli olan yeni doğrulama kodu: " + validCode;
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Connect("smtp.gmail.com", 587, false);
                smtpClient.Authenticate("talhacardd@gmail.com", "dmop rsvw ykds uyqe");
                smtpClient.Send(mimeMessage);
                smtpClient.Disconnect(true);
            }

            TempData["CodeSent"] = "Yeni doğrulama kodu e-posta adresinize gönderildi.";
            return RedirectToAction("Index", new { email = email });
        }
    }
}
