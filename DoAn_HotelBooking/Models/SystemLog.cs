using System.ComponentModel.DataAnnotations;

namespace DoAn_HotelBooking.Models
{
    public class SystemLog
    {
        [Key]
        public int Id { get; set; }

        public string Message { get; set; }

        [MaxLength(50)]
        public string Level { get; set; }

        public DateTime Timestamp { get; set; }

        public string Exception { get; set; }
        public bool DaXuLy { get; set; } = false;
    }
}
