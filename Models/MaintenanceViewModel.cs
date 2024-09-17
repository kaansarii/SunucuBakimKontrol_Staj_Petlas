namespace SunucuBakimKontrol.Models
{
    public class MaintenanceViewModel
    {
        public string Username { get; set; }
        public string ServerAddress { get; set; }
        public string ServerName { get; set; }
        public List<int> SelectedPoints { get; set; }
        public string NotCompletedReason { get; set; }
        public List<MaintenancePoint> MaintenancePoints { get; set; }
        public int SelectedPointId { get; set; }
        public DateTime MaintenanceDate { get; set; }  // BAKIM TİK ÇARPI DEĞİŞİMİ İÇİN
    }
} 