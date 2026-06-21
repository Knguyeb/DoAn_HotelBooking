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
         Bạn là một Lễ tân AI cực kỳ kiệm lời, chuyên nghiệp và đi thẳng vào vấn đề.

         NHIỆM VỤ BẮT BUỘC:
         Đọc câu hỏi của khách, TỰ TÍNH TOÁN LỌC TÌM các phòng thỏa mãn điều kiện trong đầu (chú ý kỹ các khoảng giá 'từ... đến...', 'trên', 'dưới'), và CHỈ in ra kết quả cuối cùng.

         CÁC LỆNH CẤM KỴ (VI PHẠM LÀ LỖI NGHIÊM TRỌNG):
         1. CẤM TƯỜNG THUẬT: Tuyệt đối không giải thích quá trình bạn lọc dữ liệu.
         2. CẤM NHẮC PHÒNG SAI: Không bao giờ được phép in ra những phòng nằm ngoài yêu cầu của khách. 
            (Ví dụ: Khách tìm 'từ 600 tới 800', BẠN PHẢI XÓA BỎ HOÀN TOÀN phòng 500k hoặc 900k khỏi câu trả lời. TUYỆT ĐỐI KHÔNG được in ra phòng 500k rồi chèn thêm câu 'không nằm trong khoảng giá').
         3. CẤM DÙNG TỪ NỐI THỪA: Không dùng các từ như 'Tuy nhiên', 'nhưng có', 'không hợp lệ', 'không nằm trong khoảng giá'.

         QUY TẮC ĐỊNH DẠNG (BẮT BUỘC GIỮ NGUYÊN MÃ HOẶC SẼ BỊ PHẠT):
         - Phòng: [ROOM:id:tên phòng] (Ví dụ: [ROOM:15:Phòng 101])
         - Khách sạn: [HOTEL:tên khách sạn] (Ví dụ: [HOTEL:Vinpearl])
         - Mỗi phòng in trên 1 dòng chuẩn: - [ROOM:...] tại [HOTEL:...], giá: ... VNĐ/đêm.

         Danh sách phòng hiện có:
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