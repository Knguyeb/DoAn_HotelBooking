using DoAn_HotelBooking.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_HotelBooking.Models
{
    public class KhachSan
    {
        [Key]
        [Display(Name = "MÃ KHÁCH SẠN")]
        public string MaKhachSan { get; set; }

        [Required(ErrorMessage = "TÊN KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "TÊN KHÁCH SẠN")]
        public string TenKhachSan { get; set; }

        [Required(ErrorMessage = "ĐỊA CHỈ KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "ĐỊA CHỈ")]
        public string DiaChi { get; set; }

        [Display(Name = "VĨ ĐỘ")]
        public string? ViDo { get; set; }

        [Display(Name = "KINH ĐỘ")]
        public string? KinhDo { get; set; }

        [Required(ErrorMessage = "SĐT KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [RegularExpression(@"^\d{9,11}$", ErrorMessage = "SĐT phải từ 9-11 số")]
        [Display(Name = "SỐ ĐIỆN THOẠI")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "HÌNH KHÔNG ĐƯỢC ĐỂ TRỐNG")]
        [Display(Name = "HÌNH ẢNH")]
        public string HinhAnh { get; set; }

        [ValidateNever]
        public ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();

        [NotMapped]
        public double TrungBinhSao { get; set; }

        // Thuộc tính để Include danh sách đánh giá từ bảng DanhGiaPhong
        public virtual ICollection<DanhGiaKS> DanhGiaKhachSans { get; set; }

        public virtual ICollection<Phong> Phongs { get; set; }


        private static readonly Random random = new Random();

        // Hàm sinh mã khách sạn ngẫu nhiên
        public static string GenerateMaKhachSan()
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            // 3 chữ cái ngẫu nhiên
            string chars = new string(Enumerable.Range(0, 3)
                .Select(_ => letters[random.Next(letters.Length)]).ToArray());

            // 3 số ngẫu nhiên
            string numbers = random.Next(0, 1000).ToString("D3");

            return chars + numbers;
        }
    }
}
