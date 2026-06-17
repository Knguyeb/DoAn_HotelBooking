using DoAn_HotelBooking.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn_HotelBooking.Controllers
{
    // Mở comment dòng dưới đây nếu bạn đã thiết lập xong phân quyền Admin
    // [Authorize(Roles = "Admin")]
    public class SystemLogsController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;

        public SystemLogsController(DoAn_HotelBookingContext context)
        {
            _context = context;
        }

        // 1. Giao diện trang chính (Hiển thị toàn bộ lỗi)
        public async Task<IActionResult> Index()
        {
            var logs = await _context.SystemLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return View(logs);
        }

        // 2. Hàm AJAX để đổi trạng thái "Đã xử lý" / "Chưa xử lý"
        [HttpPost]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var log = await _context.SystemLogs.FindAsync(id);
            if (log == null)
            {
                return Json(new { success = false, message = "Không tìm thấy log này!" });
            }

            // Đảo ngược trạng thái
            log.DaXuLy = !log.DaXuLy;

            _context.Update(log);
            await _context.SaveChangesAsync();

            return Json(new { success = true, trangThaiMoi = log.DaXuLy });
        }

        // 3. API dành riêng cho Popup (Nút Con Bọ) trên thanh Navbar
        [HttpGet]
        public async Task<IActionResult> GetRecentLogs()
        {
            var logs = await _context.SystemLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(5) // Chỉ lấy 5 lỗi mới nhất để Load siêu nhanh
                .Select(l => new {
                    id = l.Id,
                    message = l.Message,
                    level = l.Level,
                    timestamp = l.Timestamp.ToString("dd/MM/yyyy HH:mm"),
                    daXuLy = l.DaXuLy
                })
                .ToListAsync();

            return Json(logs);
        }
    }
}