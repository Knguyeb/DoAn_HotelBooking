using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn_HotelBooking.Controllers
{
    public class DanhGiaKSsController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;

        public DanhGiaKSsController(DoAn_HotelBookingContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFromSwal([FromBody] DanhGiaKS model)
        {
            // Trả về errorMessage thay vì message
            if (model == null || string.IsNullOrEmpty(model.MaKhachSan) || model.SoSao < 1 || model.SoSao > 5)
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ!" });

            // ✅ 1. Kiểm tra đăng nhập (Bảo vệ lỗi null reference)
            var maTaiKhoan = HttpContext.Session.GetInt32("ID");
            if (maTaiKhoan == null)
            {
                return Json(new { success = false, errorMessage = "Vui lòng đăng nhập để thực hiện đánh giá!" });
            }

            // ✅ 2. KIỂM TRA ĐÁNH GIÁ TRÙNG LẶP
            bool daDanhGia = await _context.DanhGiaKS.AnyAsync(d =>
                d.MaKhachSan == model.MaKhachSan &&
                d.MaTaiKhoan == maTaiKhoan.Value);

            if (daDanhGia)
            {
                // Trả về errorMessage để frontend dùng Swal hiển thị
                return Json(new { success = false, errorMessage = "Bạn đã đánh giá khách sạn này rồi! Mỗi khách hàng chỉ được đánh giá 1 lần." });
            }

            // ✅ 3. Gán dữ liệu còn thiếu và lưu vào DB
            model.MaTaiKhoan = maTaiKhoan.Value;

            _context.Add(model);
            await _context.SaveChangesAsync();

            // Trường hợp thành công thì thường Swal bên JS của bạn vẫn dùng key 'message' đúng không? Mình giữ nguyên nhé.
            return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá khách sạn!" });
        }

        [HttpGet]
        public async Task<IActionResult> KiemTraDanhGia(string maKhachSan)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("ID");

            // 1. Kiểm tra đăng nhập
            if (maTaiKhoan == null)
                return Json(new { hopLe = false, message = "Vui lòng đăng nhập để đánh giá!" });

            // 2. Kiểm tra xem đã đánh giá chưa
            bool daDanhGia = await _context.DanhGiaKS.AnyAsync(d =>
                d.MaKhachSan == maKhachSan &&
                d.MaTaiKhoan == maTaiKhoan.Value);

            if (daDanhGia)
                return Json(new { hopLe = false, message = "Bạn đã đánh giá khách sạn này rồi! Mỗi khách hàng chỉ được đánh giá 1 lần." });

            // 3. Nếu chưa đánh giá, cho phép tiếp tục
            return Json(new { hopLe = true });
        }
    }
}