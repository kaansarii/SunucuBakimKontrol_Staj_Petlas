using System.ComponentModel.DataAnnotations;

namespace SunucuBakimKontrol.Models
{
    public class CombinedViewModel
    {
        public int UserId { get; set; } 

        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required(ErrorMessage = "Mevcut şifre zorunludur.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre tekrar zorunludur.")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifre ve şifre tekrarı uyuşmuyor.")]
        public string ConfirmPassword { get; set; }
    }
}