using System.ComponentModel.DataAnnotations;

namespace SunucuBakimKontrol.Models
{
    public class MaintenancePoint
    {
        [Key]
        public int MaintenancePointId { get; set; }
        public int ServerId { get; set; }
        public string MaintenancePointName { get; set; }
        public bool IsActive { get; set; }
        public Server Server { get; set; }
        
        public ICollection<MaintenanceLog> MaintenanceLogs { get; set; }  
    }
}