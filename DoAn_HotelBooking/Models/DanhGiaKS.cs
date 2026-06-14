using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_HotelBooking.Models
{
    public class DanhGiaKS
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [ForeignKey("KhachSan")]
        public string MaKhachSan { get; set; }
        public KhachSan KhachSan { get; set; }

        [Required]
        [ForeignKey("TaiKhoan")]
        public int MaTaiKhoan { get; set; }
        public TaiKhoan TaiKhoan { get; set; }

        [Range(1, 5)]
        public int SoSao { get; set; }
    }
}