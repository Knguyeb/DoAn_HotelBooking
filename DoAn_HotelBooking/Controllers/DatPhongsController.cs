using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Models;
using DoAn_HotelBooking.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
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

        private const decimal SO_TIEN_QUY_DOI_DIEM = 100000; // 100.000 VNĐ = 1 điểm

        private readonly ThangHangHelper _thangHangHelper;

        public DatPhongsController(DoAn_HotelBookingContext context, IConfiguration configuration, ThangHangHelper thangHangHelper)
        {
            _context = context;
            _configuration = configuration;
            _thangHangHelper = thangHangHelper;
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
                d.TrangThaiDatPhong == "Chờ xác nhận" ||
                d.TrangThaiDatPhong == "Đã xác nhận" ||
                d.TrangThaiDatPhong == "Đã thanh toán" ||
                d.TrangThaiDatPhong == "Đã hủy"
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

            // --- BỔ SUNG LẤY THÔNG TIN HẠNG THÀNH VIÊN ---
            var taiKhoan = await _context.TaiKhoan
                .Include(t => t.HangThanhVien)
                .FirstOrDefaultAsync(t => t.ID == taiKhoanId.Value);

            if (taiKhoan != null)
            {
                // 1. GỌI HÀM KIỂM TRA VÀ NÂNG HẠNG Ở ĐÂY (TRƯỚC KHI GÁN VIEWBAG)
                await _thangHangHelper.KiemTraVaNangHangAsync(taiKhoan);

                // 2. SAU KHI NÂNG HẠNG XONG MỚI GÁN VÀO VIEWBAG ĐỂ HIỂN THỊ
                ViewBag.TenHang = taiKhoan.HangThanhVien != null ? taiKhoan.HangThanhVien.TenHang : "Thành viên mới";
                ViewBag.DiemTichLuy = taiKhoan.DiemTichLuy;
            }
            else
            {
                ViewBag.TenHang = "Không xác định";
                ViewBag.DiemTichLuy = 0;
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

            string tenKS = null;
            string maKS_DangXem = null;

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
                .Select(g =>
                {
                    var taiKhoan = g.First().TaiKhoan;

                    // ✅ ĐÃ SỬA: Lấy tổng cột TongTien (chính là số tiền khách thực trả sau khi đã giảm giá)
                    decimal totalAmount = g.Sum(item => item.TongTien);

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
            string quyen = HttpContext.Session.GetString("QuyenHan");
            string maKhachSan = HttpContext.Session.GetString("MaKhachSan");

            var query = _context.DatPhong
             .Include(d => d.Phong)
                 .ThenInclude(p => p.KhachSan)
             .Include(d => d.TaiKhoan)
                 .ThenInclude(t => t.HangThanhVien)
             .Where(d =>
                 d.TaiKhoan.ID == id &&
                 d.TrangThaiDatPhong == "Hoàn thành" &&
                 d.TrangThaiThanhToan == "Đã thanh toán");

            // Chỉ lọc hóa đơn theo khách sạn
            if (quyen != "Admin")
            {
                query = query.Where(d => d.Phong.MaKhachSan == maKhachSan);
            }

            var chiTietHoaDon = await query
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            if (chiTietHoaDon.Any())
            {
                ViewBag.TenKhachHang = chiTietHoaDon.First().TaiKhoan?.HoVaTen;
                ViewBag.DiemTichLuy = chiTietHoaDon.First().TaiKhoan?.DiemTichLuy ?? 0;
                ViewBag.TenHang = chiTietHoaDon.First().TaiKhoan?.HangThanhVien?.TenHang ?? "Thành viên mới";
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
            // Bổ sung thêm .ThenInclude(p => p.KhachSan) để lấy tên khách sạn phục vụ gửi email
            var datPhong = await _context.DatPhong
                .Include(dp => dp.Phong)
                    .ThenInclude(p => p.KhachSan)
                .Include(dp => dp.TaiKhoan)
                .FirstOrDefaultAsync(dp => dp.ID == id);

            if (datPhong != null)
            {
                // Đồng bộ cả 2 trạng thái khi khách bấm xác nhận trên điện thoại
                datPhong.TrangThaiDatPhong = "Đã thanh toán";
                datPhong.TrangThaiThanhToan = "Đã thanh toán";

                // 🌟 LOGIC TÍCH ĐIỂM
                int soNgayO = (datPhong.NgayTraPhong - datPhong.NgayNhanPhong).Days;
                if (soNgayO <= 0) soNgayO = 1;

                // ✅ ĐÃ SỬA: Lấy tiền từ DB để tính điểm và gửi Email chuẩn xác
                decimal tongTienThucTe = datPhong.TongTien;
                decimal giaGoc = (datPhong.Phong?.GiaPhong ?? 0) * soNgayO;

                int diemDuocCong = 0;
                if (datPhong.TaiKhoan != null)
                {
                    // Tính điểm dựa trên TỔNG TIỀN THỰC TRẢ
                    diemDuocCong = (int)(tongTienThucTe / SO_TIEN_QUY_DOI_DIEM);
                    datPhong.TaiKhoan.DiemTichLuy += diemDuocCong;
                }

                _context.Update(datPhong);
                await _context.SaveChangesAsync();

                await _thangHangHelper.KiemTraVaNangHangAsync(datPhong.TaiKhoan);

                // 📧 GỬI EMAIL BIÊN LAI KHI THANH TOÁN QUA ĐIỆN THOẠI THÀNH CÔNG
                if (!string.IsNullOrEmpty(datPhong.TaiKhoan?.Email))
                {
                    string subject = $"[{datPhong.Phong?.KhachSan?.TenKhachSan}] Biên lai thanh toán điện tử (Mobile)";

                    // ✅ ĐÃ SỬA: Ghi rõ giá gốc, tiền ưu đãi hạng và tổng thực trả
                    string extraInfo = $@"
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Thời gian lưu trú:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>{datPhong.NgayNhanPhong:dd/MM/yyyy} đến {datPhong.NgayTraPhong:dd/MM/yyyy} ({soNgayO} đêm)</td></tr>
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Giá phòng gốc:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef; text-decoration: line-through; color: #6c757d;'>{giaGoc:N0} VNĐ</td></tr>";

                    // Nếu có giảm giá thì hiển thị
                    if (datPhong.TienGiam > 0)
                    {
                        extraInfo += $@"<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Ưu đãi hạng thẻ:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef; color: #dc3545; font-weight: bold;'>-{datPhong.TienGiam:N0} VNĐ</td></tr>";
                    }

                    extraInfo += $@"
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Tổng thanh toán:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef; color: #198754; font-weight: bold; font-size: 16px;'>{tongTienThucTe:N0} VNĐ</td></tr>
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Điểm thưởng tích lũy:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef; color: #ffc107; font-weight: bold;'>+ {diemDuocCong} điểm</td></tr>
<tr><td style='padding: 12px 0 0 0;'><b>Trạng thái:</b></td><td style='padding: 12px 0 0 0;'><span style='background-color: #198754; color: white; padding: 4px 10px; border-radius: 4px; font-size: 12px; font-weight: bold;'>ĐÃ THANH TOÁN QUA DI ĐỘNG</span></td></tr>";

                    string body = TaoNoiDungEmail(
                        datPhong,
                        mauChuDao: "#198754", // Màu xanh lá chủ đạo cho thanh toán thành công
                        tieuDe: "BIÊN LAI THANH TOÁN TRỰC TUYẾN",
                        loiNhanChinh: "Hệ thống điện tử đã ghi nhận khoản giao dịch trực tuyến thành công từ thiết bị di động của Quý khách. Dưới đây là biên lai thanh toán chi tiết:",
                        thongTinBoSung: extraInfo,
                        loiChaoKet: "Cảm ơn Quý khách đã sử dụng phương thức thanh toán tiện lợi của hệ thống. Kính chúc Quý khách có một kỳ nghỉ thật tuyệt vời và trọn vẹn!"
                    );

                    // Gọi hàm gửi mail bất đồng bộ
                    await SendEmailAsync(datPhong.TaiKhoan.Email, subject, body);
                }

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
                // 🌟 BỔ SUNG: Dùng Include để kết nối bảng TaiKhoan với bảng HangThanhVien
                var taiKhoan = _context.TaiKhoan
                    .Include(t => t.HangThanhVien)
                    .FirstOrDefault(t => t.ID == userId);

                if (taiKhoan != null)
                {
                    ViewBag.MaTaiKhoan = taiKhoan.ID;
                    ViewBag.TenKhachHang = taiKhoan.HoVaTen;
                    ViewBag.SoDienThoai = taiKhoan.SoDienThoai;

                    // 🌟 LẤY HẠNG VÀ TỶ LỆ GIẢM GIÁ TRUYỀN RA VIEW
                    ViewBag.TenHang = taiKhoan.HangThanhVien?.TenHang ?? "Thành viên mới";
                    ViewBag.TyLeGiamGia = taiKhoan.HangThanhVien?.TyLeGiamGia ?? 0;
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

                // --- 🌟 BỔ SUNG: LẤY DANH SÁCH NGÀY ĐÃ ĐẶT ---
                var bookedDates = _context.DatPhong
                    .Where(dp => dp.MaPhong == MaPhong &&
                                (dp.TrangThaiDatPhong == "Chờ xác nhận" || dp.TrangThaiDatPhong == "Đã xác nhận"))
                    .Select(dp => new
                    {
                        from = dp.NgayNhanPhong.ToString("yyyy-MM-dd"),
                        to = dp.NgayTraPhong.ToString("yyyy-MM-dd")
                    })
                    .ToList(); // Dùng ToList() vì hàm này không dùng async/await

                ViewBag.BookedDates = JsonSerializer.Serialize(bookedDates);
                // ---------------------------------------------
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatPhong datPhong)
        {
            // 1. Chuyển đổi thời gian sang UTC trước để kiểm tra trùng lịch chính xác
            DateTime checkInUtc = datPhong.NgayNhanPhong.ToUniversalTime();
            DateTime checkOutUtc = datPhong.NgayTraPhong.ToUniversalTime();

            // 2. THUẬT TOÁN KIỂM TRA TRÙNG LỊCH ĐẶT PHÒNG
            // Kiểm tra xem phòng này đã có ai đặt trong khoảng thời gian được yêu cầu chưa
            bool isConflict = await _context.DatPhong.AnyAsync(dp =>
                dp.MaPhong == datPhong.MaPhong &&
                (dp.TrangThaiDatPhong == "Chờ xác nhận" || dp.TrangThaiDatPhong == "Đã xác nhận") && // Bỏ qua các đơn Đã hủy/Đã trả phòng
                dp.NgayNhanPhong < checkOutUtc &&
                dp.NgayTraPhong > checkInUtc
            );

            // Nếu trùng lịch, thêm lỗi vào ModelState
            if (isConflict)
            {
                ModelState.AddModelError(string.Empty, "Rất tiếc! Phòng này đã có khách đặt trong khoảng thời gian bạn chọn. Vui lòng chọn ngày khác.");
            }

            // 3. Kiểm tra tính hợp lệ của Model (bao gồm cả lỗi trùng lịch vừa ném vào)
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                ViewBag.Error = string.Join(" | ", errors);

                // Nạp lại dữ liệu ViewBag để không mất khi hiển thị lại form
                var phongHienTai = await _context.Phong.Include(p => p.KhachSan)
                    .FirstOrDefaultAsync(p => p.ID == datPhong.MaPhong);
                if (phongHienTai != null)
                {
                    ViewBag.TenKhachSan = phongHienTai.KhachSan?.TenKhachSan;
                    ViewBag.SoPhong = phongHienTai.SoPhong;
                    ViewBag.MaPhong = phongHienTai.ID;
                    ViewBag.GiaPhong = phongHienTai.GiaPhong;
                }

                var taiKhoanHienTai = await _context.TaiKhoan
                    .FirstOrDefaultAsync(t => t.ID == datPhong.MaTaiKhoan);
                if (taiKhoanHienTai != null)
                {
                    ViewBag.TenKhachHang = taiKhoanHienTai.HoVaTen;
                    ViewBag.SoDienThoai = taiKhoanHienTai.SoDienThoai;
                    ViewBag.MaTaiKhoan = taiKhoanHienTai.ID;
                }

                return View(datPhong);
            }

            // --- 🌟 XỬ LÝ KHI MODEL HỢP LỆ VÀ BẮT ĐẦU TẠO ĐƠN ĐẶT PHÒNG ---

            // 4. Tìm thông tin tài khoản và hạng thành viên để lấy tỷ lệ giảm giá
            var taiKhoan = await _context.TaiKhoan
                .Include(t => t.HangThanhVien)
                .FirstOrDefaultAsync(t => t.ID == datPhong.MaTaiKhoan);

            if (taiKhoan?.HangThanhVien != null)
            {
                // Tính số tiền được giảm dựa theo % của hạng (Ví dụ: 5%, 10%)
                decimal tienGiam = datPhong.TongTien * (decimal)(taiKhoan.HangThanhVien.TyLeGiamGia / 100);

                datPhong.TienGiam = tienGiam;
                datPhong.TongTien -= tienGiam; // Khấu trừ thẳng vào tổng tiền phải trả
            }
            else
            {
                datPhong.TienGiam = 0; // Không có hạng hoặc lỗi thì mặc định giảm 0đ
            }

            // 5. Thiết lập các thông tin mặc định còn lại
            datPhong.TrangThaiDatPhong = "Chờ xác nhận";

            datPhong.NgayTao = DateTime.UtcNow;
            datPhong.NgayNhanPhong = checkInUtc; // Dùng luôn biến đã convert ở trên
            datPhong.NgayTraPhong = checkOutUtc;

            // 6. Lưu vào Cơ sở dữ liệu
            _context.DatPhong.Add(datPhong);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đặt phòng thành công! Hạng thành viên của bạn đã được áp dụng ưu đãi.";
            return RedirectToAction(nameof(LichSuDatPhong));
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

            // 🌟 SỬA TẠI ĐÂY: Khách thực sự vào ở thì phòng chuyển thành "Đang sử dụng"
            if (datPhong.Phong != null)
            {
                datPhong.Phong.TrangThai = "Đang sử dụng";
                _context.Update(datPhong.Phong);
            }

            _context.Update(datPhong);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã Check-in thành công! Khách hàng bắt đầu lưu trú.";
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
            datPhong.NgayTao = DateTime.UtcNow;

            // ✅ 3. GIỮ NGUYÊN: Trả phòng thì phòng vật lý trở về "Còn trống"
            if (datPhong.Phong != null)
            {
                datPhong.Phong.TrangThai = "Còn trống";
                _context.Update(datPhong.Phong);
            }

            _context.Update(datPhong);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã Check-out thành công! Phòng đã được dọn dẹp và sẵn sàng.";
            return RedirectToAction(nameof(HoaDon));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(int id)
        {
            var datPhong = await _context.DatPhong
                .Include(dp => dp.TaiKhoan)
                    .ThenInclude(t => t.HangThanhVien)
                .Include(dp => dp.Phong)
                    .ThenInclude(p => p.KhachSan)
                .FirstOrDefaultAsync(dp => dp.ID == id);

            if (datPhong == null)
                return Json(new { success = false, message = "Không tìm thấy dữ liệu đặt phòng!" });

            // ✅ Cập nhật trạng thái đặt phòng
            datPhong.TrangThaiDatPhong = "Đã xác nhận";
            _context.DatPhong.Update(datPhong);

            // ❌ SỬA TẠI ĐÂY: XÓA ĐOẠN ĐỔI TRẠNG THÁI PHÒNG
            // Lễ tân xác nhận đơn cho tháng sau thì phòng tháng này vẫn phải Trống.

            await _context.SaveChangesAsync();

            // 📧 GỬI EMAIL XÁC NHẬN (GIỮ NGUYÊN CODE CỦA BẠN)
            if (!string.IsNullOrEmpty(datPhong.TaiKhoan?.Email))
            {
                int soNgayO = (datPhong.NgayTraPhong - datPhong.NgayNhanPhong).Days;
                if (soNgayO <= 0) soNgayO = 1;

                decimal tongTienThucTe = datPhong.TongTien;
                decimal giaGoc = (datPhong.Phong?.GiaPhong ?? 0) * soNgayO;

                string subject = $"[{datPhong.Phong?.KhachSan?.TenKhachSan}] Xác nhận đơn đặt phòng thành công";

                string extraInfo = $@"
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Số khách lưu trú:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>{datPhong.SoNguoi} người</td></tr>
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Giờ nhận phòng:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>Từ 14:00 - {datPhong.NgayNhanPhong:dd/MM/yyyy}</td></tr>
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Giờ trả phòng:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>Trước 12:00 - {datPhong.NgayTraPhong:dd/MM/yyyy} ({soNgayO} đêm)</td></tr>
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Giá phòng gốc:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef; text-decoration: line-through; color: #6c757d;'>{giaGoc:N0} VNĐ</td></tr>";

                if (datPhong.TienGiam > 0)
                {
                    extraInfo += $@"<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Ưu đãi hạng thẻ:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef; color: #dc3545; font-weight: bold;'>-{datPhong.TienGiam:N0} VNĐ</td></tr>";
                }

                extraInfo += $@"
<tr><td style='padding: 12px 0 0 0; color: #333;'><b>Tổng thanh toán:</b></td><td style='padding: 12px 0 0 0; color: #198754; font-weight: bold; font-size: 16px;'>{tongTienThucTe:N0} VNĐ</td></tr>";

                string body = TaoNoiDungEmail(
                    datPhong,
                    mauChuDao: "#007bff",
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

            // 🌟 SỬA TẠI ĐÂY: Thanh toán xong đang ở thì là Đang sử dụng
            if (datPhong.Phong != null)
            {
                datPhong.Phong.TrangThai = "Đang sử dụng";
                _context.Update(datPhong.Phong);
            }

            // 🌟 LOGIC TÍCH ĐIỂM TẠI QUẦY LỄ TÂN (GIỮ NGUYÊN)
            int soNgayO = (datPhong.NgayTraPhong - datPhong.NgayNhanPhong).Days;
            if (soNgayO <= 0) soNgayO = 1;

            decimal tongTienThucTe = datPhong.TongTien;
            decimal giaGoc = (datPhong.Phong?.GiaPhong ?? 0) * soNgayO;

            int diemDuocCong = 0;
            if (datPhong.TaiKhoan != null)
            {
                diemDuocCong = (int)(tongTienThucTe / SO_TIEN_QUY_DOI_DIEM);
                datPhong.TaiKhoan.DiemTichLuy += diemDuocCong;
            }

            _context.Update(datPhong);
            await _context.SaveChangesAsync();

            await _thangHangHelper.KiemTraVaNangHangAsync(datPhong.TaiKhoan);

            // 📧 GỬI EMAIL THANH TOÁN KÈM THÔNG TIN ĐIỂM THƯỞNG (GIỮ NGUYÊN)
            if (!string.IsNullOrEmpty(datPhong.TaiKhoan?.Email))
            {
                string subject = $"[{datPhong.Phong?.KhachSan?.TenKhachSan}] Biên lai thanh toán điện tử";

                string extraInfo = $@"
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Thời gian lưu trú:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>{datPhong.NgayNhanPhong:dd/MM/yyyy} đến {datPhong.NgayTraPhong:dd/MM/yyyy} ({soNgayO} đêm)</td></tr>
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Giá phòng gốc:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef; text-decoration: line-through; color: #6c757d;'>{giaGoc:N0} VNĐ</td></tr>";

                if (datPhong.TienGiam > 0)
                {
                    extraInfo += $@"<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Ưu đãi hạng thẻ:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef; color: #dc3545; font-weight: bold;'>-{datPhong.TienGiam:N0} VNĐ</td></tr>";
                }

                extraInfo += $@"
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Tổng thanh toán:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef; color: #198754; font-weight: bold; font-size: 16px;'>{tongTienThucTe:N0} VNĐ</td></tr>
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Điểm thưởng tích lũy:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef; color: #ffc107; font-weight: bold;'>+ {diemDuocCong} điểm</td></tr>
<tr><td style='padding: 12px 0 0 0;'><b>Trạng thái đơn:</b></td><td style='padding: 12px 0 0 0;'><span style='background-color: #198754; color: white; padding: 4px 10px; border-radius: 4px; font-size: 12px; font-weight: bold;'>XÁC NHẬN ĐÃ THANH TOÁN</span></td></tr>";

                string body = TaoNoiDungEmail(
                    datPhong,
                    mauChuDao: "#198754",
                    tieuDe: "BIÊN LAI THANH TOÁN",
                    loiNhanChinh: "Chúng tôi xin gửi thông báo xác nhận: Khách sạn đã nhận được đầy đủ khoản thanh toán cho đơn đặt phòng của Quý khách. Dưới đây là biên lai điện tử chi tiết:",
                    thongTinBoSung: extraInfo,
                    loiChaoKet: "Sự ưu ái của Quý khách là niềm vinh hạnh của chúng tôi. Kính chúc Quý khách có một kỳ nghỉ thật tuyệt vời và trọn vẹn!"
                );

                await SendEmailAsync(datPhong.TaiKhoan.Email, subject, body);
            }

            TempData["SuccessMessage"] = $"Đã thanh toán! Khách hàng được tích lũy thêm {diemDuocCong} điểm.";
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

                // ❌ SỬA TẠI ĐÂY: XÓA ĐOẠN RESET TRẠNG THÁI PHÒNG VỀ CÒN TRỐNG
                // Vì phòng chưa bao giờ bị khóa vật lý, nên ta không cần nhúng tay vào trạng thái vật lý của phòng nữa.

                _context.Update(datPhong);
                await _context.SaveChangesAsync();

                // 📧 GỬI EMAIL HỦY ĐƠN (GIỮ NGUYÊN)
                if (!string.IsNullOrEmpty(datPhong.TaiKhoan?.Email))
                {
                    string subject = $"[{datPhong.Phong?.KhachSan?.TenKhachSan}] Thông báo hủy đơn đặt phòng";
                    string extraInfo = $@"
<tr><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'><b>Thời gian dự kiến:</b></td><td style='padding: 8px 0; border-bottom: 1px dashed #e9ecef;'>{datPhong.NgayNhanPhong:dd/MM/yyyy} - {datPhong.NgayTraPhong:dd/MM/yyyy}</td></tr>
<tr><td style='padding: 12px 0 0 0;'><b>Trạng thái:</b></td><td style='padding: 12px 0 0 0;'><span style='background-color: #dc3545; color: white; padding: 4px 10px; border-radius: 4px; font-size: 12px; font-weight: bold;'>ĐÃ HỦY</span></td></tr>";

                    string body = TaoNoiDungEmail(
                        datPhong,
                        mauChuDao: "#dc3545",
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

            return RedirectToAction(nameof(LichSuDatPhong));
        }

        [HttpGet]
        public async Task<IActionResult> XuatHoaDonPDF(int id)
        {
            // 1. Lấy dữ liệu đặt phòng từ database (Giữ nguyên)
            var datPhong = await _context.DatPhong
                .Include(dp => dp.TaiKhoan)
                    .ThenInclude(t => t.HangThanhVien)
                .Include(dp => dp.Phong)
                    .ThenInclude(p => p.KhachSan)
                .FirstOrDefaultAsync(dp => dp.ID == id);

            if (datPhong == null)
                return NotFound();

            int soNgayO = (datPhong.NgayTraPhong - datPhong.NgayNhanPhong).Days;
            if (soNgayO <= 0) soNgayO = 1;

            decimal giaGoc = (datPhong.Phong?.GiaPhong ?? 0) * soNgayO;
            decimal tienGiam = datPhong.TienGiam;
            decimal tongTienThucTe = datPhong.TongTien;
            string tenHang = datPhong.TaiKhoan?.HangThanhVien?.TenHang ?? "Thành viên mới";

            // --- Đặt tên file có ngày giờ xuất (Giữ nguyên) ---
            string fileName = $"HoaDon_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            // --- Cấu hình đường dẫn Font chữ (Giữ nguyên) ---
            string fontFileName = "arial.ttf";
            string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", fontFileName);

            if (!System.IO.File.Exists(fontPath))
            {
                return Content($"Không tìm thấy font: {fontPath}. Hãy thêm file arial.ttf vào thư mục wwwroot/fonts.");
            }

            // =========================================================================
            // THAY ĐỔI CHÍNH: Thay thế khối FileStream (ổ D) bằng MemoryStream (giống Excel)
            // =========================================================================
            using (var stream = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 50, 50, 50, 50);

                // Trỏ PdfWriter vào 'stream' thay vì file cứng trên server
                PdfWriter.GetInstance(doc, stream);
                doc.Open();

                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                Font titleFont = new Font(bf, 20, Font.BOLD, new BaseColor(0, 102, 204));
                Font blackFont = new Font(bf, 12, Font.NORMAL, new BaseColor(0, 0, 0));
                Font boldFont = new Font(bf, 12, Font.BOLD, new BaseColor(0, 102, 204));
                Font redFont = new Font(bf, 12, Font.BOLD, new BaseColor(220, 53, 69));
                Font greenFont = new Font(bf, 14, Font.BOLD, new BaseColor(25, 135, 84));

                // --- Tiêu đề ---
                var title = new Paragraph("HÓA ĐƠN ĐẶT PHÒNG", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };
                doc.Add(title);

                // --- Ngày xuất ---
                var ngayXuat = new Paragraph($"Mã đơn: #{datPhong.ID} | Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}", blackFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingAfter = 15f
                };
                doc.Add(ngayXuat);

                // --- Thông tin khách hàng ---
                doc.Add(new Paragraph("THÔNG TIN KHÁCH HÀNG", boldFont) { SpacingAfter = 5f });
                doc.Add(new Paragraph($"Họ và tên: {datPhong.TaiKhoan?.HoVaTen ?? "Không rõ"}", blackFont));
                doc.Add(new Paragraph($"SĐT: {datPhong.TaiKhoan?.SoDienThoai ?? "Không có"}", blackFont));
                doc.Add(new Paragraph($"Email: {datPhong.TaiKhoan?.Email ?? "Không có"}", blackFont));
                doc.Add(new Paragraph($"Hạng thẻ: {tenHang}", boldFont));
                doc.Add(new Paragraph(" ", blackFont));

                // --- Thông tin phòng & khách sạn ---
                doc.Add(new Paragraph("THÔNG TIN PHÒNG & KHÁCH SẠN", boldFont) { SpacingAfter = 5f });
                doc.Add(new Paragraph($"Khách sạn: {datPhong.Phong?.KhachSan?.TenKhachSan ?? "Không rõ"}", blackFont));
                doc.Add(new Paragraph($"Số phòng: {(datPhong.Phong?.SoPhong.ToString() ?? "")}", blackFont));
                doc.Add(new Paragraph($"Giá phòng / đêm: {(datPhong.Phong?.GiaPhong ?? 0).ToString("N0", CultureInfo.InvariantCulture)} VNĐ", blackFont));
                doc.Add(new Paragraph(" ", blackFont));

                // --- Thời gian lưu trú ---
                doc.Add(new Paragraph("THỜI GIAN LƯU TRÚ", boldFont) { SpacingAfter = 5f });
                doc.Add(new Paragraph($"Ngày nhận: {datPhong.NgayNhanPhong:dd/MM/yyyy}", blackFont));
                doc.Add(new Paragraph($"Ngày trả: {datPhong.NgayTraPhong:dd/MM/yyyy}", blackFont));
                doc.Add(new Paragraph($"Tổng số đêm: {soNgayO} đêm", blackFont));
                doc.Add(new Paragraph(" ", blackFont));

                // --- Chi tiết thanh toán ---
                doc.Add(new Paragraph("CHI TIẾT THANH TOÁN", boldFont) { SpacingAfter = 5f });
                doc.Add(new Paragraph($"Tạm tính: {giaGoc.ToString("N0", CultureInfo.InvariantCulture)} VNĐ", blackFont));

                if (tienGiam > 0)
                {
                    doc.Add(new Paragraph($"Ưu đãi hạng ({tenHang}): -{tienGiam.ToString("N0", CultureInfo.InvariantCulture)} VNĐ", redFont));
                }

                // --- Tổng tiền ---
                var tongTienPara = new Paragraph($"TỔNG TIỀN PHẢI TRẢ: {tongTienThucTe.ToString("N0", CultureInfo.InvariantCulture)} VNĐ", greenFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 15f,
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

                // Đóng document để hoàn tất ghi dữ liệu vào MemoryStream
                doc.Close();

                // Chuyển đổi dữ liệu trong RAM thành mảng byte
                var content = stream.ToArray();

                // Trả về file trực tiếp về phía Client (Kích hoạt trình tải xuống giống như Excel)
                return File(content, "application/pdf", fileName);
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // 1. Lấy thông tin từ cấu hình hoặc Render Environment
                string senderEmail = _configuration["EmailConfig:SenderEmail"] ?? Environment.GetEnvironmentVariable("EMAIL");
                string apiKey = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(apiKey))
                {
                    TempData["ErrorMessage"] = "Lỗi hệ thống: Không tìm thấy tài khoản Email hoặc API Key!";
                    Console.WriteLine("LỖI: Thiếu biến môi trường EMAIL hoặc EMAIL_PASSWORD");
                    return false;
                }

                // 2. Sử dụng HttpClient để gọi API qua cổng bảo mật HTTPS (Vượt rào Render)
                using (var client = new System.Net.Http.HttpClient())
                {
                    // Gắn chìa khóa API vào Header
                    client.DefaultRequestHeaders.Add("api-key", apiKey);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    // Đóng gói dữ liệu Email theo đúng chuẩn của Brevo
                    var emailData = new
                    {
                        sender = new { name = "Hệ thống Đặt phòng Khách sạn", email = senderEmail },
                        to = new[] { new { email = toEmail } },
                        subject = subject,
                        htmlContent = body
                    };

                    // Chuyển dữ liệu sang chuỗi JSON
                    string jsonContent = System.Text.Json.JsonSerializer.Serialize(emailData);
                    var content = new System.Net.Http.StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                    // Bấm nút gửi lên máy chủ Brevo
                    var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("THÀNH CÔNG: Đã gửi email qua Brevo API!");
                        return true;
                    }
                    else
                    {
                        // Bắt lỗi chi tiết nếu gửi thất bại
                        string errorResult = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"LỖI TỪ BREVO: {response.StatusCode} - {errorResult}");
                        TempData["ErrorMessage"] = "Lỗi cấu hình gửi mail: " + response.StatusCode;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi gọi API gửi mail: " + ex.Message;
                Console.WriteLine("LỖI GỬI EMAIL API (Ngoại lệ): " + ex.Message);
                return false;
            }
        }

        private string TaoNoiDungEmail(DoAn_HotelBooking.Models.DatPhong datPhong, string mauChuDao, string tieuDe, string loiNhanChinh, string thongTinBoSung, string loiChaoKet)
        {
            string tenKhachSan = datPhong.Phong?.KhachSan?.TenKhachSan ?? "Hệ thống Khách sạn";
            string tenKhachHang = datPhong.TaiKhoan?.HoVaTen ?? "Quý khách";
            string soPhong = datPhong.Phong?.SoPhong.ToString() ?? "Chưa rõ";

            // 🌟 BỔ SUNG: Đọc tên hạng thành viên của khách hàng để tôn vinh trong Email
            string tenHang = datPhong.TaiKhoan?.HangThanhVien?.TenHang ?? "Thành viên mới";

            // Tạo mã đặt phòng chuyên nghiệp (VD: BK001024)
            string maDatPhong = "BK" + datPhong.ID.ToString("D6");

            // Lấy thông tin liên hệ của Khách sạn
            string diaChi = datPhong.Phong?.KhachSan?.DiaChi ?? "Việt Nam";
            string sdt = datPhong.Phong?.KhachSan?.SoDienThoai ?? "Hotline: 1900 xxxx";

            return $@"
<div style='font-family: ""Segoe UI"", Tahoma, Arial, sans-serif; line-height: 1.6; color: #333; max-width: 650px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.05);'>
    
    <div style='background-color: {mauChuDao}; padding: 30px 20px; text-align: center;'>
        <h1 style='color: #ffffff; margin: 0; font-size: 24px; text-transform: uppercase; letter-spacing: 1px;'>{tenKhachSan}</h1>
        <p style='color: rgba(255,255,255,0.85); margin: 8px 0 0 0; font-size: 15px; font-weight: 500;'>{tieuDe}</p>
    </div>
    
    <div style='padding: 30px 25px;'>
        <p style='font-size: 16px; margin-top: 0;'>Kính gửi Quý khách <b>{tenKhachHang}</b> <span style='font-size: 12px; background-color: rgba(255,215,0,0.15); color: #d4af37; padding: 3px 10px; border-radius: 20px; margin-left: 6px; border: 1px solid #d4af37; font-weight: bold;'>🎖️ Hạng {tenHang}</span>,</p>
        <p style='font-size: 15px;'>{loiNhanChinh}</p>
        
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