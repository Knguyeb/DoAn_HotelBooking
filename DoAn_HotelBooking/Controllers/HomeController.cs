using System.Diagnostics;
using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoAn_HotelBooking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DoAn_HotelBookingContext _context;

        public HomeController(ILogger<HomeController> logger, DoAn_HotelBookingContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(string? search, int page = 1)
        {
            int pageSize = 8;
            var query = _context.KhachSan.AsQueryable();

            // 1. LỌC DỮ LIỆU: Nếu người dùng nhấn nút "Tìm kiếm" hoặc Enter
            if (!string.IsNullOrEmpty(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(k =>
                    k.TenKhachSan.ToLower().Contains(searchLower) ||
                    k.DiaChi.ToLower().Contains(searchLower));
            }

            // 2. PHÂN TRANG: Áp dụng cho danh sách hiển thị chính thức
            int totalItems = query.Count();
            int totalPages = totalItems > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 1;

            var pagedList = query
                .OrderBy(k => k.TenKhachSan)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // =========================================================
            // 3. TÍNH ĐIỂM TRUNG BÌNH CHO TỪNG KHÁCH SẠN TRÊN TRANG
            // =========================================================
            foreach (var ks in pagedList)
            {
                // Lấy danh sách đánh giá của riêng khách sạn này
                var danhGias = _context.DanhGiaKS
                    .Where(d => d.MaKhachSan == ks.MaKhachSan)
                    .ToList();

                if (danhGias.Any())
                {
                    // Tính trung bình cộng và làm tròn 1 chữ số thập phân (vd: 4.5)
                    ks.TrungBinhSao = Math.Round(danhGias.Average(d => (double)d.SoSao), 1);
                }
                else
                {
                    ks.TrungBinhSao = 0;
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            // Trả về View Index mặc định
            return View(pagedList);
        }

        [HttpGet]
        public IActionResult SearchSuggestions(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return Json(new List<object>());
            }

            string searchLower = search.ToLower();

            // Lấy tối đa 5 kết quả gợi ý tốt nhất
            var results = _context.KhachSan
                .Where(k => k.TenKhachSan.ToLower().Contains(searchLower) || k.DiaChi.ToLower().Contains(searchLower))
                .Take(5)
                .Select(k => new {
                    id = k.MaKhachSan,
                    name = k.TenKhachSan,
                    address = k.DiaChi,
                    // Xử lý lấy ảnh đầu tiên an toàn
                    image = string.IsNullOrEmpty(k.HinhAnh) ? "/Images/no-image.png" :
                            (k.HinhAnh.Split(';', StringSplitOptions.RemoveEmptyEntries)[0].Trim().StartsWith("/") ?
                             k.HinhAnh.Split(';', StringSplitOptions.RemoveEmptyEntries)[0].Trim() :
                             "/Images/" + k.HinhAnh.Split(';', StringSplitOptions.RemoveEmptyEntries)[0].Trim())
                })
                .ToList();

            return Json(results);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
