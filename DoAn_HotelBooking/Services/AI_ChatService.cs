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

        // Đổi tham số thứ 2 thành systemPrompt để nhận toàn bộ luật từ Controller
        public async Task<string> TuVanKhachHangAsync(string userMessage, string systemPrompt)
        {
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
                temperature = 0.1
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