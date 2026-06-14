using ClosedXML.Excel;
using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn_HotelBooking.Controllers
{
    public class PhongsController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;

        public PhongsController(DoAn_HotelBookingContext context)
        {
            _context = context;
        }

        // GET: Phongs
        public async Task<IActionResult>Index(string id) // id = MaKhachSan
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var khachSan = await _context.KhachSan
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.MaKhachSan == id);

            if (khachSan == null) return NotFound();

            ViewBag.KhachSan = khachSan;
            ViewBag.MaKhachSan = khachSan.MaKhachSan;

            var phongs = await _context.Phong
            .Where(p => p.MaKhachSan == id)
            .OrderBy(p => p.SoPhong)
            .ToListAsync();

            return View(phongs);
        }

        // GET: Phongs/TatCaPhong
        public async Task<IActionResult> TatCaPhong(string? maKhachSan)
        {
            // Bắt đầu với câu truy vấn lấy tất cả phòng và bao gồm thông tin Khách sạn
            var query = _context.Phong.Include(p => p.KhachSan).AsQueryable();

            // KIỂM TRA MÃ KHÁCH SẠN TỪ NÚT BẤM
            if (!string.IsNullOrEmpty(maKhachSan))
            {
                // Lọc chỉ lấy phòng của khách sạn có mã tương ứng
                query = query.Where(p => p.MaKhachSan == maKhachSan);

                // Lấy thông tin khách sạn để truyền tên ra ViewBag (hiển thị lên banner)
                var ks = await _context.KhachSan.FirstOrDefaultAsync(k => k.MaKhachSan == maKhachSan);
                if (ks != null)
                {
                    ViewBag.TenKhachSan = ks.TenKhachSan;
                    ViewBag.MaKhachSan = ks.MaKhachSan;
                }
            }
            else
            {
                // Nếu không có mã khách sạn truyền vào, hiển thị tiêu đề chung
                ViewBag.TenKhachSan = "Tất cả hệ thống phòng";
            }

            // THỰC THI TRUY VẤN VÀ TRẢ VỀ VIEW
            var dsPhong = await query.OrderBy(p => p.SoPhong).ToListAsync();

            return View(dsPhong);
        }

        // GET: Phongs/DetailsPartial/5
        public async Task<IActionResult> DetailsPartial(int? id)
        {
            if (id == null) return NotFound();

            var phong = await _context.Phong
                .Include(p => p.KhachSan)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (phong == null) return NotFound();

            // Lấy thông tin User từ Session
            if (HttpContext.Session.GetString("UserID") != null)
            {
                ViewBag.MaTaiKhoan = int.Parse(HttpContext.Session.GetString("UserID"));
            }

            // Lấy danh sách đánh giá
            var danhGias = await _context.DanhGiaPhong
                .Where(d => d.MaPhong == id)
                .ToListAsync();

            // Tính toán số sao
            ViewBag.TrungBinhSao = danhGias.Any() ? Math.Round(danhGias.Average(d => d.SoSao), 1) : 0;
            ViewBag.SoDanhGia = danhGias.Count;

            // Trả về PartialView thay vì View
            return PartialView("_DetailsPartial", phong);
        }

        // GET: Phongs/Create
        public IActionResult Create(string id) // id = MaKhachSan
        {
            var ks = _context.KhachSan.Find(id);
            if (ks == null) return NotFound();

            var phong = new Phong
            {
                MaKhachSan = id // gán sẵn khóa ngoại
            };

            ViewBag.KhachSan = ks;
            return View(phong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Phong phong, List<IFormFile> HinhAnhFiles)
        {
            ModelState.Remove("KhachSan");
            ModelState.Remove("HinhAnh");
            ModelState.Remove("ID");
            ModelState.Remove("TrangThai");
            ModelState.Remove("DanhGiaPhongs");
            ModelState.Remove("DatPhongs");

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }

            // ❌ Check trùng
            if (_context.Phong.Any(x => x.MaKhachSan == phong.MaKhachSan && x.SoPhong == phong.SoPhong))
            {
                return Json(new { success = false, message = "Số phòng đã tồn tại!" });
            }

            if (HinhAnhFiles == null || HinhAnhFiles.Count == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất 1 ảnh!" });
            }

            try
            {
                phong.TrangThai = "Còn trống";

                var fileNames = new List<string>();
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in HinhAnhFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        fileNames.Add("/images/" + fileName);
                    }
                }

                phong.HinhAnh = string.Join(";", fileNames);

                _context.Add(phong);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm phòng thành công!";
                return RedirectToAction("Index", new { id = phong.MaKhachSan });
            }
            catch (Exception ex)
            {
                var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

                return Json(new
                {
                    success = false,
                    message = string.Join(" | ", errors)
                });
            }
        }

        // GET: Phongs/Edit/5
        public async Task<IActionResult> Edit(int? id, string returnUrl = null)
        {
            if (id == null) return NotFound();

            var phong = await _context.Phong.FindAsync(id);
            if (phong == null) return NotFound();

            ViewBag.KhachSan = await _context.KhachSan.FindAsync(phong.MaKhachSan);
            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", new { id = phong.MaKhachSan });

            return View(phong);
        }

        // POST: Phongs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Phong phong, List<IFormFile>? HinhAnhFiles, string ExistingImages)
        {
            // 1. Loại bỏ các trường không cần thiết khỏi ModelState
            ModelState.Remove("HinhAnh");
            ModelState.Remove("DatPhongs");
            ModelState.Remove("KhachSan");
            ModelState.Remove("DanhGiaPhongs"); // Thêm dòng này nếu bảng Phong có liên kết Đánh giá

            if (id != phong.ID)
                return Json(new { success = false, message = "ID phòng không hợp lệ." });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ các thông tin bắt buộc (*)." });

            // 2. Tìm phòng trong DB
            var existing = await _context.Phong.FirstOrDefaultAsync(p => p.ID == id);
            if (existing == null)
                return Json(new { success = false, message = "Không tìm thấy phòng cần cập nhật." });

            // 3. Kiểm tra trùng số phòng (trong cùng 1 khách sạn)
            bool isDuplicate = await _context.Phong.AnyAsync(p =>
                p.ID != id &&
                p.MaKhachSan == phong.MaKhachSan &&
                p.SoPhong == phong.SoPhong);

            if (isDuplicate)
                return Json(new { success = false, message = "⚠️ Số phòng này đã tồn tại trong khách sạn!" });

            try
            {
                // 4. Cập nhật thông tin cơ bản
                existing.SoPhong = phong.SoPhong;
                existing.Tang = phong.Tang;
                existing.LoaiPhong = phong.LoaiPhong;
                existing.SucChua = phong.SucChua;
                existing.GiaPhong = phong.GiaPhong;
                existing.TrangThai = phong.TrangThai;

                // 5. QUAN TRỌNG: Xử lý gộp ảnh CŨ và ảnh MỚI
                List<string> finalImages = new List<string>();

                // Lấy lại danh sách ảnh cũ được giữ lại từ input ẩn "ExistingImages"
                if (!string.IsNullOrEmpty(ExistingImages))
                {
                    finalImages = ExistingImages.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                // Xử lý lưu các tệp ảnh mới được tải lên
                if (HinhAnhFiles != null && HinhAnhFiles.Count > 0)
                {
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

                            finalImages.Add("/images/" + fileName);
                        }
                    }
                }

                // Cập nhật lại chuỗi đường dẫn ảnh cuối cùng
                existing.HinhAnh = string.Join(";", finalImages);

                _context.Update(existing);
                await _context.SaveChangesAsync();

                // 6. Trả về JSON thành công kèm URL để redirect
                TempData["SuccessMessage"] = "✏️ Cập nhật phòng thành công!";
                return Json(new { success = true, redirectUrl = Url.Action("Index", new { id = existing.MaKhachSan }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // POST: Phongs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phong = await _context.Phong.FindAsync(id);
            if (phong != null)
            {
                _context.Phong.Remove(phong);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "🗑️ Xóa phòng thành công!";
                return RedirectToAction(nameof(Index), new { id = phong.MaKhachSan });
            }

            return NotFound();
        }

        // Xuất Excel
        public IActionResult ExportExcel(string maKhachSan)
        {
            if (string.IsNullOrEmpty(maKhachSan))
                return Content("Vui lòng chọn khách sạn để xuất danh sách phòng.");

            var danhSachPhong = _context.Phong
                .Include(p => p.KhachSan)
                .Where(p => p.MaKhachSan == maKhachSan)
                .OrderBy(p => p.SoPhong)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DanhSachPhong");

                // Tiêu đề cột
                worksheet.Cell(1, 1).Value = "Số phòng";
                worksheet.Cell(1, 3).Value = "Loại phòng";
                worksheet.Cell(1, 4).Value = "Giá";
                worksheet.Cell(1, 5).Value = "Khách sạn";

                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = 2;
                foreach (var p in danhSachPhong)
                {
                    worksheet.Cell(row, 1).Value = p.SoPhong;
                    worksheet.Cell(row, 3).Value = p.LoaiPhong;
                    worksheet.Cell(row, 4).Value = p.GiaPhong;
                    worksheet.Cell(row, 5).Value = p.KhachSan?.TenKhachSan;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                string tenKS = danhSachPhong.FirstOrDefault()?.KhachSan?.TenKhachSan ?? "KhachSan";
                string fileName = $"DanhSachPhong_{tenKS}.xlsx";

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }
        }
    }
}