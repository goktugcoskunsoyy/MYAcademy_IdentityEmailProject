using Microsoft.AspNetCore.Identity;

namespace MYAcademy_IdentityEmailProject.Models
{
    public class CustomIdentityValidator : IdentityErrorDescriber
    {
        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError()
            {
                Code = "PasswordTooShort",
                Description = $"Şifre en az {length} karakter olmalıdır."
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresUpper",
                Description = "Şifre en az bir büyük harf içermelidir."
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresLower",
                Description = "Şifre en az bir küçük harf içermelidir."
            };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresDigit",
                Description = "Şifre en az bir rakam içermelidir."
            };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresNonAlphanumeric",
                Description = "Şifre en az bir özel karakter (! @ # $ vb.) içermelidir."
            };
        }

        public override IdentityError PasswordMismatch()
        {
            return new IdentityError()
            {
                Code = "PasswordMismatch",
                Description = "Mevcut şifrenizi hatalı girdiniz."
            };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError()
            {
                Code = "DuplicateUserName",
                Description = $"{userName} kullanıcı adı zaten kullanılıyor."
            };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError()
            {
                Code = "DuplicateEmail",
                Description = $"{email} adresiyle kayıtlı bir hesap zaten var."
            };
        }

        public override IdentityError InvalidUserName(string? userName)
        {
            return new IdentityError()
            {
                Code = "InvalidUserName",
                Description = "Kullanıcı adı yalnızca harf, rakam ve . _ - karakterlerinden oluşabilir."
            };
        }

        public override IdentityError InvalidEmail(string? email)
        {
            return new IdentityError()
            {
                Code = "InvalidEmail",
                Description = "Geçerli bir e-posta adresi girin."
            };
        }

        public override IdentityError InvalidToken()
        {
            return new IdentityError()
            {
                Code = "InvalidToken",
                Description = "Bağlantının süresi dolmuş veya bağlantı daha önce kullanılmış."
            };
        }
    }
}
