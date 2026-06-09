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
            if (model == null || string.IsNullOrEmpty(model.MaKhachSan) || model.SoSao < 1 || model.SoSao > 5)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

            // ✅ Lấy ID tài khoản theo session bạn đã lưu
            var maTaiKhoan = HttpContext.Session.GetInt32("ID");

            // ✅ Gán dữ liệu còn thiếu
            model.MaTaiKhoan = maTaiKhoan.Value;

            _context.Add(model);

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá khách sạn!" });
        }
    }
}
