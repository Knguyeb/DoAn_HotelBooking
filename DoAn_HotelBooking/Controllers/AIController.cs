using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Models;
using DoAn_HotelBooking.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn_HotelBooking.Controllers
{
    // Kế thừa từ Controller để có thể sử dụng PartialView() và Json()
    public class AIController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;
        private readonly IAI_ReviewService _aiReviewService;
        private readonly IAI_ChatService _aiChatService;

        // Tiêm cả 3 dịch vụ vào đây
        public AIController(DoAn_HotelBookingContext context, IAI_ReviewService aiReviewService, IAI_ChatService aiChatService)
        {
            _context = context;
            _aiReviewService = aiReviewService;
            _aiChatService = aiChatService;
        }

        // ==========================================
        // 1. TÍNH NĂNG TỔNG HỢP ĐÁNH GIÁ (REVIEW)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> TongHopReview(int maPhong)
        {
            var danhGias = await _context.DanhGiaPhong
                .Where(d => d.MaPhong == maPhong)
                .ToListAsync();

            if (danhGias == null || !danhGias.Any())
            {
                return PartialView("~/Views/Phongs/_TongHopAiPartial.cshtml", new AI_ReviewViewModel());
            }

            int tongSoDanhGia = danhGias.Count;
            int soDanhGiaTot = danhGias.Count(d => d.SoSao >= 4);
            double tiLeYeuThich = Math.Round((double)soDanhGiaTot / tongSoDanhGia * 100, 2);

            var cacBinhLuan = danhGias
                .Where(d => !string.IsNullOrWhiteSpace(d.NoiDung))
                .Select(d => d.NoiDung)
                .ToList();

            var viewModel = await _aiReviewService.AnalyzeReviewsAsync(cacBinhLuan);
            viewModel.TiLeYeuThich = tiLeYeuThich;

            return PartialView("~/Views/Phongs/_TongHopAiPartial.cshtml", viewModel);
        }


        // ==========================================
        // 2. TÍNH NĂNG CHATBOT TƯ VẤN (CHATBOT)
        // ==========================================
        public class ChatRequest
        {
            public string Message { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> AskChatbot([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Tin nhắn không được để trống." });
            }

            var tatCaPhong = await _context.Phong
                .Include(p => p.KhachSan)
                .ToListAsync();

            var phongTrong = tatCaPhong
                .Where(p => p.TrangThaiHomNay == "Còn trống")
                .Take(30)
                .ToList();

            string thongTinPhong = "";

            if (phongTrong.Any())
            {
                foreach (var p in phongTrong)
                {
                    string tenKS = p.KhachSan?.TenKhachSan ?? "Chưa xác định";

                    string maKhachSan = $"[HOTEL:{tenKS}]";
                    string maPhong = $"[ROOM:{p.ID}:Phòng {p.SoPhong}]";

                    thongTinPhong +=
                        $"- {maPhong} tại {maKhachSan}, giá: {((long)p.GiaPhong):N0} VNĐ/đêm.\n";
                }
            }
            else
            {
                thongTinPhong = "Không có phòng trống.";
            }

            string promptCuoiCung = $@"
                Bạn là lễ tân AI của website đặt phòng.

                QUAN TRỌNG:
                1. Khi nhắc tới phòng PHẢI GIỮ NGUYÊN định dạng:
                   [ROOM:id:tên phòng]

                Ví dụ:
                [ROOM:15:Phòng 101]

                2. Khi nhắc tới khách sạn PHẢI GIỮ NGUYÊN định dạng:
                   [HOTEL:tên khách sạn]

                Ví dụ:
                [HOTEL:Vinpearl Resort]

                3. Không được đổi [ROOM:...] thành chữ thường.
                4. Không được xóa mã ROOM hoặc HOTEL.
                5. Chỉ trả lời dựa trên danh sách phòng bên dưới.
                6. Trả lời ngắn gọn, đúng trọng tâm.

                Danh sách phòng:

                {thongTinPhong}

                Câu hỏi khách:
                {request.Message}
                ";

            string aiResponse = await _aiChatService.TuVanKhachHangAsync(
                request.Message,
                promptCuoiCung
            );

            return Json(new { answer = aiResponse });
        }
    }
}