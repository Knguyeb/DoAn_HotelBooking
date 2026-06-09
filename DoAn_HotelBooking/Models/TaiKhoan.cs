using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_HotelBooking.Models
{
    [Index(nameof(TenDangNhap), IsUnique = true)] // Tên đăng nhập không được trùng

    public class TaiKhoan
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "HỌ TÊN KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "HỌ TÊN")]
        public string HoVaTen { get; set; }

        [Required(ErrorMessage = "TÊN ĐĂNG NHẬP KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "TÊN ĐĂNG NHẬP")]
        public string TenDangNhap { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "MẬT KHẨU")]
        public string? MatKhau { get; set; }

        [Required(ErrorMessage = "EMAIL KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [EmailAddress(ErrorMessage = "EMAIL KHÔNG HỢP LỆ")]
        [Display(Name = "EMAIL")]
        public string Email { get; set; }

        [Required(ErrorMessage = "SĐT KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Phone(ErrorMessage = "SĐT KHÔNG HỢP LỆ")]
        [Display(Name = "SỐ ĐIỆN THOẠI")]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "QUYỀN HẠN KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "QUYỀN HẠN")]
        public string QuyenHan { get; set; } = "Khách hàng"; // Mặc định là Khách Hàng

        [Display(Name = "KHÁCH SẠN")]
        // Liên kết với khách sạn
        [ForeignKey("KhachSan")]
        public string? MaKhachSan { get; set; }
        public KhachSan? KhachSan { get; set; }
    }
}
