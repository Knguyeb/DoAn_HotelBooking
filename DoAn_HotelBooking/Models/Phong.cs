using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_HotelBooking.Models
{
    public class Phong
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "SỐ PHÒNG KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "SỐ PHÒNG")]
        public int SoPhong { get; set; }

        [Required(ErrorMessage = "TẦNG KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "TẦNG")]
        public int Tang { get; set; }

        [Required(ErrorMessage = "LOẠI PHÒNG KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [StringLength(100)]
        [Display(Name = "LOẠI PHÒNG")]
        public string LoaiPhong { get; set; }

        [Required(ErrorMessage = "SỨC CHỨA KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "SỨC CHỨA (người)")]
        public int SucChua { get; set; }

        [Required(ErrorMessage = "GIÁ PHÒNG KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "GIÁ PHÒNG")]
        public decimal GiaPhong { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "TRẠNG THÁI")]
        public string TrangThai { get; set; } = "Còn trống"; // Còn trống, Đang thuê, Bảo trì

        [Required(ErrorMessage = "HÌNH KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "HÌNH ẢNH")]
        public string HinhAnh { get; set; }

        [Display(Name = "KHÁCH SẠN")]
        // Liên kết với khách sạn
        [ForeignKey("KhachSan")]
        public string? MaKhachSan { get; set; }
        public KhachSan? KhachSan { get; set; }

        [NotMapped]
        public double TrungBinhSao { get; set; }

        // Thuộc tính để Include danh sách đánh giá từ bảng DanhGiaPhong
        public virtual ICollection<DanhGiaPhong> DanhGiaPhongs { get; set; }

        public virtual ICollection<DatPhong> DatPhongs { get; set; }

    }
}
