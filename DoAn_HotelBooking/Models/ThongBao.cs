using System.ComponentModel.DataAnnotations;

namespace DoAn_HotelBooking.Models
{
    public class ThongBao
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string TieuDe { get; set; }

        [Required]
        public string NoiDung { get; set; }

        public string Loai { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        public bool DaDoc { get; set; } = false;

        public string MaKhachSan { get; set; }
    }
}