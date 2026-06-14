using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Helper;
using DoAn_HotelBooking.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DoAn_HotelBooking.Controllers
{
    public class DangKy_DangNhapController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;
        private readonly ILogger<DangKy_DangNhapController> _logger;

        public DangKy_DangNhapController(DoAn_HotelBookingContext context, ILogger<DangKy_DangNhapController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult DangKy()
        {
            return View();
        }

        // ✅ Đăng nhập bằng Google
        public IActionResult DangNhapGoogle()
        {
            var redirectUrl = Url.Action("GoogleCallback", "DangKy_DangNhap");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // ✅ Callback từ Google sau khi đăng nhập thành công
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result?.Principal != null)
            {
                var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
                var email = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                var name = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;

                if (!string.IsNullOrEmpty(email))
                {
                    // 🔹 Kiểm tra tài khoản đã tồn tại chưa
                    var user = await _context.TaiKhoan.FirstOrDefaultAsync(x => x.Email == email);

                    if (user == null)
                    {
                        user = new TaiKhoan
                        {
                            HoVaTen = name,
                            TenDangNhap = email,
                            Email = email,
                            SoDienThoai = "0000000000",
                            QuyenHan = "Khách hàng",
                            MaKhachSan = null
                        };

                        _context.Add(user);
                        await _context.SaveChangesAsync();

                        // 🔹 Đảm bảo EF cập nhật ID (phòng khi có trigger DB)
                        await _context.Entry(user).ReloadAsync();
                    }

                    // 🔹 Debug ID để kiểm tra
                    Console.WriteLine($"[GoogleLogin] Đăng nhập thành công - ID: {user.ID}, Email: {user.Email}");

                    // ✅ Lưu Session
                    HttpContext.Session.SetInt32("ID", user.ID);
                    HttpContext.Session.SetString("TenDangNhap", user.TenDangNhap ?? "");
                    HttpContext.Session.SetString("HoVaTen", user.HoVaTen ?? "");
                    HttpContext.Session.SetString("QuyenHan", user.QuyenHan ?? "Khách hàng");

                    TempData["SuccessMessage"] = "✅ Đăng nhập Google thành công!";
                    return RedirectToAction("Index", "Home");
                }
            }

            TempData["ErrorMessage"] = "❌ Đăng nhập Google thất bại!";
            return RedirectToAction("DangNhap");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy([Bind("HoVaTen,TenDangNhap,MatKhau,Email,SoDienThoai")] TaiKhoan taiKhoan)
        {
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
                        return View("DangKy", taiKhoan); // quay lại form đăng ký
                    }

                    // ✅ Kiểm tra email trùng
                    if (!string.IsNullOrEmpty(taiKhoan.Email) &&
                        _context.TaiKhoan.Any(x => x.Email.Trim().ToLower() == taiKhoan.Email.Trim().ToLower()))
                    {
                        ModelState.AddModelError("Email", "⚠️ Email này đã được sử dụng.");
                        return View("DangKy", taiKhoan);
                    }

                    // ✅ Mã hóa mật khẩu bằng PBKDF2 + Salt
                    taiKhoan.MatKhau = PasswordHelper.HashPassword(taiKhoan.MatKhau);

                    // ✅ Mặc định quyền hạn là Khách hàng
                    taiKhoan.QuyenHan = "Khách hàng";

                    // ✅ Không liên kết với khách sạn
                    taiKhoan.MaKhachSan = null;

                    // ✅ Lưu DB
                    _context.Add(taiKhoan);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "✅ Đăng ký thành công!";
                    return RedirectToAction("DangNhap");
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "❌ Lỗi khi lưu dữ liệu: " + (ex.InnerException?.Message ?? ex.Message));
                }
            }

            return View("DangKy", taiKhoan);
        }

        [HttpGet]
        public IActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangNhap(TaiKhoan model)
        {
            string username = model.TenDangNhap?.Trim().ToLower();

            var user = _context.TaiKhoan
                .FirstOrDefault(x => x.TenDangNhap.Trim().ToLower() == username);

            if (user != null)
            {
                Console.WriteLine($"[DEBUG] DB Hash = {user.MatKhau}");
                Console.WriteLine($"[DEBUG] Input Password = {model.MatKhau}");
                bool ok = PasswordHelper.VerifyPassword(model.MatKhau, user.MatKhau);
                Console.WriteLine($"[DEBUG] Verify = {ok}");

                if (ok)
                {
                    HttpContext.Session.SetInt32("ID", user.ID);
                    HttpContext.Session.SetString("TenDangNhap", user.TenDangNhap);
                    HttpContext.Session.SetString("HoVaTen", user.HoVaTen);
                    HttpContext.Session.SetString("QuyenHan", user.QuyenHan);
                    HttpContext.Session.SetString("MaKhachSan", user.MaKhachSan ?? "");

                    // ✅ LẤY TÊN KHÁCH SẠN
                    if (!string.IsNullOrEmpty(user.MaKhachSan))
                    {
                        var khachSan = _context.KhachSan
                            .FirstOrDefault(k => k.MaKhachSan == user.MaKhachSan);

                        HttpContext.Session.SetString("TenKhachSan",
                            khachSan?.TenKhachSan ?? "Khách sạn");
                    }
                    else
                    {
                        HttpContext.Session.SetString("TenKhachSan", "Khách hàng");
                    }

                    // ✅ Thêm thông báo đăng nhập thành công
                    TempData["SuccessMessage"] = "✅ Đăng nhập thành công!";

                    return RedirectToAction("Index", "Home");
                }
            }

            TempData["ErrorMessage"] = "❌ Sai tên đăng nhập hoặc mật khẩu.";
            return View(model);
        }

        [HttpGet]
        public IActionResult DangXuat()
        {
            // Xóa session
            HttpContext.Session.Clear();

            // Gửi thông báo
            TempData["SuccessMessage"] = "👋 Bạn đã đăng xuất thành công!";

            // Quay về trang đăng nhập
            return RedirectToAction("Index", "Home");
        }
    }
}
