using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace DoAn_HotelBooking.Services
{
    public class AI_ChatService : IAI_ChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AI_ChatService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> TuVanKhachHangAsync(string userMessage, string thongTinPhongTrong)
        {
            // System prompt để định hình tính cách và nạp dữ liệu cho AI
            string systemPrompt = $@"
                Bạn là nhân viên lễ tân ảo vô cùng lịch sự và chuyên nghiệp của hệ thống đặt phòng khách sạn. 
                Nhiệm vụ của bạn là tư vấn cho khách dựa trên danh sách phòng trống thực tế dưới đây. 
                Tuyệt đối KHÔNG bịa đặt thông tin khách sạn hoặc giá cả không có trong danh sách. 
                Nếu khách hỏi ngoài lề (không liên quan đến đặt phòng), hãy khéo léo từ chối và quay lại chủ đề phòng ốc.

                [DANH SÁCH PHÒNG CÒN TRỐNG HÔM NAY]
                {thongTinPhongTrong}";

            return await CallGroqApiAsync(systemPrompt, userMessage);
        }

        private async Task<string> CallGroqApiAsync(string systemPrompt, string userMessage)
        {
            string apiKey = _configuration["GROQ_API_KEY"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                return "Hệ thống đang bảo trì kênh tư vấn AI (Lỗi thiếu API Key). Xin lỗi quý khách!";
            }

            string url = "https://api.groq.com/openai/v1/chat/completions";

            var requestBody = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                temperature = 0.5 // Mức 0.5 giúp AI trả lời tự nhiên nhưng không bị ảo giác bịa thông tin
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            try
            {
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    return "Xin lỗi, hiện tại tôi không thể kết nối tới hệ thống tư vấn. Vui lòng thử lại sau.";
                }

                string responseString = await response.Content.ReadAsStringAsync();

                using var document = JsonDocument.Parse(responseString);
                string textContent = document.RootElement.GetProperty("choices")[0]
                                             .GetProperty("message")
                                             .GetProperty("content")
                                             .GetString();

                return textContent ?? "Tôi chưa hiểu ý bạn, bạn có thể nói rõ hơn không?";
            }
            catch (Exception)
            {
                return "Đã xảy ra lỗi trong quá trình kết nối đến trợ lý AI.";
            }
        }
    }
}