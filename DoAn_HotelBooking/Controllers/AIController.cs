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

                    // BỔ SUNG LẤY ID KHÁCH SẠN (Thay p.KhachSan.ID bằng đúng tên thuộc tính ID khách sạn của bạn)
                    string idKS = p.KhachSan?.MaKhachSan.ToString() ?? "0";

                    // SỬA DÒNG NÀY: Ép đúng định dạng [HOTEL:id:tên] để JS nhận diện được
                    string maKhachSan = $"[HOTEL:{idKS}:{tenKS}]";

                    string maPhong = $"[ROOM:{p.ID}:Phòng {p.SoPhong}]";

                    // Nhồi thêm Địa chỉ vào dữ liệu cho AI
                    thongTinPhong += $"- {maPhong} tại {maKhachSan} (Địa chỉ: {diaChi}), giá: {((long)p.GiaPhong):N0} VNĐ/đêm.\n";
                }
            }
            else
            {
                thongTinPhong = "Không có phòng trống.";
            }

            // NÂNG CẤP PROMPT: Dạy AI phân biệt lúc nào trả lời Khách sạn, lúc nào trả lời Phòng
            string promptCuoiCung = $@"
             Bạn là Lễ tân AI cực kỳ kiệm lời, chuyên nghiệp và đi thẳng vào vấn đề. TUYỆT ĐỐI tuân thủ các quy tắc sau:

             [QUY TẮC PHÂN TÍCH YÊU CẦU - QUAN TRỌNG NHẤT]
             1. Bắt buộc phân tích xem khách đang hỏi tìm 'KHÁCH SẠN' hay tìm 'PHÒNG'.
             2. NẾU KHÁCH HỎI TÌM 'KHÁCH SẠN' (VD: 'khách sạn ở quận 1', 'có những khách sạn nào'):
                - CHỈ liệt kê TÊN CÁC KHÁCH SẠN thỏa mãn điều kiện.
                - Phải gom nhóm lại: Nếu có nhiều phòng thuộc cùng 1 khách sạn, CHỈ IN TÊN KHÁCH SẠN ĐÓ 1 LẦN DUY NHẤT.
                - TUYỆT ĐỐI KHÔNG liệt kê ID phòng, tên phòng hay giá tiền.
                - BẮT BUỘC IN KÈM ĐỊA CHỈ. Định dạng mẫu phải dùng: - [HOTEL:id:tên khách sạn], Địa chỉ: ...
                - Định dạng bắt buộc phải dùng: - [ROOM:id:tên phòng] tại [HOTEL:id:tên khách sạn], giá: ... VNĐ/đêm.
             3. NẾU KHÁCH HỎI TÌM 'PHÒNG' (VD: 'phòng ở quận 1', 'phòng dưới 800k'):
                - Liệt kê chi tiết TỪNG PHÒNG thỏa mãn điều kiện.
                - Định dạng bắt buộc phải dùng: - [ROOM:id:tên phòng] tại [HOTEL:tên khách sạn], giá: ... VNĐ/đêm.

             [QUY TẮC LỌC DỮ LIỆU & ĐỊA CHỈ - SỐNG CÒN]
             1. Lọc chuẩn xác theo GIÁ và ĐỊA ĐIỂM (VD: Khách hỏi 'Quận 1' phải tìm đúng chữ 'Quận 1' hoặc 'Q.1').
             2. TUYỆT ĐỐI KHÔNG xuất ra dữ liệu sai quận, sai giá. KHÔNG giải thích.
             3. CẤM TƯỜNG THUẬT: KHÔNG dùng các câu rườm rà như 'Xin chào', 'Dưới đây là...', 'Tuy nhiên...'. CHỈ IN RA KẾT QUẢ.

             [QUY TẮC VỀ THỜI GIAN - RẤT QUAN TRỌNG]
             1. Dữ liệu bên dưới chỉ là của HÔM NAY.
             2. Nếu khách hỏi ngày tương lai, hãy từ chối: 'Hiện tại Lễ tân AI chỉ kiểm tra phòng trống trong ngày. Quý khách vui lòng dùng công cụ Tìm Kiếm trên website để xem các ngày tới.'
             3. Mặc định không nhắc thời gian là hỏi cho hôm nay.

            [QUY TẮC ĐỊNH DẠNG - BẮT BUỘC]
             1. Khi nhắc tới phòng PHẢI GIỮ NGUYÊN định dạng: [ROOM:id:tên phòng] (VD: [ROOM:15:Phòng 101]).
             2. Khi nhắc tới khách sạn PHẢI GIỮ NGUYÊN định dạng: [HOTEL:id:tên khách sạn] (VD: [HOTEL:5:Vinpearl]).
             3. Trình bày mỗi phòng trên 1 dòng theo mẫu: - [ROOM:...] tại [HOTEL:...], giá: ... VNĐ/đêm. Không đổi thành chữ thường, không xóa ngoặc vuông.

             Danh sách dữ liệu phòng trống TRONG HÔM NAY:
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