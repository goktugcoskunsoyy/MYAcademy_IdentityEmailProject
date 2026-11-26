using System.ComponentModel.DataAnnotations;

namespace MYAcademy_IdentityEmailProject.Models
{
    public class CreateMessageViewModel
    {
        public int MessageId { get; set; }

        [Required(ErrorMessage = "Alıcı e-posta adresini girin.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin (ör. ad@alanadi.com).")]
        public string? ReceiverEmail { get; set; }

        [Required(ErrorMessage = "Konu alanı boş bırakılamaz.")]
        [StringLength(200, ErrorMessage = "Konu en fazla 200 karakter olabilir.")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "Mesaj içeriği boş olamaz.")]
        public string? MessageDetail { get; set; }

        [Required(ErrorMessage = "Lütfen bir kategori seçin.")]
        public int? CategoryId { get; set; }
    }
}
