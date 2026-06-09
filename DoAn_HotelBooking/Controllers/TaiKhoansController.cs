using ClosedXML.Excel;
using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Helper;
using DoAn_HotelBooking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DoAn_HotelBooking.Controllers
{
    public class TaiKhoansController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;

        public TaiKhoansController(DoAn_HotelBookingContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> DoiMatKhauJson([FromBody] DoiMatKhauRequest model)
        {
            if (model == null)
                return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ!" });

            try
            {
                var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap);
                if (taiKhoan == null)
                    return Json(new { success = false, message = "Không tìm thấy tài khoản!" });

                // 🔐 Kiểm tra mật khẩu cũ
                if (!PasswordHelper.VerifyPassword(model.MatKhauCu, taiKhoan.MatKhau))
                    return Json(new { success = false, message = "Mật khẩu cũ không đúng!" });

                // 🧠 Kiểm tra trùng với mật khẩu cũ
                if (PasswordHelper.VerifyPassword(model.MatKhauMoi, taiKhoan.MatKhau))
                    return Json(new { success = false, message = "Mật khẩu mới không được trùng với mật khẩu cũ!" });

                // ✅ Hash và lưu mật khẩu mới
                taiKhoan.MatKhau = PasswordHelper.HashPassword(model.MatKhauMoi);
                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // GET: TaiKhoans
        public async Task<IActionResult> Index(string? maKhachSan)
        {
            var quyenHan = HttpContext.Session.GetString("QuyenHan");
            var maKhachSanUser = HttpContext.Session.GetString("MaKhachSan");

            var query = _context.TaiKhoan
                .Include(t => t.KhachSan)
                .AsQueryable();

            // ✅ Nếu là Admin → xem tất cả hoặc lọc theo khách sạn cụ thể
            if (quyenHan == "Admin")
            {
                if (!string.IsNullOrEmpty(maKhachSan))
                {
                    // 🔹 Admin xem tài khoản trong 1 khách sạn → chỉ hiển thị nhân viên & quản lý của KS đó
                    query = query.Where(t =>
                        t.MaKhachSan == maKhachSan &&
                        t.QuyenHan != "Khách hàng");

                    var ks = await _context.KhachSan
                        .FirstOrDefaultAsync(k => k.MaKhachSan == maKhachSan);

                    ViewBag.CurrentKhachSan = ks?.TenKhachSan ?? "Không xác định";
                    ViewBag.MaKhachSan = maKhachSan;
                    ViewBag.FromHotel = true;
                }
                else
                {
                    // 🔹 Admin xem tất cả → hiển thị mọi tài khoản
                    ViewBag.CurrentKhachSan = "Tất cả khách sạn";
                    ViewBag.FromHotel = false;
                }
            }
            else
            {
                // ✅ Nếu là Quản lý hoặc Nhân viên khách sạn
                if (!string.IsNullOrEmpty(maKhachSanUser))
                {
                    // 🔹 Quản lý/nhân viên chỉ xem tài khoản cùng khách sạn + khách hàng (chung hệ thống)
                    query = query.Where(t =>
                        t.MaKhachSan == maKhachSanUser ||
                        (t.QuyenHan == "Khách hàng" && t.MaKhachSan == null));

                    var ks = await _context.KhachSan
                        .FirstOrDefaultAsync(k => k.MaKhachSan == maKhachSanUser);

                    ViewBag.CurrentKhachSan = ks?.TenKhachSan ?? "Không xác định";
                    ViewBag.MaKhachSan = maKhachSanUser;
                    ViewBag.FromHotel = true;
                }
                else
                {
                    // ✅ Nếu là khách hàng (không thuộc khách sạn nào) → chặn truy cập
                    TempData["ErrorMessage"] = "⚠️ Bạn không có quyền xem danh sách tài khoản.";
                    return RedirectToAction("Index", "Home");
                }
            }

            var accounts = await query.ToListAsync();
            ViewBag.CurrentRole = quyenHan;
            return View(accounts);
        }

        // GET: TaiKhoans/Details/5
        public async Task<IActionResult> DetailsJson(int id)
        {
            var taiKhoan = await _context.TaiKhoan
                .Include(t => t.KhachSan)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (taiKhoan == null)
                return NotFound();

            return Json(new
            {
                hoVaTen = taiKhoan.HoVaTen,
                tenDangNhap = taiKhoan.TenDangNhap,
                email = taiKhoan.Email,
                soDienThoai = taiKhoan.SoDienThoai,
                quyenHan = taiKhoan.QuyenHan,
                khachSan = taiKhoan.KhachSan?.TenKhachSan
            });
        }

        // GET: TaiKhoans/Create
        public IActionResult Create(string maKhachSan, string? returnUrl)
        {
            if (!string.IsNullOrEmpty(maKhachSan))
            {
                // Tìm khách sạn
                var ks = _context.KhachSan.FirstOrDefault(x => x.MaKhachSan == maKhachSan);
                if (ks != null)
                {
                    ViewBag.DefaultKhachSan = ks; // gửi sang View
                }
            }

            ViewData["MaKhachSan"] = new SelectList(_context.KhachSan, "MaKhachSan", "TenKhachSan");

            // ✅ Lưu returnUrl để View dùng
            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "TaiKhoans");
            return View();
        }

        // POST: TaiKhoans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoan taiKhoan, string? returnUrl)
        {
            // Bỏ qua lỗi validation cho ID vì đây là tạo mới (ID tự tăng)
            ModelState.Remove("ID");

            if (ModelState.IsValid)
            {
                try
                {
                    string tenDangNhapMoi = taiKhoan.TenDangNhap?.Trim().ToLower();

                    // ✅ Kiểm tra trùng tên đăng nhập
                    if (_context.TaiKhoan.Any(x => x.TenDangNhap.Trim().ToLower() == tenDangNhapMoi))
                    {
                        ModelState.AddModelError("TenDangNhap", "⚠️ Tên đăng nhập đã tồn tại.");
                        return ReloadViewWithError(taiKhoan, returnUrl);
                    }

                    // ✅ Kiểm tra email trùng
                    if (!string.IsNullOrEmpty(taiKhoan.Email) &&
                        _context.TaiKhoan.Any(x => x.Email.Trim().ToLower() == taiKhoan.Email.Trim().ToLower()))
                    {
                        ModelState.AddModelError("Email", "⚠️ Email này đã được sử dụng.");
                        return ReloadViewWithError(taiKhoan, returnUrl);
                    }

                    // ✅ Hash mật khẩu
                    taiKhoan.MatKhau = PasswordHelper.HashPassword(taiKhoan.MatKhau);

                    // ✅ Lưu DB
                    _context.Add(taiKhoan);
                    await _context.SaveChangesAsync();

                    // ✅ Gửi thông báo thành công qua TempData
                    TempData["SuccessMessage"] = "✅ Tạo tài khoản thành công!";

                    // ✅ Sau khi tạo xong → quay lại returnUrl
                    if (!string.IsNullOrEmpty(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction(nameof(Index), "TaiKhoans");
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "❌ Lỗi khi lưu dữ liệu: " + (ex.InnerException?.Message ?? ex.Message));
                }
            }

            // ❌ Nếu có lỗi validation chung → load lại form Create
            return ReloadViewWithError(taiKhoan, returnUrl);
        }
      
        // GET: TaiKhoans/Edit/5
        public async Task<IActionResult> Edit(int? id, string? returnUrl, string? maKhachSan)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoan.FindAsync(id);
            if (taiKhoan == null) return NotFound();

            ViewData["MaKhachSan"] = new SelectList(
                _context.KhachSan,
                "MaKhachSan",
                "TenKhachSan",
                taiKhoan.MaKhachSan
            );

            // ✅ Xác định ReturnUrl
            if (!string.IsNullOrEmpty(returnUrl))
                ViewBag.ReturnUrl = returnUrl;
            else if (!string.IsNullOrEmpty(maKhachSan))
                ViewBag.ReturnUrl = Url.Action("Index", "TaiKhoans", new { maKhachSan });
            else
                ViewBag.ReturnUrl = Url.Action("Index", "TaiKhoans");

            return View(taiKhoan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaiKhoan taiKhoan, string? returnUrl)
        {
            if (id != taiKhoan.ID)
                return NotFound();

            // ✅ Bỏ validation cho mật khẩu nếu người dùng không nhập
            if (string.IsNullOrWhiteSpace(taiKhoan.MatKhau))
            {
                ModelState.Remove("MatKhau");
            }

            // ✅ Bỏ validation cho Khách sạn vì chúng ta không cho phép thay đổi
            ModelState.Remove("MaKhachSan");

            if (ModelState.IsValid)
            {
                var existing = await _context.TaiKhoan.FindAsync(id);
                if (existing == null) return NotFound();

                // ✅ Kiểm tra trùng tên đăng nhập
                string tenDangNhapMoi = taiKhoan.TenDangNhap?.Trim().ToLower();
                if (_context.TaiKhoan.Any(x => x.ID != id && x.TenDangNhap.Trim().ToLower() == tenDangNhapMoi))
                {
                    ModelState.AddModelError("TenDangNhap", "⚠️ Tên đăng nhập đã tồn tại.");
                    return ReloadViewWithError(taiKhoan, returnUrl);
                }

                // ✅ Kiểm tra trùng email
                if (!string.IsNullOrEmpty(taiKhoan.Email) &&
                    _context.TaiKhoan.Any(x => x.ID != id && x.Email.Trim().ToLower() == taiKhoan.Email.Trim().ToLower()))
                {
                    ModelState.AddModelError("Email", "⚠️ Email đã được sử dụng.");
                    return ReloadViewWithError(taiKhoan, returnUrl);
                }

                // ✅ Cập nhật dữ liệu (BỎ GÁN MaKhachSan ĐỂ GIỮ NGUYÊN DỮ LIỆU CŨ)
                existing.HoVaTen = taiKhoan.HoVaTen;
                existing.TenDangNhap = taiKhoan.TenDangNhap;
                existing.Email = taiKhoan.Email;
                existing.SoDienThoai = taiKhoan.SoDienThoai;
                existing.QuyenHan = taiKhoan.QuyenHan;
                // Xóa dòng: existing.MaKhachSan = taiKhoan.MaKhachSan;

                // ✅ Nếu nhập mật khẩu mới → hash lại, còn trống thì giữ nguyên
                if (!string.IsNullOrWhiteSpace(taiKhoan.MatKhau))
                {
                    existing.MatKhau = PasswordHelper.HashPassword(taiKhoan.MatKhau);
                }

                try
                {
                    _context.Update(existing);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "✏️ Cập nhật tài khoản thành công!";

                    if (!string.IsNullOrEmpty(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "❌ Lỗi khi lưu: " + (ex.InnerException?.Message ?? ex.Message));
                }
            }

            // Nếu validation chung (ModelState.IsValid = false) thất bại
            return ReloadViewWithError(taiKhoan, returnUrl);
        }

        // POST: TaiKhoans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string? returnUrl, string? maKhachSan)
        {
            var taiKhoan = await _context.TaiKhoan.FindAsync(id);
            if (taiKhoan != null)
            {
                _context.TaiKhoan.Remove(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "🗑️ Xóa tài khoản thành công!";
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            if (!string.IsNullOrEmpty(maKhachSan))
                return RedirectToAction(nameof(Index), new { maKhachSan });

            return RedirectToAction(nameof(Index));
        }

        private bool TaiKhoanExists(int id)
        {
            return _context.TaiKhoan.Any(e => e.ID == id);
        }

        // Xuất Excel
        public IActionResult ExportExcel(string role = "", string? maKhachSan = null)
        {
            // ✅ 1. Lấy người dùng hiện tại
            var currentUsername = HttpContext.Session.GetString("TenDangNhap");
            var currentUser = _context.TaiKhoan
                .Include(t => t.KhachSan)
                .FirstOrDefault(t => t.TenDangNhap == currentUsername);

            if (currentUser == null)
                return Content("Không xác định được tài khoản hiện tại.");

            // ✅ 2. Truy vấn dữ liệu (Giữ nguyên logic lọc)
            IQueryable<TaiKhoan> query = _context.TaiKhoan.Include(t => t.KhachSan);
            if (currentUser.QuyenHan == "Admin")
            {
                if (!string.IsNullOrEmpty(maKhachSan))
                    query = query.Where(x => x.MaKhachSan == maKhachSan);
            }
            else
            {
                if (currentUser.MaKhachSan != null)
                {
                    query = query.Where(x =>
                        (x.MaKhachSan == currentUser.MaKhachSan) ||
                        (x.QuyenHan == "Khách hàng" && x.MaKhachSan == null)
                    );
                }
            }

            if (!string.IsNullOrEmpty(role) && role != "All")
                query = query.Where(x => x.QuyenHan == role);

            var accounts = query.ToList();

            // ✅ 3. Xử lý logic chia Sheet
            using (var workbook = new XLWorkbook())
            {
                List<string> rolesToExport = new List<string>();
                if (role == "All" || string.IsNullOrEmpty(role))
                {
                    if (currentUser.QuyenHan == "Admin")
                        rolesToExport = new List<string> { "Admin", "Quản lý", "Nhân viên", "Khách hàng" };
                    else
                        rolesToExport = new List<string> { "Quản lý", "Nhân viên", "Khách hàng" };
                }
                else
                {
                    rolesToExport.Add(role);
                }

                foreach (var r in rolesToExport)
                {
                    var dataForSheet = accounts.Where(x => x.QuyenHan == r).ToList();
                    var worksheet = workbook.Worksheets.Add(r);

                    // 🧱 Định nghĩa Header động
                    var columns = new List<string> { "Họ và tên", "Email", "Số điện thoại", "Tên đăng nhập" };

                    if (r != "Khách hàng" && r != "Admin")
                    {
                        if (currentUser.QuyenHan == "Admin")
                        {
                            columns.Add("Khách sạn");
                        }
                    }
                    // Cột "Quyền hạn" đã được xóa bỏ hoàn toàn tại đây

                    // Ghi Header
                    for (int i = 0; i < columns.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = columns[i];
                    }

                    var headerRange = worksheet.Range(1, 1, 1, columns.Count);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // 🧾 Ghi dữ liệu
                    int currentRow = 2;
                    foreach (var acc in dataForSheet)
                    {
                        int col = 1;
                        foreach (var colName in columns)
                        {
                            switch (colName)
                            {
                                case "Họ và tên": worksheet.Cell(currentRow, col).Value = acc.HoVaTen; break;
                                case "Email": worksheet.Cell(currentRow, col).Value = acc.Email; break;
                                case "Số điện thoại": worksheet.Cell(currentRow, col).Value = acc.SoDienThoai; break;
                                case "Tên đăng nhập": worksheet.Cell(currentRow, col).Value = acc.TenDangNhap; break;
                                case "Khách sạn": worksheet.Cell(currentRow, col).Value = acc.KhachSan?.TenKhachSan ?? "Không có"; break;
                            }
                            col++;
                        }
                        currentRow++;
                    }
                    worksheet.Columns().AdjustToContents();
                }

                // ✅ 4. Xuất file
                string roleLabel = (role == "All" || string.IsNullOrEmpty(role)) ? "TatCa" : role.Replace(" ", "_");
                string fileName = $"TaiKhoan_{roleLabel}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        // 💡 Hàm hỗ trợ xử lý khi form có lỗi
        private IActionResult ReloadViewWithError(TaiKhoan taiKhoan, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));

            // Gán trống mật khẩu để View không tự điền lại chuỗi mã hóa
            taiKhoan.MatKhau = "";

            return View(taiKhoan);
        }
    }
}