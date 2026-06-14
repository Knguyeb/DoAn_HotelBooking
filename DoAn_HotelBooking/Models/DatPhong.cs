using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_HotelBooking.Models
{
    public class DatPhong
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "NGÀY TẠO")]
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "NGÀY NHẬN PHÒNG KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "NGÀY NHẬN PHÒNG")]
        [DataType(DataType.Date)]
        public DateTime NgayNhanPhong { get; set; }

        [Required(ErrorMessage = "NGÀY TRẢ PHÒNG KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "NGÀY TRẢ PHÒNG")]
        [DataType(DataType.Date)]
        public DateTime NgayTraPhong { get; set; }

        [Required(ErrorMessage = "SỐ NGƯỜI KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "SỐ NGƯỜI")]
        [Range(1, 20, ErrorMessage = "Số người phải từ 1 đến 20")]
        public int SoNguoi { get; set; }

        [Display(Name = "TỔNG TIỀN")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        [Display(Name = "TIỀN GIẢM")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TienGiam { get; set; }

        [Required]
        [Display(Name = "TRẠNG THÁI ĐẶT PHÒNG")]
        public string TrangThaiDatPhong { get; set; } = "Chờ xác nhận";

        public string TrangThaiThanhToan { get; set; } = "Chưa thanh toán";

        [Display(Name = "GHI CHÚ")]
        public string? GhiChu { get; set; }

        // ===== KHÓA NGOẠI KHÁCH HÀNG =====
        [Required(ErrorMessage = "KHÁCH HÀNG KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "KHÁCH HÀNG")]
        public int MaTaiKhoan { get; set; }

        [ForeignKey(nameof(MaTaiKhoan))]
        public TaiKhoan? TaiKhoan { get; set; }

        // ===== KHÓA NGOẠI PHÒNG =====
        [Required(ErrorMessage = "PHÒNG KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "PHÒNG")]
        public int MaPhong { get; set; }

        [ForeignKey(nameof(MaPhong))]
        public Phong? Phong { get; set; }
    }
}