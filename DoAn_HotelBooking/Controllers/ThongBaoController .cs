using DoAn_HotelBooking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn_HotelBooking.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;

        public ThongBaoController(DoAn_HotelBookingContext context)
        {
            _context = context;
        }

        // GET: ThongBao/Index (Trang hiển thị tất cả thông báo)
        public async Task<IActionResult> Index()
        {
            var maKS = HttpContext.Session.GetString("MaKhachSan");
            var quyen = HttpContext.Session.GetString("QuyenHan");

            // Lấy toàn bộ thông báo từ Database
            IQueryable<Models.ThongBao> query = _context.ThongBao;

            // Phân quyền dữ liệu (Row-level Security)
            if (quyen != "Admin")
            {
                // Nhân viên/Quản lý chỉ được xem thông báo của Khách sạn mình
                if (string.IsNullOrEmpty(maKS))
                {
                    TempData["ErrorMessage"] = "⚠️ Khách hàng không có quyền xem trang này.";
                    return RedirectToAction("Index", "Home");
                }
                query = query.Where(t => t.MaKhachSan == maKS);
            }

            // Sắp xếp thông báo mới nhất lên đầu tiên
            var danhSachThongBao = await query
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();

            return View(danhSachThongBao);
        }


        [HttpGet]
        public IActionResult GetThongBao()
        {
            var maKS = HttpContext.Session.GetString("MaKhachSan");

            var data = _context.ThongBao
                .Where(x => x.MaKhachSan == maKS)
                .OrderByDescending(x => x.NgayTao)
                .Take(10)
                .Select(x => new
                {
                    noiDung = x.NoiDung,
                    ngayTao = x.NgayTao.ToString("dd/MM/yyyy HH:mm")
                })
                .ToList();

            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> DanhDauDaDocToanBo()
        {
            var maKS = HttpContext.Session.GetString("MaKhachSan");
            var quyen = HttpContext.Session.GetString("QuyenHan");

            // Chỉ tìm những thông báo CHƯA ĐỌC
            IQueryable<Models.ThongBao> query = _context.ThongBao.Where(t => t.DaDoc == false);

            if (quyen != "Admin")
            {
                if (string.IsNullOrEmpty(maKS))
                    return Json(new { success = false });

                // Ép theo khách sạn
                query = query.Where(t => t.MaKhachSan == maKS);
            }

            var danhSachChuaDoc = await query.ToListAsync();

            if (danhSachChuaDoc.Any())
            {
                foreach (var item in danhSachChuaDoc)
                {
                    item.DaDoc = true; // Chuyển thành đã đọc
                }
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }
    }
}
