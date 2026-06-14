using System.ComponentModel.DataAnnotations;

namespace DoAn_HotelBooking.Models
{
    public class HangThanhVien
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Tên hạng không được để trống")]
        [Display(Name = "TÊN HẠNG")]
        public string TenHang { get; set; } // Ví dụ: Bạc, Vàng, Kim Cương

        [Required(ErrorMessage = "Mốc điểm tối thiểu không được để trống")]
        [Display(Name = "MỐC ĐIỂM TỐI THIỂU")]
        public int MocDiemToiThieu { get; set; } // Ví dụ: 0, 100, 500

        [Required(ErrorMessage = "Tỷ lệ giảm giá không được để trống")]
        [Display(Name = "TỶ LỆ GIẢM GIÁ (%)")]
        [Range(0, 100, ErrorMessage = "Tỷ lệ giảm giá từ 0 đến 100%")]
        public double TyLeGiamGia { get; set; } // Ví dụ: 5.0 (cho 5%), 10.0 (cho 10%)

        // Navigation property: Một hạng có thể có nhiều tài khoản
        public ICollection<TaiKhoan>? TaiKhoans { get; set; }
    }
}