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

        [NotMapped] // Thuộc tính ảo, không tạo cột trong Database
        [Display(Name = "TRẠNG THÁI HÔM NAY")]
        public string TrangThaiHomNay
        {
            get
            {
                // 1. Ưu tiên cao nhất: Nếu phòng đang hỏng thì báo bảo trì
                if (TrangThai == "Bảo trì")
                    return "Bảo trì";

                // 2. Ưu tiên hai: Lễ tân đã bấm Check-in cho khách
                if (TrangThai == "Đang sử dụng")
                    return "Đang sử dụng";

                // 3. Nếu phòng trống, kiểm tra xem hôm nay có ai đặt lịch không
                if (DatPhongs != null && DatPhongs.Any())
                {
                    DateTime homNay = DateTime.UtcNow.Date;

                    bool coKhachDatHomNay = DatPhongs.Any(dp =>
                        (dp.TrangThaiDatPhong == "Chờ xác nhận" || dp.TrangThaiDatPhong == "Đã xác nhận") &&
                        homNay >= dp.NgayNhanPhong.Date &&
                        homNay < dp.NgayTraPhong.Date // Dùng dấu < vì ngày trả phòng (Checkout) thì phòng sẽ trống vào buổi chiều
                    );

                    if (coKhachDatHomNay)
                    {
                        return "Đã đặt hôm nay"; // Lịch báo hôm nay có khách sẽ đến
                    }
                }

                // 4. Nếu không rơi vào các trường hợp trên thì trả về trạng thái DB gốc (Còn trống)
                return TrangThai;
            }

        }
    }
}
