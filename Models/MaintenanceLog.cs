using System.ComponentModel.DataAnnotations;

namespace SunucuBakimKontrol.Models
{
    public class MaintenanceLog
    {
        [Key]
        public int LogId { get; set; }
        public int ServerId { get; set; }
        public int MaintenancePointId { get; set; }
        public DateTime LogDate { get; set; }
        public bool IsCompleted { get; set; }
        public string NotCompletedReason { get; set; }

        public Server Server { get; set; }
        public MaintenancePoint MaintenancePoint { get; set; }
    }
}