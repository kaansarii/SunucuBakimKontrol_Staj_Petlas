using BCrypt.Net;
using DocumentFormat.OpenXml.InkML;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using SunucuBakimKontrol.Data;
using SunucuBakimKontrol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SunucuBakimKontrol.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            var loggedInUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == user.Username);

            if (loggedInUser != null && BCrypt.Net.BCrypt.Verify(user.Password, loggedInUser.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loggedInUser.Username),
                    new Claim(ClaimTypes.Role, loggedInUser.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); //burada kendi role kullanıcak
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                string? serverIpAddress = Dns.GetHostEntry(Dns.GetHostName())
                                          .AddressList
                                          .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                          ?.ToString();

                TempData["serverIpAddress"] = serverIpAddress;

                HttpContext.Session.SetString("LoggedInUsername", loggedInUser.Username);
                HttpContext.Session.SetString("UserRole", loggedInUser.Role);

                return RedirectToAction("MainPage", "Home");
            }

            ViewBag.Message = "Hatalı Kullanıcı Adı veya Şifre";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    var user = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Username = model.Username,
                        Email = model.Email,
                        Password = hashedPassword,
                        Role = "User"
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    ViewBag.Message = "Kayıt başarılı!";
                    return View();
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"Kayıt başarısız oldu: {ex.Message}";
                    return View(model);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ViewBag.Message = "Kayıt başarısız oldu: Model doğrulama hatası.";
                ViewBag.Errors = errors;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(MailRequest mailRequest)
        {
            if (string.IsNullOrEmpty(mailRequest.ReceiverEmail))
            {
                ModelState.AddModelError("", "Lütfen e-posta adresini doldurunuz.");
                return View(mailRequest);
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == mailRequest.ReceiverEmail);
                if (user == null)
                {
                    ModelState.AddModelError("", "E-posta adresi ile eşleşen kullanıcı bulunamadı.");
                    return View(mailRequest);
                }

                var newPassword = GenerateNewPassword();

                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                var mimeMessage = new MimeMessage();

                var mailboxAddressFrom = new MailboxAddress("Petlas Destek", "petlasdeneme@gmail.com");
                mimeMessage.From.Add(mailboxAddressFrom);

                var mailboxAddressTo = new MailboxAddress("User", mailRequest.ReceiverEmail);
                mimeMessage.To.Add(mailboxAddressTo);

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = $"Yeni şifreniz: {newPassword}\nLütfen bu şifreyi kullanarak giriş yapın."
                };
                mimeMessage.Body = bodyBuilder.ToMessageBody();
                mimeMessage.Subject = "Yeni Şifre";

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("petlasdeneme@gmail.com", "zmvu bhow yjsc jhud");
                    client.Send(mimeMessage);
                    client.Disconnect(true);
                }

                ViewBag.Message = "Yeni şifreniz e-posta adresinize gönderildi!";
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Bir hata oluştu: {ex.Message}";
            }

            return View();
        }

         private string GenerateNewPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(CombinedViewModel model)
        {
            string loggedInUsername = HttpContext.Session.GetString("LoggedInUsername");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
            {
                ModelState.AddModelError(string.Empty, "Mevcut şifre yanlış.");
                return View("Profile", model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Yeni şifre ve şifre tekrarı uyuşmuyor.");
                return View("Profile", model);
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Şifre başarıyla değiştirildi.";

            model.Username = user.Username;
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;

            return View("Profile", model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

    }
}