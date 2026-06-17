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

        // 1. Giao diện trang chính (Đã tích hợp Bộ lọc & Thống kê 7 ngày)
        public async Task<IActionResult> Index(string level, DateTime? startDate, DateTime? endDate)
        {
            var availableLevels = await _context.SystemLogs
            .Select(l => l.Level)
            .Distinct()
            .Where(l => !string.IsNullOrEmpty(l))
            .ToListAsync();

            ViewBag.AvailableLevels = availableLevels;

            var query = _context.SystemLogs.AsQueryable();

            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(l => l.Level.ToLower().Contains(level.ToLower()));
                ViewBag.CurrentLevel = level;
            }

            // Lọc theo Từ ngày
            if (startDate.HasValue)
            {
                var utcStart = startDate.Value.ToUniversalTime();
                query = query.Where(l => l.Timestamp >= utcStart);
                ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            }

            // Lọc theo Đến ngày
            if (endDate.HasValue)
            {
                var utcEnd = endDate.Value.AddDays(1).ToUniversalTime(); // Lấy hết ngày được chọn
                query = query.Where(l => l.Timestamp < utcEnd);
                ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            }

            var logs = await query.OrderByDescending(l => l.Timestamp).ToListAsync();

            // 🌟 XỬ LÝ DỮ LIỆU CHO BIỂU ĐỒ CHART.JS (7 Ngày qua)
            // Lấy danh sách 7 ngày gần nhất theo giờ Việt Nam (UTC+7)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.UtcNow.AddHours(7).Date.AddDays(-i))
                .Reverse()
                .ToList();

            var chartLabels = last7Days.Select(d => d.ToString("dd/MM")).ToList();
            var chartData = new List<int>();

            // Lấy log 7 ngày để đếm
            var date7DaysAgo = DateTime.UtcNow.AddDays(-7);
            var allLogs7Days = await _context.SystemLogs
                .Where(l => l.Timestamp >= date7DaysAgo)
                .ToListAsync();

            foreach (var day in last7Days)
            {
                // Chuyển thời gian DB sang giờ VN rồi đếm
                var count = allLogs7Days.Count(l => l.Timestamp.AddHours(7).Date == day);
                chartData.Add(count);
            }

            ViewBag.ChartLabels = System.Text.Json.JsonSerializer.Serialize(chartLabels);
            ViewBag.ChartData = System.Text.Json.JsonSerializer.Serialize(chartData);

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