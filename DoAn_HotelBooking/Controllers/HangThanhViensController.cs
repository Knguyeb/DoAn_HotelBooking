using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Models;

namespace DoAn_HotelBooking.Controllers
{
    public class HangThanhViensController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;

        public HangThanhViensController(DoAn_HotelBookingContext context)
        {
            _context = context;
        }

        // GET: HangThanhViens
        public async Task<IActionResult> Index()
        {
            // Sắp xếp tăng dần theo mốc điểm tối thiểu
            var danhSachHang = await _context.HangThanhVien
                .OrderBy(h => h.MocDiemToiThieu)
                .ToListAsync();

            return View(danhSachHang);
        }

        // ================= XỬ LÝ AJAX (THÊM & SỬA TRỰC TIẾP TRÊN LƯỚI) =================

        // POST: HangThanhViens/CreateAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax([FromBody] HangThanhVien model)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
                return Json(new { success = false, message = errorMessage ?? "Vui lòng nhập đầy đủ thông tin!" });
            }

            bool isDuplicateName = await _context.HangThanhVien.AnyAsync(h => h.TenHang.ToLower() == model.TenHang.ToLower());
            if (isDuplicateName)
            {
                return Json(new { success = false, message = $"Tên hạng '{model.TenHang}' đã tồn tại!" });
            }

            try
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Thêm hạng thành viên thành công!", id = model.ID });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // POST: HangThanhViens/EditAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAjax(int id, [FromBody] HangThanhVien model)
        {
            if (id != model.ID)
            {
                return Json(new { success = false, message = "Dữ liệu không khớp. Vui lòng tải lại trang!" });
            }

            if (!ModelState.IsValid)
            {
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
                return Json(new { success = false, message = errorMessage ?? "Vui lòng nhập đầy đủ thông tin!" });
            }

            // Kiểm tra trùng tên nhưng bỏ qua chính nó
            bool isDuplicateName = await _context.HangThanhVien.AnyAsync(h => h.TenHang.ToLower() == model.TenHang.ToLower() && h.ID != id);
            if (isDuplicateName)
            {
                return Json(new { success = false, message = $"Tên hạng '{model.TenHang}' đã tồn tại!" });
            }

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật hạng thành viên thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ================= XỬ LÝ TRUYỀN THỐNG (XÓA BẰNG FORM) =================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hangThanhVien = await _context.HangThanhVien.FindAsync(id);
            if (hangThanhVien != null)
            {
                _context.HangThanhVien.Remove(hangThanhVien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa hạng thành viên thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HangThanhVienExists(int id)
        {
            return _context.HangThanhVien.Any(e => e.ID == id);
        }
    }
}