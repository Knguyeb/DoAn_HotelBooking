using System.Text.Json;
using System.Text;
using DoAn_HotelBooking.Models;
using Microsoft.Extensions.Configuration;

namespace DoAn_HotelBooking.Services
{
    public class AI_ReviewService : IAI_ReviewService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AI_ReviewService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AI_ReviewViewModel> AnalyzeReviewsAsync(List<string> reviews)
        {
            if (reviews == null || !reviews.Any())
            {
                return new AI_ReviewViewModel();
            }

            string chuoiBinhLuanGop = string.Join(" | ", reviews);

            string prompt = $"Bạn là trợ lý AI khách sạn. Đọc các bình luận: '{chuoiBinhLuanGop}'. " +
                 "Hãy trả về JSON: {\"UuDiem\": [], \"NhuocDiem\": [], \"GoiY\": []}. " +
                 "YÊU CẦU: " +
                 "1. Nếu có nhược điểm: Gợi ý cách khắc phục nhược điểm đó. " +
                 "2. Nếu phòng đã hoàn hảo (không có nhược điểm): Hãy đưa ra các gợi ý 'nâng cấp trải nghiệm' (ví dụ: trang trí thêm, thêm dịch vụ, hoặc tạo điểm nhấn sang trọng) để khách hàng hài lòng hơn nữa. " +
                 "KHÔNG dùng Markdown, KHÔNG giải thích thêm.";

            return await CallGroqApiAsync(prompt);
        }

        private async Task<AI_ReviewViewModel> CallGroqApiAsync(string prompt)
        {
            // Lấy API Key từ cấu hình (đảm bảo file .env hoặc biến môi trường đã set GROQ_API_KEY)
            string apiKey = _configuration["GROQ_API_KEY"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                return new AI_ReviewViewModel { NhuocDiem = new List<string> { "Lỗi: Không tìm thấy GROQ_API_KEY." } };
            }

            // URL chuẩn cho OpenAI-compatible của Groq
            string url = "https://api.groq.com/openai/v1/chat/completions";

            var requestBody = new
            {
                // Bạn có thể đổi sang "llama-3.3-70b-versatile" hoặc model khác tùy chọn
                model = "llama-3.3-70b-versatile",
                messages = new[]
                {
            new { role = "system", content = "Bạn là trợ lý AI. Trả về kết quả JSON, không markdown." },
            new { role = "user", content = prompt }
        }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Xóa header cũ (nếu có) và thêm Bearer token cho Groq
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            try
            {
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    return new AI_ReviewViewModel { NhuocDiem = new List<string> { $"Lỗi Groq ({(int)response.StatusCode}): {errorBody}" } };
                }

                string responseString = await response.Content.ReadAsStringAsync();

                // Đọc dữ liệu theo cấu trúc OpenAI chuẩn
                using var document = JsonDocument.Parse(responseString);
                string textContent = document.RootElement.GetProperty("choices")[0]
                                             .GetProperty("message")
                                             .GetProperty("content")
                                             .GetString();

                textContent = textContent.Replace("```json", "").Replace("```", "").Trim();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<AI_ReviewViewModel>(textContent, options);
            }
            catch (Exception ex)
            {
                return new AI_ReviewViewModel { NhuocDiem = new List<string> { "Lỗi xử lý: " + ex.Message } };
            }
        }
    }
}