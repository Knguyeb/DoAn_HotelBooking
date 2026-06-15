using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn_HotelBooking.Controllers
{
    public class DanhGiaPhongsController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;

        public DanhGiaPhongsController(DoAn_HotelBookingContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFromSwal([FromBody] DanhGiaPhong model)
        {
            // ✅ Kiểm tra dữ liệu đầu vào (Đổi key thành errorMessage)
            if (model == null || model.MaPhong <= 0 || model.SoSao < 1 || model.SoSao > 5)
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ!" });

            // ✅ 1. Kiểm tra đăng nhập (Bảo vệ ứng dụng không bị văng lỗi khi Session null)
            var maTaiKhoan = HttpContext.Session.GetInt32("ID");
            if (maTaiKhoan == null)
            {
                return Json(new { success = false, errorMessage = "Vui lòng đăng nhập để thực hiện đánh giá!" });
            }

            // ✅ 2. KIỂM TRA ĐÁNH GIÁ TRÙNG LẶP
            // Quét xem tài khoản này đã từng đánh giá phòng cụ thể này chưa
            bool daDanhGia = await _context.DanhGiaPhong.AnyAsync(d =>
                d.MaPhong == model.MaPhong &&
                d.MaTaiKhoan == maTaiKhoan.Value);

            if (daDanhGia)
            {
                return Json(new { success = false, errorMessage = "Bạn đã đánh giá phòng này rồi! Mỗi khách hàng chỉ được đánh giá 1 lần." });
            }

            // ✅ 3. Gán dữ liệu còn thiếu và lưu vào DB
            model.MaTaiKhoan = maTaiKhoan.Value;

            _context.Add(model);
            await _context.SaveChangesAsync();

            // Trả về message khi thành công để Swal hiển thị icon success
            return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá phòng!" });
        }

        [HttpGet]
        public async Task<IActionResult> KiemTraDanhGia(int maPhong)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("ID");

            // 1. Kiểm tra đăng nhập
            if (maTaiKhoan == null)
                return Json(new { hopLe = false, message = "Vui lòng đăng nhập để đánh giá phòng này!" });

            // 2. Kiểm tra xem đã đánh giá chưa
            bool daDanhGia = await _context.DanhGiaPhong.AnyAsync(d =>
                d.MaPhong == maPhong &&
                d.MaTaiKhoan == maTaiKhoan.Value);

            if (daDanhGia)
                return Json(new { hopLe = false, message = "Bạn đã đánh giá phòng này rồi! Mỗi khách hàng chỉ được đánh giá 1 lần." });

            // 3. Nếu chưa đánh giá, cho phép mở form 5 sao
            return Json(new { hopLe = true });
        }
    }
}