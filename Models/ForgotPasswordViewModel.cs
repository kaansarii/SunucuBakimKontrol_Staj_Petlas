using System.ComponentModel.DataAnnotations;

namespace SunucuBakimKontrol.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
