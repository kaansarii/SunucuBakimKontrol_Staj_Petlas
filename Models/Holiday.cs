using System.ComponentModel.DataAnnotations;

namespace SunucuBakimKontrol.Models
{
    public class Holiday
    {
        [Key]
        public int HolidayId { get; set; }
        public DateTime Date { get; set; }
        public bool IsHoliday { get; set; }
        
    }
}
