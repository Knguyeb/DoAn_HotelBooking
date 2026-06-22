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
                    string diaChi = p.KhachSan?.DiaChi ?? "Chưa xác định";
                    string idKS = p.KhachSan?.MaKhachSan.ToString() ?? "0";
                    string maKhachSan = $"[HOTEL:{idKS}:{tenKS}]";
                    string maPhong = $"[ROOM:{p.ID}:Phòng {p.SoPhong}]";
                    string moTa = string.IsNullOrWhiteSpace(p.MoTa) ? "Không có thông tin thêm." : p.MoTa.Replace("\n", " ").Replace("\r", "");

                    thongTinPhong += $"- {maPhong} tại {maKhachSan} (Địa chỉ: {diaChi}), giá: {((long)p.GiaPhong):N0} VNĐ/đêm. Mô tả: {moTa}\n";
                }
            }
            else
            {
                thongTinPhong = "Không có phòng trống.";
            }

            // NÂNG CẤP PROMPT: Phân biệt Khách sạn/Phòng, Tìm kiếm theo ngữ nghĩa (chống lỗi view biển), và Ẩn mô tả
            string systemPrompt = $@"
             Bạn là Lễ tân AI cực kỳ kiệm lời, chuyên nghiệp và đi thẳng vào vấn đề. BẠN LÀ MỘT BỘ LỌC DỮ LIỆU. TUYỆT ĐỐI tuân thủ các quy tắc sau:

             [QUY TẮC PHÂN TÍCH YÊU CẦU - QUAN TRỌNG NHẤT]
             1. Bắt buộc phân tích xem khách đang hỏi tìm 'KHÁCH SẠN', tìm 'PHÒNG', hay tìm 'TIỆN ÍCH'.
             2. NẾU KHÁCH HỎI TÌM 'KHÁCH SẠN' (VD: 'khách sạn ở quận 1', 'có những khách sạn nào'):
                - CHỈ liệt kê TÊN CÁC KHÁCH SẠN thỏa mãn điều kiện.
                - Phải gom nhóm lại: Nếu có nhiều phòng thuộc cùng 1 khách sạn, CHỈ IN TÊN KHÁCH SẠN ĐÓ 1 LẦN DUY NHẤT.
                - TUYỆT ĐỐI KHÔNG liệt kê ID phòng, tên phòng hay giá tiền.
                - BẮT BUỘC IN KÈM ĐỊA CHỈ theo định dạng: - [HOTEL:id:tên khách sạn], Địa chỉ: ...
             3. NẾU KHÁCH HỎI TÌM 'PHÒNG' HOẶC 'TIỆN ÍCH' (VD: 'phòng dưới 800k', 'phòng có view biển', 'có bồn tắm'):
                - Đối chiếu NGỮ NGHĨA yêu cầu của khách với phần 'Giá', 'Địa điểm' và 'Mô tả' để LỌC (VD: 'view biển' đồng nghĩa với 'sát bờ biển', 'hướng biển').
                - CHỈ in ra các phòng THỰC SỰ KHỚP yêu cầu. TIÊU DIỆT KẾT QUẢ SAI (VD: Hỏi biển thì loại ngay Sài Gòn, Cần Thơ).
                - Nếu không có phòng nào khớp, in đúng 1 câu: 'Xin lỗi, hiện không có phòng nào thỏa mãn yêu cầu của quý khách.'
                - TUYỆT ĐỐI KHÔNG IN RA PHẦN MÔ TẢ trong câu trả lời của bạn.
                - Định dạng bắt buộc phải dùng: - [ROOM:id:tên phòng] tại [HOTEL:id:tên khách sạn], giá: ... VNĐ/đêm.

             [QUY TẮC LỌC DỮ LIỆU & ĐỊA CHỈ - SỐNG CÒN]
             1. Lọc chuẩn xác theo khu vực (VD: Khách hỏi 'Quận 1' phải tìm đúng chữ 'Quận 1' hoặc 'Q.1').
             2. TUYỆT ĐỐI KHÔNG xuất ra dữ liệu sai lệch. KHÔNG tự ý gợi ý bừa bãi.
             3. CẤM TƯỜNG THUẬT: KHÔNG dùng các câu rườm rà như 'Xin chào', 'Dưới đây là...', 'Tuy nhiên...'. CHỈ IN RA KẾT QUẢ.

             [QUY TẮC VỀ THỜI GIAN - RẤT QUAN TRỌNG]
             1. Dữ liệu bên dưới chỉ là của HÔM NAY.
             2. Nếu khách hỏi ngày tương lai, hãy từ chối: 'Hiện tại Lễ tân AI chỉ kiểm tra phòng trống trong ngày. Quý khách vui lòng dùng công cụ Tìm Kiếm trên website để xem các ngày tới.'
             3. Mặc định không nhắc thời gian là hỏi cho hôm nay.

             [QUY TẮC ĐỊNH DẠNG - BẮT BUỘC]
             1. Khi nhắc tới phòng hoặc khách sạn, PHẢI GIỮ NGUYÊN cụm: [ROOM:id:tên phòng] và [HOTEL:id:tên khách sạn]. Không đổi thành chữ thường, không xóa ngoặc vuông.
             2. Trình bày mỗi kết quả trên 1 dòng riêng biệt.

             Danh sách dữ liệu phòng trống TRONG HÔM NAY (Kèm mô tả dùng để lọc):
             {thongTinPhong}

             Câu hỏi khách:
             {request.Message}
             ";

            string aiResponse = await _aiChatService.TuVanKhachHangAsync(
                request.Message,
                systemPrompt
            );

            return Json(new { answer = aiResponse });
        }
    }
}