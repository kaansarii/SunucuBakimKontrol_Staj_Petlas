namespace SunucuBakimKontrol.ViewModels
{
    public class TaskViewModel
    {
        public string ServerName { get; set; }
        public string ServerIpAddress { get; set; }
        public string ControlPointName { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}