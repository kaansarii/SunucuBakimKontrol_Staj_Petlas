using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using SunucuBakimKontrol.Data;
using SunucuBakimKontrol.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.AspNetCore.Identity;

namespace SunucuBakimKontrol.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        

        public HomeController(ApplicationDbContext context )
        {
            _context = context;
            
        }

         // Kullanıcı giriş yaptıktan sonra giriş yapan kullanıcıya ait tabloya erişim 

        public async Task<IActionResult> MainPage()
        {
            var currentDate = DateTime.UtcNow;
            var startDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            
            string loggedInUsername = HttpContext.Session.GetString("LoggedInUsername");

            if (string.IsNullOrEmpty(loggedInUsername))
            {
                
                return RedirectToAction("Login", "Account");
            }

            
            var loggedInUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loggedInUsername);

            if (loggedInUser == null)
            {
                
                return RedirectToAction("Login", "Account");
            }

            List<Server> servers;

            if (loggedInUser.Role == "Admin")
            {
                
                servers = await _context.Servers
                    .Include(s => s.Responsible)
                    .Include(s => s.MaintenancePoints.Where(mp => mp.IsActive))
                        .ThenInclude(mp => mp.MaintenanceLogs)
                    .Where(s => s.IsActive) 
                    .ToListAsync();
            }
            else
            {
                
                servers = await _context.Servers
                    .Include(s => s.Responsible)
                    .Include(s => s.MaintenancePoints.Where(mp => mp.IsActive))
                        .ThenInclude(mp => mp.MaintenanceLogs)
                    .Where(s => s.Responsible.Username == loggedInUsername && s.IsActive) 
                    .ToListAsync();
            }


            var holidays = await _context.Holidays
                .Where(h => h.IsHoliday)
                .Select(h => h.Date)
                .ToListAsync();

            ViewBag.Holidays = holidays;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View((startDate, endDate, servers));
        }

        // Anasayfada bulunan tabloya resmi tatil ekleme işlemi

        [HttpPost]
        public async Task<IActionResult> MarkHoliday(DateTime holidayDate)
        {
            var holiday = await _context.Holidays.FirstOrDefaultAsync(h => h.Date == holidayDate);

            if (holiday == null)
            {
                holiday = new Holiday
                {
                    Date = holidayDate,
                    IsHoliday = true
                };
                _context.Holidays.Add(holiday);
            }
            else
            {
                holiday.IsHoliday = !holiday.IsHoliday;
                if (!holiday.IsHoliday)
                {
                    _context.Holidays.Remove(holiday);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MainPage");
        }

        //Tabloyu excel olarak indirme işlemleri

        [HttpPost]
        public async Task<IActionResult> ExportToExcel(DateTime startDate, DateTime endDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var servers = await _context.Servers
                .Include(s => s.MaintenancePoints)
                    .ThenInclude(mp => mp.MaintenanceLogs)
                .Include(s => s.Responsible)
                .Where(s => s.IsActive)
                .ToListAsync();

            var holidays = await _context.Holidays
                .Where(h => h.IsHoliday)
                .Select(h => h.Date)
                .ToListAsync();

            var users = await _context.Users.ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Bakım Çizelgesi");


                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    var logoFile = new FileInfo(logoPath);
                    var picture = worksheet.Drawings.AddPicture("PetlasLogo", logoFile);
                    worksheet.Cells["A1:B5"].Merge = true;
                    picture.SetPosition(0, 0, 0, 0);
                    picture.SetSize(300, 100);
                }


                worksheet.Cells["C1:AB5"].Merge = true;
                worksheet.Cells["C1"].Value = "YAZILIM ŞEFLİĞİ SUNUCU PERİYODİK BAKIM ÇİZELGE FORMU";
                worksheet.Cells["C1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["C1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells["C1"].Style.Font.Size = 14;
                worksheet.Cells["C1"].Style.Font.Bold = true;


                worksheet.Cells["AC1:AE1"].Merge = true;
                worksheet.Cells["AC1"].Value = "Yayımlayan:";
                worksheet.Cells["AF1:AH1"].Merge = true;
                worksheet.Cells["AF1"].Value = "Yazılım Gel. Şef.";
                worksheet.Cells["AF1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["AF1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["AC2:AE2"].Merge = true;
                worksheet.Cells["AC2"].Value = "Onaylayan:";
                worksheet.Cells["AF2:AH2"].Merge = true;
                worksheet.Cells["AF2"].Value = "Yazılım Şefi";
                worksheet.Cells["AF2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["AF2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["AC3:AE3"].Merge = true;
                worksheet.Cells["AC3"].Value = "Yürürlülük:";
                worksheet.Cells["AF3:AH3"].Merge = true;

                worksheet.Cells["AC4:AE4"].Merge = true;
                worksheet.Cells["AC4"].Value = "Revizyon Tarihi:";
                worksheet.Cells["AF4:AH4"].Merge = true;

                worksheet.Cells["AC5:AE5"].Merge = true;
                worksheet.Cells["AC5"].Value = "Revizyon No:";
                worksheet.Cells["AF5:AH5"].Merge = true;

                worksheet.Cells["AC1:AH5"].Style.Border.Top.Style = ExcelBorderStyle.Thin;

                worksheet.Cells["AC1:AH5"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["AC1:AH5"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells["AC1:AH5"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;


                worksheet.Cells[6, 1].Value = "Sunucu Adresi";
                worksheet.Cells[6, 2].Value = "Bakım Kontrol Noktası";
                worksheet.Cells[6, 3].Value = "Sorumlu";


                worksheet.Cells[6, 1, 6, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[6, 1, 6, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                int colIndex = 4;
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    worksheet.Cells[6, colIndex].Value = date.ToString("dd/MM/yyyy");
                    worksheet.Cells[6, colIndex].Style.TextRotation = 90;
                    worksheet.Cells[6, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Column(colIndex).Width = 3;
                    worksheet.Cells[6, colIndex].Style.Font.Size = 8;


                    var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                    var isHoliday = holidays.Contains(date);
                    if (isWeekend || isHoliday)
                    {
                        worksheet.Cells[6, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[6, colIndex].Style.Fill.BackgroundColor.SetColor(Color.DarkGray);
                    }
                    colIndex++;
                }

                int rowIndex = 7;
                foreach (var server in servers)
                {
                    foreach (var point in server.MaintenancePoints)
                    {
                        worksheet.Cells[rowIndex, 1].Value = server.ServerAddress;
                        worksheet.Cells[rowIndex, 2].Value = point.MaintenancePointName;
                        worksheet.Cells[rowIndex, 3].Value = $"{server.Responsible?.FirstName} {server.Responsible?.LastName}";


                        worksheet.Cells[rowIndex, 1, rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowIndex, 1, rowIndex, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;




                        colIndex = 4;
                        for (var date = startDate; date <= endDate; date = date.AddDays(1))
                        {
                            var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                            var isHoliday = holidays.Contains(date);
                            var log = point.MaintenanceLogs.FirstOrDefault(ms => ms.LogDate.Date == date.Date);
                            if (log != null)
                            {
                                worksheet.Cells[rowIndex, colIndex].Value = log.IsCompleted ? "✓" : "X";
                                worksheet.Cells[rowIndex, colIndex].Style.Font.Bold = true;
                                worksheet.Cells[rowIndex, colIndex].Style.Font.Color.SetColor(log.IsCompleted ? Color.Green : Color.Red);
                            }
                            else if (!isHoliday && !isWeekend && date.Date < DateTime.Today.Date)
                            {
                                worksheet.Cells[rowIndex, colIndex].Value = "-";
                                worksheet.Cells[rowIndex, colIndex].Style.Font.Bold = true;
                            }
                            else if (!isHoliday && !isWeekend && date.Date >= DateTime.Today.Date)
                            {
                                worksheet.Cells[rowIndex, colIndex].Value = string.Empty;
                            }
                            if (isWeekend || isHoliday)
                            {
                                worksheet.Cells[rowIndex, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[rowIndex, colIndex].Style.Fill.BackgroundColor.SetColor(Color.DarkGray);
                                worksheet.Cells[rowIndex, colIndex].Value = string.Empty;
                            }
                            worksheet.Cells[rowIndex, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Cells[rowIndex, colIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            colIndex++;
                        }
                        rowIndex++;
                    }
                }

                var cellRange = worksheet.Cells[6, 1, rowIndex - 1, colIndex - 1];
                cellRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cellRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cellRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cellRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();


                int footerStartRow = rowIndex + 2;
                int footerEndRow = footerStartRow + 1;


                worksheet.Cells[footerStartRow - 2, 17, footerEndRow - 2, 18].Merge = true;
                worksheet.Cells[footerStartRow - 2, 17].Value = "Kontrol Eden:";
                worksheet.Cells[footerStartRow - 2, 17].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[footerStartRow - 2, 17].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[footerStartRow - 2, 17].Style.Font.Bold = true;

                var kontrolEdenler = users.Take(5).Select(u => $"{u.FirstName} {u.LastName}").ToList();
                int startColumn = 19;
                for (int i = 0; i < kontrolEdenler.Count; i++)
                {
                    int endColumn = startColumn + 1;
                    worksheet.Cells[footerStartRow - 2, startColumn, footerEndRow - 2, endColumn].Merge = true;
                    worksheet.Cells[footerStartRow - 2, startColumn].Value = kontrolEdenler[i];
                    worksheet.Cells[footerStartRow - 2, startColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[footerStartRow - 2, startColumn].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    startColumn += 2;
                }


                worksheet.Cells[footerEndRow - 1, 17, footerEndRow, 18].Merge = true;
                worksheet.Cells[footerEndRow - 1, 17].Value = "İmza:";
                worksheet.Cells[footerEndRow - 1, 17].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[footerEndRow - 1, 17].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                startColumn = 19;
                for (int i = 0; i < kontrolEdenler.Count; i++)
                {
                    int endColumn = startColumn + 1;
                    worksheet.Cells[footerEndRow - 1, startColumn, footerEndRow, endColumn].Merge = true;
                    startColumn += 2;
                }

                var onaylayan = users.Skip(5).FirstOrDefault();
                if (onaylayan != null)
                {
                    worksheet.Cells[footerStartRow - 2, 29, footerEndRow - 2, 30].Merge = true;
                    worksheet.Cells[footerStartRow - 2, 29].Value = "Onaylayan:";
                    worksheet.Cells[footerStartRow - 2, 29].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[footerStartRow - 2, 29].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells[footerStartRow - 2, 29].Style.Font.Bold = true;

                    worksheet.Cells[footerStartRow - 2, 31, footerEndRow - 2, 32].Merge = true;
                    worksheet.Cells[footerStartRow - 2, 31].Value = $"{onaylayan.FirstName} {onaylayan.LastName}"; //veritabanından çekiyor admin yetkisi olanı
                    worksheet.Cells[footerStartRow - 2, 31].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[footerStartRow - 2, 31].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    worksheet.Cells[footerEndRow - 1, 29, footerEndRow, 30].Merge = true;
                    worksheet.Cells[footerEndRow - 1, 29].Value = "İmza:";
                    worksheet.Cells[footerEndRow - 1, 29].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[footerEndRow - 1, 29].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    worksheet.Cells[footerEndRow - 1, 31, footerEndRow, 32].Merge = true;
                }

                worksheet.Cells[footerStartRow - 2, 33, footerEndRow - 2, 34].Merge = true;
                worksheet.Cells[footerStartRow - 2, 33].Value = "Açıklama";
                worksheet.Cells[footerStartRow - 2, 33].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[footerStartRow - 2, 33].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[footerEndRow - 1, 33, footerEndRow, 34].Merge = true;

                worksheet.Cells[footerStartRow - 2, 17, footerEndRow, 34].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[footerStartRow - 2, 17, footerEndRow, 34].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[footerStartRow - 2, 17, footerEndRow, 34].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[footerStartRow - 2, 17, footerEndRow, 34].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BakimCizelgesi.xlsx");
            }
        }


        // Sunucu ekleme/pasif/güncelleme işlemleri

        [HttpGet]
        public IActionResult AddRemoveServer()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRemoveServer(string username, string serverAddress, string serverName, string action)
        {
            // Kullanıcının oturum bilgilerini al
            string loggedInUsername = HttpContext.Session.GetString("LoggedInUsername");
            if (string.IsNullOrEmpty(loggedInUsername))
            {
                ViewBag.ErrorMessage = "Oturum bilgisi bulunamadı. Lütfen yeniden giriş yapın.";
                return View();
            }

            // Kullanıcının rolünü al
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loggedInUsername);
            if (user == null || user.Role != "Admin")
            {
                ViewBag.ErrorMessage = "Bu işlemi gerçekleştirme yetkiniz yok.";
                return View();
            }

            if (action == "add" || action == "update")
            {
                var responsibleUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (responsibleUser != null)
                {
                    var server = await _context.Servers.FirstOrDefaultAsync(s => s.ServerName == serverName);
                    if (server == null)
                    {
                        server = new Server
                        {
                            ServerAddress = serverAddress,
                            ServerName = serverName,
                            ResponsibleId = responsibleUser.UserId,
                            IsActive = true
                        };
                        _context.Servers.Add(server);
                        ViewBag.AddMessage = "Sunucu başarıyla eklendi.";
                    }
                    else
                    {
                        server.ServerAddress = serverAddress;
                        server.ResponsibleId = responsibleUser.UserId;
                        _context.Servers.Update(server);
                        ViewBag.UpdateMessage = "Sunucu adresi başarıyla güncellendi.";
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
                    ViewBag.AddMessage = "Kullanıcı bulunamadı.";
                }
            }
            else if (action == "remove")
            {
                if (!string.IsNullOrEmpty(serverName) && !string.IsNullOrEmpty(serverAddress))
                {
                    var server = await _context.Servers
                        .FirstOrDefaultAsync(s => s.ServerName == serverName && s.ServerAddress == serverAddress);
                    if (server != null)
                    {
                        server.IsActive = false;
                        _context.Servers.Update(server);
                        await _context.SaveChangesAsync();
                        ViewBag.RemoveMessage = "Sunucu başarıyla pasif hale getirildi.";
                    }
                    else
                    {
                        ViewBag.RemoveMessage = "Sunucu bulunamadı.";
                    }
                }
                else
                {
                    ViewBag.RemoveMessage = "Sunucu adı ve adresi gereklidir.";
                }
            }

            return View();
        }

        // Sunucuları Listeleme İşlemleri
        [HttpPost]
        public async Task<IActionResult> ListServers()
        {
            var servers = await _context.Servers
                .Include(s => s.Responsible)
                .Select(s => new
                {
                    s.ServerId,
                    s.ServerName,
                    s.ServerAddress,
                    Name = s.Responsible.Username,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            ViewBag.Servers = servers;

            return View("AddRemoveServer");
        }

        // Eğer Sunucu Pasifse Aktif Etme İşlemi
        [HttpPost]
        public async Task<IActionResult> ActivateServer(int serverId)
        {
            // Kullanıcının oturum bilgilerini al
            string loggedInUsername = HttpContext.Session.GetString("LoggedInUsername");
            if (string.IsNullOrEmpty(loggedInUsername))
            {
                ViewBag.ErrorMessage = "Oturum bilgisi bulunamadı. Lütfen yeniden giriş yapın.";
                return View("AddRemoveServer");
            }

            // Kullanıcının rolünü al
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loggedInUsername);
            if (user == null || user.Role != "Admin")
            {
                ViewBag.ErrorMessage = "Bu işlemi gerçekleştirme yetkiniz yok.";
                return View("AddRemoveServer");
            }

            var server = await _context.Servers.FirstOrDefaultAsync(s => s.ServerId == serverId);
            if (server != null)
            {
                server.IsActive = true;
                _context.Servers.Update(server);
                await _context.SaveChangesAsync();
                ViewBag.ActivateMessage = "Sunucu başarıyla aktif hale getirildi.";
            }
            else
            {
                ViewBag.ActivateMessage = "Sunucu bulunamadı.";
            }

            return View("AddRemoveServer");
        }


        // Kontrol Noktası Ekleme Pasife Çekme İşlemleri

        [HttpGet]
        public IActionResult AddRemoveControl()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRemoveControl(string serverAddress, string controlPoint, string action)
        {
            // Kullanıcının oturum bilgilerini al
            string loggedInUsername = HttpContext.Session.GetString("LoggedInUsername");
            if (string.IsNullOrEmpty(loggedInUsername))
            {
                ViewBag.ErrorMessage = "Oturum bilgisi bulunamadı. Lütfen yeniden giriş yapın.";
                return View();
            }

            // Kullanıcının rolünü al
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loggedInUsername);
            if (user == null || user.Role != "Admin")
            {
                ViewBag.ErrorMessage = "Bu işlemi gerçekleştirme yetkiniz yok.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(controlPoint))
            {
                ViewBag.ErrorMessage = "Kontrol noktası adı boş olamaz.";
                return View();
            }

            var server = await _context.Servers.FirstOrDefaultAsync(s => s.ServerAddress == serverAddress);
            if (server == null)
            {
                ViewBag.ErrorServerMessage = "Sunucu bulunamadı. Lütfen geçerli bir sunucu adresi girin.";
                return View();
            }

            if (action == "add")
            {
                var existingMaintenancePoint = await _context.MaintenancePoints
                    .FirstOrDefaultAsync(mp => mp.MaintenancePointName == controlPoint && mp.ServerId == server.ServerId);
                if (existingMaintenancePoint != null)
                {
                    ViewBag.ErrorMessage = "Bu sunucuda aynı isimde bir kontrol noktası zaten mevcut.";
                    return View();
                }

                var maintenancePoint = new MaintenancePoint
                {
                    MaintenancePointName = controlPoint,
                    ServerId = server.ServerId,
                    IsActive = true
                };
                _context.MaintenancePoints.Add(maintenancePoint);
                try
                {
                    await _context.SaveChangesAsync();
                    ViewBag.SuccessAddMessage = "Kontrol noktası başarıyla eklendi.";
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Veritabanına kayıt sırasında bir hata oluştu: " + ex.Message;
                }
            }
            else if (action == "remove")
            {
                var maintenancePoint = await _context.MaintenancePoints
                    .FirstOrDefaultAsync(mp => mp.MaintenancePointName == controlPoint && mp.ServerId == server.ServerId);
                if (maintenancePoint != null)
                {
                    maintenancePoint.IsActive = false;
                    _context.MaintenancePoints.Update(maintenancePoint);
                    await _context.SaveChangesAsync();
                    ViewBag.SuccessRemoveMessage = "Kontrol noktası başarıyla pasif hale getirildi.";
                }
                else
                {
                    ViewBag.ErrorControlMessage = "Kontrol noktası bulunamadı.";
                }
            }

            var controls = await _context.MaintenancePoints
                .Include(mp => mp.Server)
                .Select(mp => new
                {
                    mp.MaintenancePointId,
                    mp.MaintenancePointName,
                    ServerName = mp.Server.ServerName,
                    ServerAddress = mp.Server.ServerAddress,
                    IsActive = mp.IsActive
                })
                .ToListAsync();

            ViewBag.Controls = controls;

            return View();
        }


        // Kontrolleri Listeleme 

        [HttpPost]
        public async Task<IActionResult> ListControls()
        {
            var controls = await _context.MaintenancePoints
                .Include(mp => mp.Server)
                .Select(mp => new
                {
                    mp.MaintenancePointId,
                    mp.MaintenancePointName,
                    ServerName = mp.Server.ServerName,
                    ServerAddress = mp.Server.ServerAddress,
                    IsActive = mp.IsActive
                })
                .ToListAsync();

            ViewBag.Controls = controls;
            return View("AddRemoveControl");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateControl(int maintenancePointId)
        {
            var maintenancePoint = await _context.MaintenancePoints.FirstOrDefaultAsync(mp => mp.MaintenancePointId == maintenancePointId);
            if (maintenancePoint != null)
            {
                maintenancePoint.IsActive = true;
                _context.MaintenancePoints.Update(maintenancePoint);
                await _context.SaveChangesAsync();
                ViewBag.ActivateMessage = "Kontrol noktası başarıyla aktif hale getirildi.";
            }
            else
            {
                ViewBag.ActivateMessage = "Kontrol noktası bulunamadı.";
            }

            var controls = await _context.MaintenancePoints
                .Include(mp => mp.Server)
                .Select(mp => new
                {
                    mp.MaintenancePointId,
                    mp.MaintenancePointName,
                    ServerName = mp.Server.ServerName,    
                    ServerAddress = mp.Server.ServerAddress, 
                    IsActive = mp.IsActive
                })
                .ToListAsync();

            ViewBag.Controls = controls;

            return View("AddRemoveControl");
        }




        // Bakım sayfası işlemleri

        [HttpGet]
        public async Task<IActionResult> Maintenance()
        {
            try
            {
                string? serverIpAddress = Dns.GetHostEntry(Dns.GetHostName())
                                            .AddressList
                                            .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                            ?.ToString();
                if (string.IsNullOrEmpty(serverIpAddress))
                {
                    Debug.WriteLine("Cihazın IP adresi alınamadı.");
                    return BadRequest("Cihazın IP adresi alınamadı.");
                }

                Debug.WriteLine("Cihazın IP adresi: " + serverIpAddress);

                var server = await _context.Servers
                    .Include(s => s.Responsible)
                    .FirstOrDefaultAsync(s => s.ServerAddress == serverIpAddress);
                if (server == null)
                {
                    ViewBag.ErrorMessage = "Sunucu bulunamadı.";
                    return View();
                }

                if (!server.IsActive)
                {
                    Debug.WriteLine("Sunucu aktif değil.");
                    ViewBag.ErrorMessage = "Sunucu aktif değil.";
                    return View();
                }

                Debug.WriteLine("Sunucu bulundu: " + server.ServerName);

                var maintenancePoints = await _context.MaintenancePoints
                    .Where(mp => mp.ServerId == server.ServerId && mp.IsActive)
                    .ToListAsync();

                var model = new MaintenanceViewModel
                {
                    Username = server.Responsible?.Username,
                    ServerAddress = server.ServerAddress,
                    ServerName = server.ServerName,
                    MaintenancePoints = maintenancePoints,
                    MaintenanceDate = DateTime.Now.Date 
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Hata: " + ex.Message);
                ViewBag.ErrorMessage = "Bir hata oluştu: " + ex.Message;
                return View();
            }
        }


        [HttpPost]
        public async Task<IActionResult> Maintenance(MaintenanceViewModel model, string action)
        {
            try
            {
                string loggedInUsername = HttpContext.Session.GetString("LoggedInUsername");
                if (string.IsNullOrEmpty(loggedInUsername))
                {
                    ViewBag.ErrorMessage = "Oturum bilgisi bulunamadı. Lütfen yeniden giriş yapın.";
                    return View(await LoadMaintenanceViewModel());
                }

                var server = await _context.Servers
                    .Include(s => s.Responsible)
                    .FirstOrDefaultAsync(s => s.ServerAddress == model.ServerAddress);

                if (server == null)
                {
                    ViewBag.ErrorMessage = "Sunucu bulunamadı.";
                    return View(await LoadMaintenanceViewModel());
                }

                if (server.Responsible == null || server.Responsible.Username != loggedInUsername)
                {
                    ViewBag.ErrorMessage = "Yetkisiz erişim: Kullanıcı, bu sunucu için yetkili değil.";
                    return View(await LoadMaintenanceViewModel());
                }

                
                if (action == "not_complete" && string.IsNullOrWhiteSpace(model.NotCompletedReason))
                {
                    ViewBag.ErrorMessage = "Lütfen bakımın tamamlanmama nedenini belirtin.";
                    return View(await LoadMaintenanceViewModel());
                }

                var maintenancePoint = await _context.MaintenancePoints
                    .FirstOrDefaultAsync(mp => mp.MaintenancePointId == model.SelectedPointId && mp.ServerId == server.ServerId);

                if (maintenancePoint == null)
                {
                    ViewBag.ErrorMessage = "Bakım noktası bulunamadı.";
                    return View(await LoadMaintenanceViewModel());
                }

                DateTime maintenanceDate = model.MaintenanceDate;

                var existingLog = await _context.MaintenanceLogs
                    .FirstOrDefaultAsync(ml => ml.ServerId == server.ServerId && ml.MaintenancePointId == model.SelectedPointId && ml.LogDate.Date == maintenanceDate.Date);

                if (existingLog != null)
                {
                    existingLog.IsCompleted = action == "complete";
                    if (action == "not_complete")
                    {
                        existingLog.NotCompletedReason = model.NotCompletedReason;
                    }
                    else
                    {
                        existingLog.NotCompletedReason = string.Empty; 
                    }
                    _context.Update(existingLog);
                }
                else
                {
                    var maintenanceLog = new MaintenanceLog
                    {
                        ServerId = server.ServerId,
                        MaintenancePointId = maintenancePoint.MaintenancePointId,
                        LogDate = maintenanceDate,
                        IsCompleted = action == "complete",
                        NotCompletedReason = action == "not_complete" ? model.NotCompletedReason : string.Empty // Boş string olarak ayarla
                    };
                    _context.MaintenanceLogs.Add(maintenanceLog);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("MainPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Hata: " + ex.Message);
                ViewBag.ErrorMessage = "Bir hata oluştu: " + ex.Message;
                return View(await LoadMaintenanceViewModel());
            }
        }


        //HATALARI GÖSTERMEK İÇİN KULLANILAN BİR YAPI
        private async Task<MaintenanceViewModel> LoadMaintenanceViewModel()
        {
            try
            {
                string? serverIpAddress = Dns.GetHostEntry(Dns.GetHostName())
                                           .AddressList
                                           .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                           ?.ToString();
                if (string.IsNullOrEmpty(serverIpAddress))
                {
                    Debug.WriteLine("Cihazın IP adresi alınamadı.");
                    return new MaintenanceViewModel();
                }

                var server = await _context.Servers
                    .Include(s => s.Responsible)
                    .FirstOrDefaultAsync(s => s.ServerAddress == serverIpAddress);

                if (server == null)
                {
                    return new MaintenanceViewModel();
                }

                var maintenancePoints = await _context.MaintenancePoints
                    .Where(mp => mp.ServerId == server.ServerId && mp.IsActive)
                    .ToListAsync();

                return new MaintenanceViewModel
                {
                    Username = server.Responsible?.Username,
                    ServerAddress = server.ServerAddress,
                    ServerName = server.ServerName,
                    MaintenancePoints = maintenancePoints,
                    MaintenanceDate = DateTime.Now.Date
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Hata: " + ex.Message);
                return new MaintenanceViewModel();
            }
        }


        // Hata sayfası işlemleri
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Profil Sayfası 

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            string loggedInUsername = HttpContext.Session.GetString("LoggedInUsername");
            if (string.IsNullOrEmpty(loggedInUsername))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loggedInUsername);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new CombinedViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return View(model);
        }

        // Şifre Güncelleme İşlemleri

        [HttpPost]
        public async Task<IActionResult> ChangePassword(CombinedViewModel model)
        {
            string loggedInUsername = HttpContext.Session.GetString("LoggedInUsername");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (user == null || user.Password != model.CurrentPassword)
            {
                ModelState.AddModelError(string.Empty, "Mevcut şifre yanlış.");
                model.Username = user.Username;
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                return View("Profile", model); 
            }

            if (model.NewPassword == model.CurrentPassword)
            {
                ModelState.AddModelError(string.Empty, "Yeni şifre mevcut şifre ile aynı olamaz.");
                model.Username = user.Username;
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                return View("Profile", model); 
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Yeni şifre ve şifre tekrarı uyuşmuyor.");
                model.Username = user.Username;
                model.FirstName = user.FirstName;
                model.LastName = user.FirstName;
                return View("Profile", model); 
            }

            user.Password = model.NewPassword;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Şifre başarıyla değiştirildi.";

            
            model.Username = user.Username;
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;

            return View("Profile", model);
        }
    }
}