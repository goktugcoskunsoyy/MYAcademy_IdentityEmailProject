using MailKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public RegisterController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserRegisterViewModel userRegisterViewModel)
        {
            if (userRegisterViewModel.Password != userRegisterViewModel.PasswordConfirm)
            {
                ModelState.AddModelError("", "Şifre ve şifre tekrarı birbiriyle eşleşmiyor.");
                return View(userRegisterViewModel);
            }

            Random random = new Random();
            int validCode = random.Next(100000, 1000000);

            AppUser appUser = new AppUser()
            {
                Name = userRegisterViewModel.Name,
                Surname = userRegisterViewModel.Surname,
                Email = userRegisterViewModel.Email,
                UserName = userRegisterViewModel.UserName,
                ValidCode = validCode
            };

            var result = await _userManager.CreateAsync(appUser, userRegisterViewModel.Password);
            if (result.Succeeded)
            {
                MimeMessage mimeMessage = new MimeMessage();
                MailboxAddress mailboxAddressFrom = new MailboxAddress("Mailora Yönetim Paneli", "talhacardd@gmail.com");
                mimeMessage.From.Add(mailboxAddressFrom);

                MailboxAddress mailboxAddressTo = new MailboxAddress("User", userRegisterViewModel.Email);
                mimeMessage.To.Add(mailboxAddressTo);

                mimeMessage.Subject = "Email Doğrulama Kodu";
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = "Sisteme giriş yapabilmek için gerekli olan doğrulama kodu: " + validCode;
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Connect("smtp.gmail.com", 587, false);
                smtpClient.Authenticate("talhacardd@gmail.com", "dmop rsvw ykds uyqe");
                smtpClient.Send(mimeMessage);
                smtpClient.Disconnect(true);

                TempData["CodeSent"] = "Doğrulama kodu e-posta adresinize gönderildi.";
                return RedirectToAction("Index", "ConfirmMail", new { email = appUser.Email });
            }
            else
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View(userRegisterViewModel);
            }
        }
    }
}
