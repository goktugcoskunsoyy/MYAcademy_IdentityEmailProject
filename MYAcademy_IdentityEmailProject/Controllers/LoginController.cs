using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public LoginController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult SignIn(string? returnUrl)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(UserLoginViewModel userLoginViewModel, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(userLoginViewModel.UserName) || string.IsNullOrEmpty(userLoginViewModel.Password))
            {
                ModelState.AddModelError("", "Kullanıcı adı ve şifre alanları boş bırakılamaz.");
                return View(userLoginViewModel);
            }

            var user = await _userManager.FindByNameAsync(userLoginViewModel.UserName.Trim());
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(userLoginViewModel.UserName.Trim());
            }

            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                return View(userLoginViewModel);
            }

            var result = await _signInManager.PasswordSignInAsync(user, userLoginViewModel.Password, userLoginViewModel.RememberMe, true);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Dashboard");
            }

            if (result.IsNotAllowed)
            {
                TempData["Error"] = "Giriş yapabilmek için önce e-posta adresinizi doğrulamalısınız.";
                return RedirectToAction("Index", "ConfirmMail", new { email = user.Email });
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Çok fazla hatalı deneme yaptınız. Hesabınız 5 dakika süreyle kilitlendi.");
                return View(userLoginViewModel);
            }

            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View(userLoginViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("SignIn", "Login");
        }
    }
}
