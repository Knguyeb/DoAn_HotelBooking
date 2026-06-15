using ClosedXML.Excel;
using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DoAn_HotelBooking.Controllers
{
    public class KhachSansController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;

        public KhachSansController(DoAn_HotelBookingContext context)
        {
            _context = context;
        }

        // GET: KhachSans
        public async Task<IActionResult> Index()
        {
            var khachSans = await _context.KhachSan.ToListAsync();

            // Tạo dictionary chứa dữ liệu đánh giá
            var danhGiaData = _context.DanhGiaKS
                .GroupBy(d => d.MaKhachSan)
                .Select(g => new
                {
                    MaKhachSan = g.Key,
                    DiemTrungBinh = g.Average(d => d.SoSao),
                    SoDanhGia = g.Count()
                })
                .ToDictionary(x => x.MaKhachSan, x => new { x.DiemTrungBinh, x.SoDanhGia });

            ViewBag.DanhGiaData = danhGiaData;

            return View(khachSans);
        }

        public async Task<IActionResult> BanDo()
        {
            var list = await _context.KhachSan
                .Where(k => k.ViDo != null && k.KinhDo != null)
                .ToListAsync();

            return View(list); // ✅ chú ý: không ghi "Map" trong View()
        }

        // GET: KhachSans/Details/5
        public async Task<IActionResult> Details(string id, string? returnUrl)
        {
            if (id == null) return NotFound();

            var khachSan = await _context.KhachSan
                .FirstOrDefaultAsync(m => m.MaKhachSan == id);
            if (khachSan == null) return NotFound();

            // ==============================================================
            // ✅ 1. LẤY 3 PHÒNG CÓ ĐIỂM ĐÁNH GIÁ CAO NHẤT
            // ==============================================================
            // Bước 1: Lấy toàn bộ phòng thuộc khách sạn, kèm theo bảng Đánh giá
            var danhSachPhong = await _context.Phong
                .Where(p => p.MaKhachSan == id)
                .Include(p => p.DanhGiaPhongs) // Phải Include để lấy được dữ liệu điểm
                .Include(p => p.DatPhongs)
                .ToListAsync();

            // Bước 2: Tính điểm trung bình ảo và gán vào thuộc tính
            foreach (var p in danhSachPhong)
            {
                p.TrungBinhSao = (p.DanhGiaPhongs != null && p.DanhGiaPhongs.Any())
                                    ? p.DanhGiaPhongs.Average(d => d.SoSao)
                                    : 0;
            }

            // Bước 3: Sắp xếp giảm dần theo điểm -> Cắt lấy 3 phòng đầu tiên
            var top3Phongs = danhSachPhong
                .OrderByDescending(p => p.TrungBinhSao)
                .Take(3)
                .ToList();

            ViewBag.Phongs = top3Phongs;

            // ==============================================================
            // ✅ 2. TÍNH ĐIỂM TRUNG BÌNH CỦA KHÁCH SẠN
            // ==============================================================
            var danhGiasKS = await _context.DanhGiaKS
                .Where(d => d.MaKhachSan == id)
                .ToListAsync();

            if (danhGiasKS.Any())
            {
                ViewBag.TrungBinhSao = Math.Round(danhGiasKS.Average(d => d.SoSao), 1);
                ViewBag.SoDanhGia = danhGiasKS.Count;
            }
            else
            {
                ViewBag.TrungBinhSao = 0;
                ViewBag.SoDanhGia = 0;
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(khachSan);
        }

        // GET: KhachSans/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: KhachSans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhachSan khachSan, List<IFormFile> HinhAnhFiles)
        {
            // Loại bỏ các trường không cần kiểm tra Validation
            ModelState.Remove("HinhAnh");
            ModelState.Remove("MaKhachSan");
            ModelState.Remove("TaiKhoans");
            ModelState.Remove("Phongs");
            ModelState.Remove("DanhGiaKhachSans");

            // Lấy tọa độ
            if (Request.Form.ContainsKey("ViDo"))
                khachSan.ViDo = Request.Form["ViDo"].ToString().Replace(",", ".").Trim();
            if (Request.Form.ContainsKey("KinhDo"))
                khachSan.KinhDo = Request.Form["KinhDo"].ToString().Replace(",", ".").Trim();

            // 1. Kiểm tra ảnh
            if (HinhAnhFiles == null || HinhAnhFiles.Count == 0)
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một hình ảnh khách sạn." });

            // 2. Kiểm tra trùng tên
            string tenMoi = khachSan.TenKhachSan?.Trim().ToLower();
            if (!string.IsNullOrEmpty(tenMoi) && _context.KhachSan.Any(x => x.TenKhachSan.Trim().ToLower() == tenMoi))
                return Json(new { success = false, message = "Tên khách sạn này đã tồn tại trong hệ thống." });

            // 3. Kiểm tra Validation chung
            if (!ModelState.IsValid)
            {
                // Trích xuất chính xác tên cái cột/thuộc tính nào đang bị lỗi ngầm
                var errorList = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => x.Key + ": " + x.Value.Errors.First().ErrorMessage)
                    .ToList();

                string detailErrors = string.Join("\n", errorList);

                // Trả thẳng lỗi chi tiết ra màn hình để SweetAlert hiển thị
                return Json(new { success = false, message = "Thông tin chưa hợp lệ:\n" + detailErrors });
            }

            try
            {
                // Khởi tạo mã Khách sạn ngẫu nhiên/tự tăng
                string newCode;
                do { newCode = KhachSan.GenerateMaKhachSan(); }
                while (_context.KhachSan.Any(x => x.MaKhachSan == newCode));
                khachSan.MaKhachSan = newCode;

                // Xử lý lưu ảnh
                var fileNames = new List<string>();
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in HinhAnhFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        fileNames.Add("/images/" + fileName);
                    }
                }

                khachSan.HinhAnh = string.Join(";", fileNames);

                _context.Add(khachSan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm khách sạn thành công!";

                // TRẢ VỀ JSON THÀNH CÔNG ĐỂ JS CHUYỂN TRANG
                return Json(new { success = true, redirectUrl = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // GET: KhachSans/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var khachSan = await _context.KhachSan.FindAsync(id);
            if (khachSan == null)
                return NotFound();

            return View(khachSan);
        }

        // POST: KhachSans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, KhachSan khachSan, List<IFormFile>? HinhAnhFiles, string ExistingImages)
        {
            // Loại bỏ các trường không cần thiết khỏi ModelState để tránh lỗi IsValid giả
            ModelState.Remove("HinhAnh");
            ModelState.Remove("MaKhachSan");
            ModelState.Remove("TaiKhoans");
            ModelState.Remove("Phongs");
            ModelState.Remove("DanhGiaKhachSans");

            // 1. Kiểm tra tính hợp lệ của dữ liệu (ModelState)
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ các thông tin bắt buộc (*)." });
            }

            // 2. Tìm khách sạn hiện tại trong DB
            var existing = await _context.KhachSan.FirstOrDefaultAsync(x => x.MaKhachSan == id);
            if (existing == null) return Json(new { success = false, message = "Không tìm thấy khách sạn cần cập nhật." });

            // 3. Xử lý tọa độ
            if (Request.Form.ContainsKey("ViDo"))
                khachSan.ViDo = Request.Form["ViDo"].ToString().Replace(",", ".").Trim();

            if (Request.Form.ContainsKey("KinhDo"))
                khachSan.KinhDo = Request.Form["KinhDo"].ToString().Replace(",", ".").Trim();

            // 4. Kiểm tra trùng tên (trừ khách sạn hiện tại)
            string tenMoi = khachSan.TenKhachSan?.Trim().ToLower();
            bool tenTonTai = await _context.KhachSan.AnyAsync(x =>
                x.MaKhachSan != id &&
                x.TenKhachSan.Trim().ToLower() == tenMoi);

            if (tenTonTai)
            {
                return Json(new { success = false, message = "⚠️ Tên khách sạn đã tồn tại." });
            }

            try
            {
                // 5. Cập nhật các thông tin cơ bản
                existing.TenKhachSan = khachSan.TenKhachSan;
                existing.DiaChi = khachSan.DiaChi;
                existing.ViDo = khachSan.ViDo;
                existing.KinhDo = khachSan.KinhDo;
                existing.SoDienThoai = khachSan.SoDienThoai;

                // 6. QUAN TRỌNG: Xử lý gộp ảnh CŨ và ảnh MỚI
                List<string> finalImages = new List<string>();

                // Lấy lại danh sách ảnh cũ được giữ lại từ input ẩn "ExistingImages"
                if (!string.IsNullOrEmpty(ExistingImages))
                {
                    finalImages = ExistingImages.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                // Xử lý lưu các tệp ảnh mới được tải lên
                if (HinhAnhFiles != null && HinhAnhFiles.Count > 0)
                {
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images");
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    foreach (var file in HinhAnhFiles)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadPath, fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            finalImages.Add("/Images/" + fileName);
                        }
                    }
                }

                // Cập nhật lại chuỗi đường dẫn ảnh cuối cùng
                existing.HinhAnh = string.Join(";", finalImages);

                _context.Update(existing);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✏️ Cập nhật khách sạn thành công!";
                return Json(new { success = true, redirectUrl = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // 1. Tìm khách sạn và Include toàn bộ phả hệ (Tài khoản, Đánh giá, Phòng và Lịch sử đặt phòng của từng phòng)
            var khachSan = await _context.KhachSan
                .Include(k => k.TaiKhoans)
                .Include(k => k.DanhGiaKhachSans)
                .Include(k => k.Phongs)
                    .ThenInclude(p => p.DatPhongs) // ⚠️ LƯU Ý: Thay "DatPhongs" bằng tên thuộc tính liên kết trong Model Phong của bạn (VD: ChiTietDatPhongs, HoaDons...)
                .FirstOrDefaultAsync(k => k.MaKhachSan == id);

            if (khachSan == null)
            {
                TempData["ErrorMessage"] = "❌ Không tìm thấy khách sạn cần xóa.";
                return RedirectToAction(nameof(Index));
            }

            // 2. KIỂM TRA NGHIỆP VỤ: Có phòng nào của khách sạn này đã từng được đặt chưa?
            bool daCoNguoiDat = khachSan.Phongs != null && khachSan.Phongs.Any(p => p.DatPhongs != null && p.DatPhongs.Any());

            if (daCoNguoiDat)
            {
                // NẾU CÓ: Lập tức quay xe, không cho xóa!
                TempData["ErrorMessage"] = "❌ Không thể xóa! Khách sạn này đã có dữ liệu đơn đặt phòng liên quan.";
                return RedirectToAction(nameof(Index));
            }

            // 3. NẾU KHÔNG CÓ ĐƠN ĐẶT PHÒNG: Tiến hành "thanh trừng" dữ liệu
            try
            {
                // 3.1 Xóa các tài khoản trực thuộc
                if (khachSan.TaiKhoans != null && khachSan.TaiKhoans.Any())
                {
                    _context.TaiKhoan.RemoveRange(khachSan.TaiKhoans);
                }

                // 3.2 Xóa các bài đánh giá
                if (khachSan.DanhGiaKhachSans != null && khachSan.DanhGiaKhachSans.Any())
                {
                    _context.DanhGiaKS.RemoveRange(khachSan.DanhGiaKhachSans);
                }

                // 3.3 Xóa tất cả các phòng
                if (khachSan.Phongs != null && khachSan.Phongs.Any())
                {
                    _context.Phong.RemoveRange(khachSan.Phongs);
                }

                // 3.4 Cuối cùng, xóa khách sạn (Trùm cuối)
                _context.KhachSan.Remove(khachSan);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Đã xóa khách sạn cùng toàn bộ phòng và tài khoản liên quan thành công!";
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "❌ Lỗi hệ thống khi xóa dữ liệu: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        // Xuất Excel
        public IActionResult ExportExcelAll()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DanhSachKhachSan");
                var currentRow = 1;

                // Header
                worksheet.Cell(currentRow, 1).Value = "Mã KS";
                worksheet.Cell(currentRow, 2).Value = "Tên khách sạn";
                worksheet.Cell(currentRow, 3).Value = "Địa chỉ";
                worksheet.Cell(currentRow, 4).Value = "Số điện thoại";

                // Data
                var ds = _context.KhachSan.ToList();
                foreach (var ks in ds)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = ks.MaKhachSan;
                    worksheet.Cell(currentRow, 2).Value = ks.TenKhachSan;
                    worksheet.Cell(currentRow, 3).Value = ks.DiaChi;
                    worksheet.Cell(currentRow, 4).Value = ks.SoDienThoai;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "DanhSachKhachSan.xlsx");
                }
            }
        }
    }
}