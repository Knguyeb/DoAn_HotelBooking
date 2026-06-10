using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DoAn_HotelBooking.Controllers
{
    public class DatPhongsController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;
        private readonly IConfiguration _configuration;

        public DatPhongsController(DoAn_HotelBookingContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: DatPhongs
        public async Task<IActionResult> Index()
        {
            // Lấy quyền & thông tin người dùng từ Session
            string quyen = HttpContext.Session.GetString("QuyenHan");
            string maKhachSan = HttpContext.Session.GetString("MaKhachSan");

            // Nếu là Khách hàng, không cho phép truy cập trang quản lý này
            // Chuyển hướng sang Action dành riêng cho khách hàng
            if (quyen == "Khách hàng")
            {
                return RedirectToAction("LichSuDatPhong", "DatPhongs");
            }

            var query = _context.DatPhong
                .Include(d => d.Phong)
                    .ThenInclude(p => p.KhachSan)
                .Include(d => d.TaiKhoan)
                .AsQueryable();

            // --- Lọc theo quyền (Chỉ áp dụng cho Nhân viên & Quản lý) ---
            if ((quyen == "Quản lý" || quyen == "Nhân viên") && !string.IsNullOrEmpty(maKhachSan))
            {
                // Nhân viên / Quản lý xem các đơn thuộc khách sạn của họ
                query = query.Where(d => d.Phong.MaKhachSan == maKhachSan);
            }

            query = query.Where(d =>
                d.TrangThaiDatPhong != "Đang lưu trú" &&
                d.TrangThaiDatPhong != "Đã trả phòng" &&
                d.TrangThaiDatPhong != "Đã thanh toán" &&
                d.TrangThaiDatPhong != "Hoàn thành"
            );

            // --- Sắp xếp theo ngày tạo (mới nhất lên đầu) ---
            query = query.OrderByDescending(d => d.NgayTao);

            // Gán tiêu đề hiển thị cho View
            ViewBag.TieuDe = (quyen == "Admin") ? "Tất cả đặt phòng" : "Danh sách đặt phòng tại cơ sở";

            var datPhongs = await query.ToListAsync();
            return View(datPhongs);
        }

        // GET: DatPhongs/LichSuDatPhong
        public async Task<IActionResult> LichSuDatPhong()
        {
            // Lấy thông tin người dùng từ Session
            string quyen = HttpContext.Session.GetString("QuyenHan");
            int? taiKhoanId = HttpContext.Session.GetInt32("ID");

            // Chỉ cho phép Khách hàng truy cập, nếu không đẩy về trang đăng nhập hoặc trang chủ
            if (quyen != "Khách hàng" || !taiKhoanId.HasValue)
            {
                return RedirectToAction("DangNhap", "TaiKhoans"); // Đổi lại tên Action/Controller đăng nhập của bạn nếu khác
            }

            // Lấy danh sách đơn đặt phòng của riêng khách hàng này
            var lichSu = await _context.DatPhong
                .Include(d => d.Phong)
                    .ThenInclude(p => p.KhachSan) // Bao gồm cả thông tin khách sạn để hiển thị cho đẹp
                .Where(d => d.MaTaiKhoan == taiKhoanId.Value)
                .OrderByDescending(d => d.NgayTao) // Đơn mới nhất lên đầu
                .ToListAsync();

            return View(lichSu);
        }

        // GET: DatPhongs/DangLuuTru
        public async Task<IActionResult> DangLuuTru()
        {
            // 1. Lấy quyền & mã khách sạn của người dùng đang đăng nhập từ Session
            string quyen = HttpContext.Session.GetString("QuyenHan");
            string maKhachSan = HttpContext.Session.GetString("MaKhachSan");

            // 2. Tạo câu truy vấn cơ bản: Chỉ lọc lấy đơn "Đang lưu trú"
            var query = _context.DatPhong
                .Include(d => d.Phong)
                    .ThenInclude(p => p.KhachSan)
                .Include(d => d.TaiKhoan)
                .Where(d => d.TrangThaiDatPhong == "Đang lưu trú") // Đã cập nhật dòng này
                .AsQueryable();

            // 3. LỌC THEO KHÁCH SẠN: Nếu là Nhân viên / Quản lý thì chỉ hiện đơn của khách sạn họ làm
            if ((quyen == "Quản lý" || quyen == "Nhân viên") && !string.IsNullOrEmpty(maKhachSan))
            {
                query = query.Where(d => d.Phong.MaKhachSan == maKhachSan);
            }
            // (Nếu là Admin thì bỏ qua bước này, tức là sẽ nhìn thấy tất cả)

            // 4. Sắp xếp ngày tạo mới nhất lên đầu và lấy dữ liệu
            var danhSachLuuTru = await query.OrderByDescending(d => d.NgayTao).ToListAsync();

            // Truyền tiêu đề sang View
            ViewBag.TieuDe = "Đơn đang lưu trú"; // Chỉnh sửa lại tiêu đề cho đúng với dữ liệu hiển thị

            // Trả về View DangLuuTru.cshtml kèm theo dữ liệu đã lọc
            return View(danhSachLuuTru);
        }

        public async Task<IActionResult> HoaDon(string? maKhachSan = null)
        {
            // ✅ 1. Cập nhật điều kiện lọc mới theo yêu cầu
            var query = _context.DatPhong
                .Include(d => d.Phong)
                    .ThenInclude(p => p.KhachSan)
                .Include(d => d.TaiKhoan)
                .Where(d => d.TrangThaiDatPhong == "Hoàn thành" && d.TrangThaiThanhToan == "Đã thanh toán");

            // Lấy quyền & thông tin từ Session
            string quyen = HttpContext.Session.GetString("QuyenHan");
            string maKhachSan_Session = HttpContext.Session.GetString("MaKhachSan");

            string tenKS = null; // tên khách sạn để hiển thị tiêu đề
            string maKS_DangXem = null; // Mã KS thực sự dùng để lọc

            // ✅ 2. Bỏ quyền của khách hàng, chỉ xử lý Quản lý/Nhân viên và Admin
            if (quyen == "Quản lý" || quyen == "Nhân viên")
            {
                maKS_DangXem = !string.IsNullOrEmpty(maKhachSan) ? maKhachSan : maKhachSan_Session;

                if (!string.IsNullOrEmpty(maKS_DangXem))
                {
                    query = query.Where(d => d.Phong.MaKhachSan == maKS_DangXem);
                    tenKS = await _context.KhachSan
                        .Where(k => k.MaKhachSan == maKS_DangXem)
                        .Select(k => k.TenKhachSan)
                        .FirstOrDefaultAsync();
                }

                ViewBag.KhachSan = tenKS != null ? $"{tenKS}" : "khách sạn của bạn";
            }
            else if (!string.IsNullOrEmpty(maKhachSan))
            {
                maKS_DangXem = maKhachSan;

                tenKS = await _context.KhachSan
                    .Where(k => k.MaKhachSan == maKhachSan)
                    .Select(k => k.TenKhachSan)
                    .FirstOrDefaultAsync();

                query = query.Where(d => d.Phong.MaKhachSan == maKhachSan);
                ViewBag.KhachSan = tenKS != null ? $"{tenKS}" : maKhachSan;
            }
            else
            {
                ViewBag.KhachSan = "Tất cả";
            }

            // ✅ 3. Lấy dữ liệu thô từ Database lên trước
            var hoaDonList = await query.ToListAsync();

            // ✅ 4. THỰC HIỆN GOM NHÓM BẰNG KIỂU DỮ LIỆU VÔ DANH (DYNAMIC)
            var userGroupedList = hoaDonList
                .Where(d => d.TaiKhoan != null)
                .GroupBy(d => d.MaTaiKhoan) // Gom nhóm các hóa đơn có cùng ID Khách Hàng
                .Select(g => {
                    var taiKhoan = g.First().TaiKhoan;

                    // Tính tổng tiền của khách hàng đó
                    decimal totalAmount = g.Sum(item => {
                        int soNgayO = (item.NgayTraPhong - item.NgayNhanPhong).Days;
                        if (soNgayO <= 0) soNgayO = 1;
                        return (item.Phong?.GiaPhong ?? 0) * soNgayO;
                    });

                    // Ép kiểu sang (dynamic) để không cần tạo class mới
                    return (dynamic)new
                    {
                        TaiKhoanId = g.Key,
                        TenKhachHang = taiKhoan?.HoVaTen ?? "Khách ẩn danh",
                        TenDangNhap = taiKhoan?.TenDangNhap ?? "",
                        SoDienThoai = taiKhoan?.SoDienThoai ?? "Chưa có",
                        Email = taiKhoan?.Email ?? "",
                        TongTienTichLuy = totalAmount,
                        SoLuongHoaDon = g.Count()
                    };
                })
                .OrderByDescending(u => u.TongTienTichLuy) // Sắp xếp khách VIP mua nhiều lên đầu
                .ToList();

            // Trả về danh sách dynamic cho View
            return View(userGroupedList);
        }

        // GET: DatPhongs/ChiTietHoaDonKhach/5
        public async Task<IActionResult> ChiTietHoaDonKhach(int id)
        {
            // Lấy danh sách hóa đơn chi tiết của người này
            var chiTietHoaDon = await _context.DatPhong
                .Include(d => d.Phong)
                    .ThenInclude(p => p.KhachSan)
                .Include(d => d.TaiKhoan)
                .Where(d => d.TaiKhoan.ID == id && d.TrangThaiDatPhong == "Hoàn thành" && d.TrangThaiThanhToan == "Đã thanh toán")
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            // Lấy tên khách hàng để truyền ra View làm tiêu đề
            if (chiTietHoaDon.Any())
            {
                ViewBag.TenKhachHang = chiTietHoaDon.First().TaiKhoan?.HoVaTen;
            }
            else
            {
                ViewBag.TenKhachHang = "Khách hàng";
            }

            return View(chiTietHoaDon);
        }

        // GET: DatPhongs/DetailsPartial/5
        public async Task<IActionResult> DetailsPartial(int? id)
        {
            if (id == null) return NotFound();

            var datPhong = await _context.DatPhong
                .Include(d => d.TaiKhoan)
                .Include(d => d.Phong)
                    .ThenInclude(p => p.KhachSan)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (datPhong == null)
                return NotFound();

            // Phải chỉ định chính xác tên file View và truyền dữ liệu model sang
            return PartialView("_DetailsPartial", datPhong);
        }

        // GET: DatPhongs/ThanhToanQRPartial/5
        public async Task<IActionResult> ThanhToanQRPartial(int? id)
        {
            if (id == null) return NotFound();

            var datPhong = await _context.DatPhong
                .Include(d => d.Phong)
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (datPhong == null) return NotFound();

            return PartialView("_ThanhToanQRPartial", datPhong);
        }

        // 1. Hàm để Máy tính gọi kiểm tra trạng thái liên tục
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)] // Chống cache trình duyệt
        public async Task<IActionResult> KiemTraTrangThai(int id)
        {
            // Dùng AsNoTracking() để xuyên qua lớp Cache, chọc thẳng xuống Database lấy kết quả mới nhất
            var datPhong = await _context.DatPhong
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(m => m.ID == id);

            if (datPhong != null && datPhong.TrangThaiDatPhong == "Đã thanh toán")
            {
                return Json(new { daThanhToan = true });
            }
            return Json(new { daThanhToan = false });
        }

        // 2. Hàm trả về giao diện cho Điện thoại khi quét mã QR
        [HttpGet]
        public async Task<IActionResult> TrangThanhToanDienThoai(int id)
        {
            var datPhong = await _context.DatPhong
                .Include(d => d.Phong)
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (datPhong == null) return NotFound();

            return View(datPhong);
        }

        // 3. Hàm xử lý khi trên Điện thoại khách bấm nút "Xác nhận Thanh toán"
        [HttpPost]
        public async Task<IActionResult> XacNhanTuDienThoai(int id)
        {
            var datPhong = await _context.DatPhong.FindAsync(id);
            if (datPhong != null)
            {
                // Đồng bộ cả 2 trạng thái khi khách bấm xác nhận trên điện thoại
                datPhong.TrangThaiDatPhong = "Đã thanh toán";
                datPhong.TrangThaiThanhToan = "Đã thanh toán";

                _context.Update(datPhong);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // GET: DatPhongs/Create
        public IActionResult Create(int? MaPhong)
        {
            // ✅ Đọc đúng key "ID" từ Session
            int? userId = HttpContext.Session.GetInt32("ID");

            if (userId != null)
            {
                var taiKhoan = _context.TaiKhoan.FirstOrDefault(t => t.ID == userId);
                if (taiKhoan != null)
                {
                    ViewBag.MaTaiKhoan = taiKhoan.ID;
                    ViewBag.TenKhachHang = taiKhoan.HoVaTen;
                    ViewBag.SoDienThoai = taiKhoan.SoDienThoai;
                }
            }
            else
            {
                // Nếu chưa đăng nhập, điều hướng về trang đăng nhập
                TempData["ErrorMessage"] = "⚠️ Vui lòng đăng nhập trước khi đặt phòng!";
                return RedirectToAction("DangNhap", "DangKy_DangNhap");
            }

            // ✅ Lấy thông tin phòng
            if (MaPhong != null)
            {
                var phong = _context.Phong
                    .Include(p => p.KhachSan)
                    .FirstOrDefault(p => p.ID == MaPhong);

                if (phong != null)
                {
                    ViewBag.MaPhong = phong.ID;
                    ViewBag.SoPhong = phong.SoPhong;
                    ViewBag.TenKhachSan = phong.KhachSan?.TenKhachSan;
                    ViewBag.GiaPhong = phong.GiaPhong;
                }
            }

            // ✅ Trả về model mặc định
            return View(new DatPhong
            {
                NgayTao = DateTime.Now,
                NgayNhanPhong = DateTime.Now,
                NgayTraPhong = DateTime.Now.AddDays(1)
            });
        }

        // POST: DatPhongs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatPhong datPhong)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                ViewBag.Error = string.Join(" | ", errors);

                // Nạp lại dữ liệu ViewBag để không mất khi hiển thị lại form
                var phong = await _context.Phong.Include(p => p.KhachSan)
                    .FirstOrDefaultAsync(p => p.ID == datPhong.MaPhong);
                if (phong != null)
                {
                    ViewBag.TenKhachSan = phong.KhachSan?.TenKhachSan;
                    ViewBag.SoPhong = phong.SoPhong;
                    ViewBag.MaPhong = phong.ID;
                    ViewBag.GiaPhong = phong.GiaPhong;
                }

                var taiKhoan = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.ID == datPhong.MaTaiKhoan);
                if (taiKhoan != null)
                {
                    ViewBag.TenKhachHang = taiKhoan.HoVaTen;
                    ViewBag.SoDienThoai = taiKhoan.SoDienThoai;
                    ViewBag.MaTaiKhoan = taiKhoan.ID;
                }

                return View(datPhong);
            }

            datPhong.TrangThaiDatPhong = "Chờ xác nhận";
            datPhong.NgayTao = DateTime.Now;

            _context.DatPhong.Add(datPhong);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đặt phòng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: DatPhongs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var datPhong = await _context.DatPhong.FindAsync(id);
            if (datPhong == null)
            {
                return NotFound();
            }
            ViewData["MaPhong"] = new SelectList(_context.Phong, "ID", "HinhAnh", datPhong.MaPhong);
            ViewData["MaTaiKhoan"] = new SelectList(_context.TaiKhoan, "ID", "Email", datPhong.MaTaiKhoan);
            return View(datPhong);
        }

        // POST: DatPhongs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,NgayTao,NgayNhanPhong,NgayTraPhong,SoNguoi,TongTien,TrangThaiDatPhong,GhiChu,MaTaiKhoan,MaPhong")] DatPhong datPhong)
        {
            if (id != datPhong.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(datPhong);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DatPhongExists(datPhong.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaPhong"] = new SelectList(_context.Phong, "ID", "HinhAnh", datPhong.MaPhong);
            ViewData["MaTaiKhoan"] = new SelectList(_context.TaiKhoan, "ID", "Email", datPhong.MaTaiKhoan);
            return View(datPhong);
        }

        // POST: DatPhongs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var datphong = await _context.DatPhong.FindAsync(id);
            if (datphong != null)
            {
                _context.DatPhong.Remove(datphong);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "🗑️ Xóa thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DatPhongExists(int id)
        {
            return _context.DatPhong.Any(e => e.ID == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var datPhong = await _context.DatPhong
                .Include(dp => dp.Phong)
                .FirstOrDefaultAsync(dp => dp.ID == id);

            if (datPhong == null)
                return NotFound();

            // ✅ Cập nhật trạng thái đơn thành Đang lưu trú
            datPhong.TrangThaiDatPhong = "Đang lưu trú";

            // ✅ Cập nhật trạng thái phòng thực tế (đổi thành Đang sử dụng)
            if (datPhong.Phong != null)
            {
                datPhong.Phong.TrangThai = "Đã đặt";
                _context.Update(datPhong.Phong);
            }

            _context.Update(datPhong);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã Check-in thành công! Khách hàng bắt đầu lưu trú.";

            // Check-in xong thì tự động chuyển hướng sang trang Đang Lưu Trú
            return RedirectToAction(nameof(DangLuuTru));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            var datPhong = await _context.DatPhong
                .Include(dp => dp.Phong)
                .FirstOrDefaultAsync(dp => dp.ID == id);

            if (datPhong == null)
                return NotFound();

            // ✅ 1. Cập nhật trạng thái đơn thành Hoàn thành
            datPhong.TrangThaiDatPhong = "Hoàn thành";

            // ✅ 2. Đảm bảo trạng thái thanh toán là Đã thanh toán khi trả phòng
            datPhong.TrangThaiThanhToan = "Đã thanh toán";

            datPhong.NgayTao = DateTime.Now;

            // ✅ 3. Cập nhật trạng thái phòng thực tế (trả về trạng thái Trống)
            if (datPhong.Phong != null)
            {
                datPhong.Phong.TrangThai = "Còn trống";
                _context.Update(datPhong.Phong);
            }

            _context.Update(datPhong);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã Check-out thành công! Phòng đã được dọn dẹp và sẵn sàng.";

            // Check-out xong thường chuyển hướng về trang Hóa Đơn hoặc danh sách lịch sử
            return RedirectToAction(nameof(HoaDon));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(int id)
        {
            // Lấy bản ghi đặt phòng kèm thông tin Khách hàng và Khách sạn để gửi Mail
            var datPhong = await _context.DatPhong
                .Include(dp => dp.TaiKhoan)
                .Include(dp => dp.Phong)
                    .ThenInclude(p => p.KhachSan)
                .FirstOrDefaultAsync(dp => dp.ID == id);

            if (datPhong == null)
                return Json(new { success = false, message = "Không tìm thấy dữ liệu đặt phòng!" });

            // ✅ Cập nhật trạng thái đặt phòng
            datPhong.TrangThaiDatPhong = "Đã xác nhận";
            _context.DatPhong.Update(datPhong);

            // ✅ Tìm phòng tương ứng và đổi trạng thái sang "Đã đặt"
            if (datPhong.Phong != null)
            {
                datPhong.Phong.TrangThai = "Đã đặt";
                _context.Phong.Update(datPhong.Phong);
            }

            await _context.SaveChangesAsync();

            // 📧 GỬI EMAIL XÁC NHẬN
            // (Trong hàm XacNhan, thay thế phần gửi email cũ bằng đoạn này)
            if (!string.IsNullOrEmpty(datPhong.TaiKhoan?.Email))
            {
                int soNgayO = (datPhong.NgayTraPhong - datPhong.NgayNhanPhong).Days;
                if (soNgayO <= 0) soNgayO = 1;
                decimal tongTien = (datPhong.Phong?.GiaPhong ?? 0) * soNgayO;

                string subject = $"[{datPhong.Phong?.KhachSan?.TenKhachSan}] Xác nhận đơn đặt phòng thành công";
                string extraInfo = $@"
        <tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Số khách lưu trú:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>{datPhong.SoNguoi} người</td></tr>
        <tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Giờ nhận phòng:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>Từ 14:00 - {datPhong.NgayNhanPhong:dd/MM/yyyy}</td></tr>
        <tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Giờ trả phòng:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>Trước 12:00 - {datPhong.NgayTraPhong:dd/MM/yyyy}</td></tr>
        <tr><td style='padding: 12px 0 0 0; color: #333;'><b>Tổng thanh toán:</b></td><td style='padding: 12px 0 0 0; color: #dc3545; font-weight: bold; font-size: 16px;'>{tongTien:N0} VNĐ</td></tr>";

                string body = TaoNoiDungEmail(
                    datPhong,
                    mauChuDao: "#007bff", // Xanh dương hoàng gia
                    tieuDe: "XÁC NHẬN ĐẶT PHÒNG",
                    loiNhanChinh: $"Chúng tôi chân thành cảm ơn Quý khách đã tin tưởng và lựa chọn <b>{datPhong.Phong?.KhachSan?.TenKhachSan}</b> cho kỳ nghỉ sắp tới. Chúng tôi rất vui mừng xác nhận đơn đặt phòng của Quý khách đã được hệ thống ghi nhận thành công.",
                    thongTinBoSung: extraInfo,
                    loiChaoKet: "Nếu Quý khách có yêu cầu đặc biệt hoặc cần hỗ trợ dịch vụ đưa đón sân bay, vui lòng liên hệ trực tiếp với bộ phận Lễ tân qua số Hotline. Rất mong được đón tiếp Quý khách!"
                );

                await SendEmailAsync(datPhong.TaiKhoan.Email, subject, body);
            }

            TempData["SuccessMessage"] = "Đã xác nhận và gửi email cho khách hàng!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(int id)
        {
            var datPhong = await _context.DatPhong
                .Include(dp => dp.TaiKhoan)
                .Include(dp => dp.Phong)
                    .ThenInclude(p => p.KhachSan)
                .FirstOrDefaultAsync(dp => dp.ID == id);

            if (datPhong == null)
                return NotFound();

            // ✅ Cập nhật trạng thái đặt phòng
            datPhong.TrangThaiDatPhong = "Đang lưu trú";
            datPhong.TrangThaiThanhToan = "Đã thanh toán";

            // ✅ Cập nhật trạng thái phòng
            if (datPhong.Phong != null)
            {
                datPhong.Phong.TrangThai = "Đã đặt";
                _context.Update(datPhong.Phong);
            }

            _context.Update(datPhong);
            await _context.SaveChangesAsync();

            // 📧 GỬI EMAIL THANH TOÁN
            // (Trong hàm ThanhToan, thay thế phần gửi email cũ bằng đoạn này)
            if (!string.IsNullOrEmpty(datPhong.TaiKhoan?.Email))
            {
                int soNgayO = (datPhong.NgayTraPhong - datPhong.NgayNhanPhong).Days;
                if (soNgayO <= 0) soNgayO = 1;
                decimal tongTien = (datPhong.Phong?.GiaPhong ?? 0) * soNgayO;

                string subject = $"[{datPhong.Phong?.KhachSan?.TenKhachSan}] Biên lai thanh toán điện tử";
                string extraInfo = $@"
        <tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Thời gian lưu trú:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>{datPhong.NgayNhanPhong:dd/MM/yyyy} đến {datPhong.NgayTraPhong:dd/MM/yyyy}</td></tr>
        <tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Tổng tiền:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef; font-weight: bold;'>{tongTien:N0} VNĐ</td></tr>
        <tr><td style='padding: 12px 0 0 0;'><b>Trạng thái:</b></td><td style='padding: 12px 0 0 0;'><span style='background-color: #198754; color: white; padding: 4px 10px; border-radius: 4px; font-size: 12px; font-weight: bold;'>ĐÃ THANH TOÁN</span></td></tr>";

                string body = TaoNoiDungEmail(
                    datPhong,
                    mauChuDao: "#198754", // Xanh lá
                    tieuDe: "BIÊN LAI THANH TOÁN",
                    loiNhanChinh: "Chúng tôi xin gửi thông báo xác nhận: Khách sạn đã nhận được đầy đủ khoản thanh toán cho đơn đặt phòng của Quý khách. Dưới đây là biên lai điện tử chi tiết:",
                    thongTinBoSung: extraInfo,
                    loiChaoKet: "Sự ưu ái của Quý khách là niềm vinh hạnh của chúng tôi. Kính chúc Quý khách có một kỳ nghỉ thật tuyệt vời và trọn vẹn!"
                );

                await SendEmailAsync(datPhong.TaiKhoan.Email, subject, body);
            }

            TempData["SuccessMessage"] = "Đã thanh toán và gửi email biên lai!";
            return RedirectToAction(nameof(DangLuuTru));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDatPhong(int id)
        {
            var datPhong = await _context.DatPhong
                .Include(dp => dp.TaiKhoan)
                .Include(dp => dp.Phong)
                    .ThenInclude(p => p.KhachSan)
                .FirstOrDefaultAsync(dp => dp.ID == id);

            if (datPhong == null)
                return NotFound();

            // ✅ Chỉ cho phép hủy khi chưa xác nhận hoặc đang chờ xác nhận
            if (datPhong.TrangThaiDatPhong == "Chờ xác nhận" || datPhong.TrangThaiDatPhong == "Đã xác nhận")
            {
                datPhong.TrangThaiDatPhong = "Đã hủy";

                // Nếu phòng đã xác nhận và đang ở trạng thái "Đã đặt", chuyển về "Còn trống"
                if (datPhong.Phong != null && datPhong.Phong.TrangThai == "Đã đặt")
                {
                    datPhong.Phong.TrangThai = "Còn trống";
                    _context.Update(datPhong.Phong);
                }

                _context.Update(datPhong);
                await _context.SaveChangesAsync();

                // 📧 GỬI EMAIL HỦY ĐƠN
                // (Trong hàm HuyDatPhong, thay thế phần gửi email cũ bằng đoạn này)
                if (!string.IsNullOrEmpty(datPhong.TaiKhoan?.Email))
                {
                    string subject = $"[{datPhong.Phong?.KhachSan?.TenKhachSan}] Thông báo hủy đơn đặt phòng";
                    string extraInfo = $@"
        <tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Thời gian dự kiến:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>{datPhong.NgayNhanPhong:dd/MM/yyyy} - {datPhong.NgayTraPhong:dd/MM/yyyy}</td></tr>
        <tr><td style='padding: 12px 0 0 0;'><b>Trạng thái:</b></td><td style='padding: 12px 0 0 0;'><span style='background-color: #dc3545; color: white; padding: 4px 10px; border-radius: 4px; font-size: 12px; font-weight: bold;'>ĐÃ HỦY</span></td></tr>";

                    string body = TaoNoiDungEmail(
                        datPhong,
                        mauChuDao: "#dc3545", // Đỏ tươi
                        tieuDe: "THÔNG BÁO HỦY PHÒNG",
                        loiNhanChinh: "Theo yêu cầu của Quý khách (hoặc do quy định của khách sạn), chúng tôi xin thông báo đơn đặt phòng của Quý khách đã được tiến hành <b>HỦY</b> trên hệ thống.",
                        thongTinBoSung: extraInfo,
                        loiChaoKet: "Nếu đây là một sự nhầm lẫn hoặc Quý khách cần hỗ trợ thêm về chính sách hoàn tiền, xin vui lòng liên hệ ngay với bộ phận Chăm sóc khách hàng. Khách sạn luôn rộng mở chào đón Quý khách trong những dịp tới."
                    );

                    await SendEmailAsync(datPhong.TaiKhoan.Email, subject, body);
                }

                TempData["SuccessMessage"] = "❌ Hủy đặt phòng thành công và đã báo qua Email!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy vì phòng đã thanh toán hoặc đã hoàn tất!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> XuatHoaDonPDF(int id)
        {
            var datPhong = await _context.DatPhong
                .Include(dp => dp.TaiKhoan)
                .Include(dp => dp.Phong)
                    .ThenInclude(p => p.KhachSan)
                .FirstOrDefaultAsync(dp => dp.ID == id);

            if (datPhong == null)
                return NotFound();

            int soNgayO = (datPhong.NgayTraPhong - datPhong.NgayNhanPhong).Days;
            if (soNgayO <= 0) soNgayO = 1;
            decimal tongTien = (datPhong.Phong?.GiaPhong ?? 0) * soNgayO;

            // --- Thư mục lưu PDF ---
            string folderPath = @"D:\DoAnTongHop\ASP.NET\HoaDonPDF";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // --- Đặt tên file có ngày giờ xuất ---
            string fileName = $"HoaDon_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(folderPath, fileName);

            // --- Font ---
            string fontFileName = "arial.ttf";
            string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", fontFileName);

            if (!System.IO.File.Exists(fontPath))
            {
                return Content($"Không tìm thấy font: {fontPath}. Hãy thêm file arial.ttf vào thư mục wwwroot/fonts.");
            }

            // --- Tạo PDF và lưu vào ổ D ---
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                Document doc = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(doc, fs);
                doc.Open();

                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                Font titleFont = new Font(bf, 20, Font.BOLD, new BaseColor(0, 102, 204));
                Font blackFont = new Font(bf, 12, Font.NORMAL, new BaseColor(0, 0, 0));
                Font boldFont = new Font(bf, 12, Font.BOLD, new BaseColor(0, 102, 204));
                Font redFont = new Font(bf, 12, Font.BOLD, new BaseColor(255, 0, 0));
                Font greenFont = new Font(bf, 14, Font.BOLD, new BaseColor(0, 128, 0));

                // --- Tiêu đề ---
                var title = new Paragraph("HÓA ĐƠN ĐẶT PHÒNG", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };
                doc.Add(title);

                // --- Ngày xuất ---
                var ngayXuat = new Paragraph($"Ngày xuất hóa đơn: {DateTime.Now:dd/MM/yyyy HH:mm}", redFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingAfter = 15f
                };
                doc.Add(ngayXuat);

                // --- Thông tin khách hàng ---
                doc.Add(new Paragraph("THÔNG TIN KHÁCH HÀNG", boldFont) { SpacingAfter = 5f });
                doc.Add(new Paragraph($"Họ và tên: {datPhong.TaiKhoan?.HoVaTen ?? "Không rõ"}", blackFont));
                doc.Add(new Paragraph($"Email: {datPhong.TaiKhoan?.Email ?? "Không có"}", blackFont));
                doc.Add(new Paragraph($"SĐT: {datPhong.TaiKhoan?.SoDienThoai ?? "Không có"}", blackFont));
                doc.Add(new Paragraph(" ", blackFont));

                // --- Thông tin phòng & khách sạn ---
                doc.Add(new Paragraph("THÔNG TIN PHÒNG & KHÁCH SẠN", boldFont) { SpacingAfter = 5f });
                doc.Add(new Paragraph($"Khách sạn: {datPhong.Phong?.KhachSan?.TenKhachSan ?? "Không rõ"}", blackFont));
                doc.Add(new Paragraph($"Số phòng: {(datPhong.Phong?.SoPhong.ToString() ?? "")}", blackFont));
                doc.Add(new Paragraph($"Giá phòng: {(datPhong.Phong?.GiaPhong ?? 0).ToString("N0", CultureInfo.InvariantCulture)} VNĐ", blackFont));
                doc.Add(new Paragraph(" ", blackFont));

                // --- Thời gian lưu trú ---
                doc.Add(new Paragraph("THỜI GIAN LƯU TRÚ", boldFont) { SpacingAfter = 5f });
                doc.Add(new Paragraph($"Ngày nhận: {datPhong.NgayNhanPhong:dd/MM/yyyy}", blackFont));
                doc.Add(new Paragraph($"Ngày trả: {datPhong.NgayTraPhong:dd/MM/yyyy}", blackFont));
                doc.Add(new Paragraph($"Số ngày ở: {soNgayO} ngày", blackFont));
                doc.Add(new Paragraph(" ", blackFont));

                // --- Tổng tiền ---
                var tongTienPara = new Paragraph($"TỔNG TIỀN: {tongTien.ToString("N0", CultureInfo.InvariantCulture)} VNĐ", greenFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 10f,
                    SpacingAfter = 15f
                };
                doc.Add(tongTienPara);

                // --- Cảm ơn ---
                var camOn = new Paragraph("Cảm ơn quý khách đã sử dụng dịch vụ của chúng tôi!", blackFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 30f
                };
                doc.Add(camOn);

                doc.Close();
            }

            // ✅ Lưu thông báo
            TempData["SuccessMessage"] = "✅ Xuất hóa đơn thành công! File đã được lưu.";

            // --- Lấy quyền để quay lại đúng trang ---
            string quyen = HttpContext.Session.GetString("QuyenHan");
            string maKhachSan = HttpContext.Session.GetString("MaKhachSan");
            int? maTaiKhoan = HttpContext.Session.GetInt32("ID");

            if (quyen == "Khách hàng" && maTaiKhoan.HasValue)
                return RedirectToAction("HoaDon", new { id = maTaiKhoan.Value });
            else if ((quyen == "Nhân viên" || quyen == "Quản lý") && !string.IsNullOrEmpty(maKhachSan))
                return RedirectToAction("HoaDon", new { maKhachSan = maKhachSan });
            else
                return RedirectToAction("HoaDon"); // Admin
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // 1. ĐỌC TRỰC TIẾP TỪ BIẾN MÔI TRƯỜNG (Cấu hình trong launchSettings.json)
                string fromEmail = Environment.GetEnvironmentVariable("EMAIL");
                string appPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

                // 2. Kiểm tra nếu biến môi trường trống thì mới thử đọc từ appsettings.json
                if (string.IsNullOrEmpty(fromEmail))
                    fromEmail = _configuration["EmailSettings:Email"];
                if (string.IsNullOrEmpty(appPassword))
                    appPassword = _configuration["EmailSettings:Email_Password"];

                // 3. KIỂM TRA CUỐI CÙNG
                if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(appPassword))
                {
                    TempData["ErrorMessage"] = "Lỗi hệ thống: Không tìm thấy cấu hình Email (Biến môi trường rỗng)!";
                    return false;
                }

                var message = new MailMessage(fromEmail, toEmail, subject, body)
                {
                    IsBodyHtml = true
                };

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(fromEmail, appPassword);

                    // Tăng thời gian chờ nếu mạng yếu
                    client.Timeout = 20000;

                    await client.SendMailAsync(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi chi tiết (Ví dụ: Sai mật khẩu, SMTP bị chặn...)
                TempData["ErrorMessage"] = "Lỗi khi gửi mail: " + ex.Message;
                return false;
            }
        }

        private string TaoNoiDungEmail(DoAn_HotelBooking.Models.DatPhong datPhong, string mauChuDao, string tieuDe, string loiNhanChinh, string thongTinBoSung, string loiChaoKet)
        {
            string tenKhachSan = datPhong.Phong?.KhachSan?.TenKhachSan ?? "Hệ thống Khách sạn";
            string tenKhachHang = datPhong.TaiKhoan?.HoVaTen ?? "Quý khách";
            string soPhong = datPhong.Phong?.SoPhong.ToString() ?? "Chưa rõ";

            // Tạo mã đặt phòng chuyên nghiệp (VD: BK001024)
            string maDatPhong = "BK" + datPhong.ID.ToString("D6");

            // Lấy thông tin liên hệ của Khách sạn
            string diaChi = datPhong.Phong?.KhachSan?.DiaChi ?? "Việt Nam";
            string sdt = datPhong.Phong?.KhachSan?.SoDienThoai ?? "Hotline: 1900 xxxx";

            return $@"
    <div style='font-family: ""Segoe UI"", Tahoma, Arial, sans-serif; line-height: 1.6; color: #333; max-width: 650px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.05);'>
        
        <!-- Header -->
        <div style='background-color: {mauChuDao}; padding: 30px 20px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 24px; text-transform: uppercase; letter-spacing: 1px;'>{tenKhachSan}</h1>
            <p style='color: rgba(255,255,255,0.85); margin: 8px 0 0 0; font-size: 15px; font-weight: 500;'>{tieuDe}</p>
        </div>
        
        <!-- Body -->
        <div style='padding: 30px 25px;'>
            <p style='font-size: 16px; margin-top: 0;'>Kính gửi Quý khách <b>{tenKhachHang}</b>,</p>
            <p style='font-size: 15px;'>{loiNhanChinh}</p>
            
            <!-- Box Chi tiết -->
            <div style='background-color: #f8f9fa; padding: 25px; border-radius: 8px; margin: 25px 0; border-left: 5px solid {mauChuDao};'>
                <h3 style='margin-top: 0; color: #333; font-size: 16px; border-bottom: 1px solid #e0e0e0; padding-bottom: 12px; margin-bottom: 15px;'>
                    THÔNG TIN ĐẶT PHÒNG <span style='float: right; color: {mauChuDao};'>#{maDatPhong}</span>
                </h3>
                
                <table style='width: 100%; font-size: 15px; color: #444; border-collapse: collapse;'>
                    <tr>
                        <td style='padding: 8px 0; width: 40%; border-bottom: 1px dashed #e9ecef;'><b>Khách sạn:</b></td>
                        <td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>{tenKhachSan}</td>
                    </tr>
                    <tr>
                        <td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Hạng/Số phòng:</b></td>
                        <td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>{soPhong}</td>
                    </tr>
                    {thongTinBoSung}
                </table>
            </div>
            
            <p style='font-size: 15px;'>{loiChaoKet}</p>
            
            <div style='margin-top: 35px; font-size: 15px;'>
                <p style='margin: 0;'>Trân trọng,</p>
                <p style='margin: 5px 0 0 0; font-weight: bold; color: {mauChuDao};'>Ban Giám Đốc & Đội ngũ {tenKhachSan}</p>
            </div>
        </div>
        
        <!-- Footer -->
        <div style='background-color: #2c3e50; padding: 25px 20px; text-align: center; color: #adb5bd; font-size: 13px;'>
            <p style='margin: 0 0 8px 0; font-size: 15px; color: #ffffff;'><b>{tenKhachSan}</b></p>
            <p style='margin: 0 0 6px 0;'>📍 Địa chỉ: {diaChi}</p>
            <p style='margin: 0 0 15px 0;'>📞 Điện thoại: {sdt}</p>
            <hr style='border: none; border-top: 1px solid #4a5b6c; margin: 0 0 15px 0;' />
            <p style='margin: 0; font-style: italic;'>Email này được tạo tự động từ hệ thống. Quý khách vui lòng không trả lời trực tiếp.</p>
        </div>
    </div>";
        }
    }
}
