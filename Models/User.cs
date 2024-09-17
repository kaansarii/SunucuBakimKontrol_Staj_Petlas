using SunucuBakimKontrol.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required(ErrorMessage = "İsim zorunludur.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Soyisim zorunludur.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Kullanıcı Adı zorunludur.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Email zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Şifre zorunludur.")]
    public string Password { get; set; }

    public string Role { get; set; }
    

    public ICollection<Server> Servers { get; set; } = new List<Server>();
   
}
