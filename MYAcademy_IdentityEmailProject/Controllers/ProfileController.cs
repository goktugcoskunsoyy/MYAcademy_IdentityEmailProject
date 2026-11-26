using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IdentityContext _context;

        public ProfileController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IdentityContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            UserEditViewModel userEditViewModel = new UserEditViewModel();
            userEditViewModel.Name = user.Name;
            userEditViewModel.Surname = user.Surname;
            userEditViewModel.UserName = user.UserName;
            userEditViewModel.Email = user.Email;
            userEditViewModel.PhoneNumber = user.PhoneNumber;
            userEditViewModel.Description = user.Description;
            userEditViewModel.ImageUrl = user.ImageUrl;
            GetProfileStatistics(user);
            return View(userEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserEditViewModel userEditViewModel)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (string.IsNullOrWhiteSpace(userEditViewModel.Name) || string.IsNullOrWhiteSpace(userEditViewModel.Surname))
            {
                ModelState.AddModelError("", "Ad ve soyad alanları boş bırakılamaz.");
            }

            if (userEditViewModel.Description != null && userEditViewModel.Description.Length > 300)
            {
                ModelState.AddModelError("", "Hakkımda alanı en fazla 300 karakter olabilir.");
            }

            if (userEditViewModel.Image != null)
            {
                var extension = Path.GetExtension(userEditViewModel.Image.FileName).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "Profil fotoğrafı yalnızca JPG, PNG, WEBP veya GIF formatında olabilir.");
                }
                else if (userEditViewModel.Image.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Profil fotoğrafı en fazla 2 MB olabilir.");
                }
            }

            if (ModelState.ErrorCount > 0)
            {
                userEditViewModel.UserName = user.UserName;
                userEditViewModel.Email = user.Email;
                userEditViewModel.ImageUrl = user.ImageUrl;
                GetProfileStatistics(user);
                return View(userEditViewModel);
            }

            if (userEditViewModel.Image != null)
            {
                var resource = Directory.GetCurrentDirectory();
                var extension = Path.GetExtension(userEditViewModel.Image.FileName).ToLower();
                var imageName = Guid.NewGuid() + extension;
                var saveFolder = Path.Combine(resource, "wwwroot", "userimages");
                Directory.CreateDirectory(saveFolder);
                var saveLocation = Path.Combine(saveFolder, imageName);
                using (var stream = new FileStream(saveLocation, FileMode.Create))
                {
                    await userEditViewModel.Image.CopyToAsync(stream);
                }
                user.ImageUrl = "/userimages/" + imageName;
            }

            user.Name = userEditViewModel.Name.Trim();
            user.Surname = userEditViewModel.Surname.Trim();
            user.PhoneNumber = userEditViewModel.PhoneNumber;
            user.Description = userEditViewModel.Description;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profil bilgileriniz başarıyla güncellendi.";
                return RedirectToAction("Index");
            }

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description);
            }
            userEditViewModel.UserName = user.UserName;
            userEditViewModel.Email = user.Email;
            userEditViewModel.ImageUrl = user.ImageUrl;
            GetProfileStatistics(user);
            return View(userEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveImage()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            user.ImageUrl = null;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profil fotoğrafınız kaldırıldı.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (string.IsNullOrEmpty(changePasswordViewModel.CurrentPassword) || string.IsNullOrEmpty(changePasswordViewModel.NewPassword))
            {
                TempData["PasswordError"] = "Mevcut şifre ve yeni şifre alanları boş bırakılamaz.";
                return RedirectToAction("Index");
            }

            if (changePasswordViewModel.NewPassword != changePasswordViewModel.NewPasswordConfirm)
            {
                TempData["PasswordError"] = "Yeni şifre ve şifre tekrarı birbiriyle eşleşmiyor.";
                return RedirectToAction("Index");
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.CurrentPassword, changePasswordViewModel.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction("Index");
            }

            TempData["PasswordError"] = string.Join(" ", result.Errors.Select(x => x.Description));
            return RedirectToAction("Index");
        }

        private void GetProfileStatistics(AppUser user)
        {
            ViewBag.SendboxCount = _context.Messages.Count(x => x.SenderEmail == user.Email && !x.IsDraft && !x.IsDeletedBySender);
            ViewBag.InboxCount = _context.Messages.Count(x => x.ReceiverEmail == user.Email && !x.IsDraft && !x.IsDeletedByReceiver);
            ViewBag.UnreadCount = _context.Messages.Count(x => x.ReceiverEmail == user.Email && !x.IsDraft && !x.IsRead && !x.IsTrashByReceiver && !x.IsDeletedByReceiver);
            ViewBag.FavoriteCount = _context.Messages.Count(x => x.ReceiverEmail == user.Email && !x.IsDraft && x.IsFavorite && !x.IsTrashByReceiver && !x.IsDeletedByReceiver);
            ViewBag.UserId = user.Id;
            ViewBag.EmailConfirmed = user.EmailConfirmed;
            ViewBag.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
        }
    }
}
