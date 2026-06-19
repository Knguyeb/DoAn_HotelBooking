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
        public virtual Phong Phong { get; set; }

        [Required]
        [ForeignKey("TaiKhoan")]
        public int MaTaiKhoan { get; set; }
        public virtual TaiKhoan TaiKhoan { get; set; }

        // Để kiểm tra mỗi đơn chỉ đánh giá 1 lần
        [Required]
        [ForeignKey("DatPhong")]
        public int MaDatPhong { get; set; }
        public virtual DatPhong DatPhong { get; set; }

        [Required]
        [Range(1, 5)]
        public int SoSao { get; set; }

        [StringLength(1000)]
        public string? NoiDung { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        public DateTime? NgayCapNhat { get; set; }
    }
}