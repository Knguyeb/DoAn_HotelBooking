using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Models;
using DoAn_HotelBooking.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn_HotelBooking.Controllers
{
    public class DanhGiaPhongsController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;
        private readonly IAI_ReviewService _aiService;

        public DanhGiaPhongsController(DoAn_HotelBookingContext context, IAI_ReviewService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        [HttpGet]
        public async Task<IActionResult> TongHopAi(int maPhong)
        {
            // 1. Lấy tất cả đánh giá của mã phòng này từ DB
            var danhGias = await _context.DanhGiaPhong
                .Where(d => d.MaPhong == maPhong)
                .ToListAsync();

            // Nếu chưa có đánh giá nào, trả về View với Model rỗng
            if (danhGias == null || !danhGias.Any())
            {
                return PartialView("~/Views/Phongs/_TongHopAiPartial.cshtml", new AI_ReviewViewModel());
            }

            // 2. Tính Tỉ lệ yêu thích bằng LINQ (Số sao >= 4 là yêu thích)
            int tongSoDanhGia = danhGias.Count;
            int soDanhGiaTot = danhGias.Count(d => d.SoSao >= 4);
            double tiLeYeuThich = Math.Round((double)soDanhGiaTot / tongSoDanhGia * 100, 2);

            // 3. Gom bình luận dạng chữ (bỏ qua các đánh giá chỉ chấm sao, không viết chữ)
            var cacBinhLuan = danhGias
                .Where(d => !string.IsNullOrWhiteSpace(d.NoiDung))
                .Select(d => d.NoiDung)
                .ToList();

            // 4. Gọi Service AI phân tích Ưu/Nhược điểm
            var viewModel = await _aiService.AnalyzeReviewsAsync(cacBinhLuan);

            // 5. Gán thêm phần trăm yêu thích đã tính được
            viewModel.TiLeYeuThich = tiLeYeuThich;

            // 6. Trả Model này về cho file View tên là "TongHopAi.cshtml" hiển thị
            return PartialView("~/Views/Phongs/_TongHopAiPartial.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFromSwal([FromBody] DanhGiaPhong model)
        {
            // ✅ 1. Kiểm tra dữ liệu đầu vào (Bổ sung kiểm tra MaDatPhong)
            if (model == null || model.MaPhong <= 0 || model.MaDatPhong <= 0 || model.SoSao < 1 || model.SoSao > 5)
                return Json(new { success = false, errorMessage = "Dữ liệu không hợp lệ!" });

            // ✅ 2. Kiểm tra đăng nhập
            var maTaiKhoan = HttpContext.Session.GetInt32("ID");
            if (maTaiKhoan == null)
            {
                return Json(new { success = false, errorMessage = "Vui lòng đăng nhập để thực hiện đánh giá!" });
            }

            // ✅ 3. Xác thực quyền sở hữu đơn đặt phòng (Bảo mật: Tránh user fake MaDatPhong của người khác)
            // Lưu ý: Đảm bảo DbContext của bạn có DbSet<DatPhong> DatPhong
            bool donHangHopLe = await _context.DatPhong.AnyAsync(dp =>
                dp.ID == model.MaDatPhong &&
                dp.MaTaiKhoan == maTaiKhoan.Value &&
                dp.MaPhong == model.MaPhong); // Đảm bảo đơn đặt phòng này khớp đúng với mã phòng

            if (!donHangHopLe)
            {
                return Json(new { success = false, errorMessage = "Đơn đặt phòng không hợp lệ hoặc không thuộc về bạn!" });
            }

            // ✅ 4. KIỂM TRA ĐÁNH GIÁ TRÙNG LẶP (Dựa trên MaDatPhong)
            bool daDanhGia = await _context.DanhGiaPhong.AnyAsync(d => d.MaDatPhong == model.MaDatPhong);

            if (daDanhGia)
            {
                return Json(new { success = false, errorMessage = "Bạn đã đánh giá cho đơn đặt phòng này rồi!" });
            }

            // ✅ 5. Gán dữ liệu còn thiếu và lưu vào DB
            model.MaTaiKhoan = maTaiKhoan.Value;
            model.NgayTao = DateTime.UtcNow; // Model có default value, nhưng gán lại cho chắc chắn đúng múi giờ hiện tại

            _context.Add(model);
            await _context.SaveChangesAsync();

            // Trả về message khi thành công để Swal hiển thị icon success
            return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá phòng!" });
        }

        [HttpGet]
        public IActionResult GetBinhLuanByPhong(int maPhong)
        {
            try
            {
                // 1. Dùng .ToList() để kéo thẳng dữ liệu thô về RAM trước, CHỐNG MỌI LỖI SQL
                var dbData = _context.DanhGiaPhong
                    .Include(d => d.TaiKhoan)
                    .Where(d => d.MaPhong == maPhong)
                    .OrderByDescending(d => d.NgayTao)
                    .ToList();

                // 2. Chuyển đổi dữ liệu đơn giản
                var result = dbData.Select(d => new {
                    // ⚠️ LƯU Ý: Nếu bảng TaiKhoan của bạn không có cột "HoTen", hãy đổi chữ HoTen dưới đây thành "TenTaiKhoan" hoặc thuộc tính đúng của bạn.
                    tenKhachHang = d.TaiKhoan != null ? d.TaiKhoan.HoVaTen : "Ẩn danh",
                    ngayTao = d.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                    noiDung = d.NoiDung ?? "Không có bình luận"
                });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                // Nếu code backend chết, nó sẽ ném lỗi thẳng ra màn hình cho bạn xem
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }


        [HttpGet]
        // Đổi tham số từ maPhong sang maDatPhong (hoặc nhận cả 2 tùy thiết kế UI của bạn)
        // Mình khuyên dùng maDatPhong vì nó là định danh duy nhất cho việc "được phép đánh giá"
        public async Task<IActionResult> KiemTraDanhGia(int maDatPhong)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("ID");

            // 1. Kiểm tra đăng nhập
            if (maTaiKhoan == null)
                return Json(new { hopLe = false, message = "Vui lòng đăng nhập để đánh giá!" });

            // 2. Xác thực quyền sở hữu đơn đặt phòng
            bool donHangHopLe = await _context.DatPhong.AnyAsync(dp =>
                dp.ID == maDatPhong &&
                dp.MaTaiKhoan == maTaiKhoan.Value);

            if (!donHangHopLe)
                return Json(new { hopLe = false, message = "Đơn đặt phòng không tồn tại hoặc không thuộc về bạn!" });

            // 3. Kiểm tra xem đơn này đã đánh giá chưa
            bool daDanhGia = await _context.DanhGiaPhong.AnyAsync(d => d.MaDatPhong == maDatPhong);

            if (daDanhGia)
                return Json(new { hopLe = false, message = "Bạn đã đánh giá đơn đặt phòng này rồi!" });

            // 4. Nếu hợp lệ, cho phép mở form đánh giá
            return Json(new { hopLe = true });
        }
    }
}