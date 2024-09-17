using System.ComponentModel.DataAnnotations;

namespace SunucuBakimKontrol.Models
{
    public class Server
    {
        [Key]
        public int ServerId { get; set; }
        public string ServerAddress { get; set; }
        public string ServerName { get; set; }
        public int ResponsibleId { get; set; }
        public bool IsActive { get; set; } 
        public User Responsible { get; set; }
        public ICollection<MaintenancePoint> MaintenancePoints { get; set; }
        public ICollection<MaintenanceLog> MaintenanceLogs { get; set; }
    }
}