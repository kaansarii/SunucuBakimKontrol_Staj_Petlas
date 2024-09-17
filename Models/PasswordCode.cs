using System.ComponentModel.DataAnnotations;

namespace SunucuBakimKontrol.Models
{
    public class PasswordCode
    {
        public int Id { get; set; }

        public User User { get; set; }
        public int UserId { get; set; }

        [StringLength(6)]

        public string Code { get; set; }
    }
}
