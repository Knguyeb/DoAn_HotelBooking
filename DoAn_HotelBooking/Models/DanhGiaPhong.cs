using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_HotelBooking.Models
{
    public class DanhGiaPhong
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [ForeignKey("Phong")]
        public int MaPhong { get; set; }
        public Phong Phong { get; set; }

        [Required]
        [ForeignKey("TaiKhoan")]
        public int MaTaiKhoan { get; set; }
        public TaiKhoan TaiKhoan { get; set; }

        [Range(1, 5)]
        public int SoSao { get; set; }
    }
}