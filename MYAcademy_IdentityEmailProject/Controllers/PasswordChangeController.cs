using MailKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;
using System.Net;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class PasswordChangeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public PasswordChangeController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel forgetPasswordViewModel)
        {
            if (string.IsNullOrWhiteSpace(forgetPasswordViewModel.Email))
            {
                ModelState.AddModelError("", "Lütfen e-posta adresinizi girin.");
                return View(forgetPasswordViewModel);
            }

            var user = await _userManager.FindByEmailAsync(forgetPasswordViewModel.Email.Trim());
            if (user != null)
            {
                string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResetTokenLink = Url.Action("ResetPassword", "PasswordChange", new
                {
                    userId = user.Id,
                    token = passwordResetToken
                }, HttpContext.Request.Scheme);

                MimeMessage mimeMessage = new MimeMessage();
                MailboxAddress mailboxAddressFrom = new MailboxAddress("Mailora Yönetim Paneli", "talhacardd@gmail.com");
                mimeMessage.From.Add(mailboxAddressFrom);

                MailboxAddress mailboxAddressTo = new MailboxAddress(user.Name + " " + user.Surname, user.Email);
                mimeMessage.To.Add(mailboxAddressTo);

                mimeMessage.Subject = "Mailora Şifre Yenileme";
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = "<p>Merhaba " + WebUtility.HtmlEncode(user.Name) + ",</p>" +
                                       "<p>Şifrenizi yenilemek için aşağıdaki bağlantıya tıklayın. Bağlantı 30 dakika geçerlidir ve yalnızca bir kez kullanılabilir.</p>" +
                                       "<p><a href=\"" + passwordResetTokenLink + "\">Şifremi Yenile</a></p>" +
                                       "<p>Bu isteği siz yapmadıysanız bu e-postayı dikkate almayın.</p>";
                bodyBuilder.TextBody = "Şifrenizi yenilemek için bağlantı: " + passwordResetTokenLink;
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Connect("smtp.gmail.com", 587, false);
                smtpClient.Authenticate("talhacardd@gmail.com", "dmop rsvw ykds uyqe");
                smtpClient.Send(mimeMessage);
                smtpClient.Disconnect(true);
            }

            ViewBag.IsSent = true;
            return View(forgetPasswordViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string? userId, string? token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                ViewBag.IsInvalid = true;
                return View();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.IsInvalid = true;
                return View();
            }

            var isValid = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, UserManager<AppUser>.ResetPasswordTokenPurpose, token);
            if (!isValid)
            {
                ViewBag.IsInvalid = true;
                return View();
            }

            ResetPasswordViewModel resetPasswordViewModel = new ResetPasswordViewModel();
            resetPasswordViewModel.UserId = userId;
            resetPasswordViewModel.Token = token;
            return View(resetPasswordViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (string.IsNullOrEmpty(resetPasswordViewModel.UserId) || string.IsNullOrEmpty(resetPasswordViewModel.Token))
            {
                ViewBag.IsInvalid = true;
                return View();
            }

            if (string.IsNullOrEmpty(resetPasswordViewModel.Password) || resetPasswordViewModel.Password != resetPasswordViewModel.PasswordConfirm)
            {
                ModelState.AddModelError("", "Şifre ve şifre tekrarı birbiriyle eşleşmiyor.");
                return View(resetPasswordViewModel);
            }

            var user = await _userManager.FindByIdAsync(resetPasswordViewModel.UserId);
            if (user == null)
            {
                ViewBag.IsInvalid = true;
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Token, resetPasswordViewModel.Password);
            if (result.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["Success"] = "Şifreniz başarıyla yenilendi. Yeni şifrenizle giriş yapabilirsiniz.";
                return RedirectToAction("SignIn", "Login");
            }

            if (result.Errors.Any(x => x.Code == "InvalidToken"))
            {
                ViewBag.IsInvalid = true;
                return View();
            }

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description);
            }
            return View(resetPasswordViewModel);
        }
    }
}
