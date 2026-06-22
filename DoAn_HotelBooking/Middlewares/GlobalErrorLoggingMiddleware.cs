using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
// Thêm thư viện chứa model SystemLog của bạn
// using DoAn_HotelBooking.Models; 

namespace DoAn_HotelBooking.Middlewares
{
    public class GlobalErrorLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalErrorLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Do dbContext có vòng đời là Scoped, ta phải inject nó vào hàm InvokeAsync thay vì Constructor
        public async Task InvokeAsync(HttpContext httpContext, DoAn_HotelBookingContext _context)
        {
            try
            {
                // Cho phép request đi tiếp qua các middleware/controller khác
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // Tóm được lỗi toàn cục -> Lưu vào Database
                await HandleExceptionAsync(_context, ex, httpContext);

                // Ném lỗi ra tiếp để hệ thống hiển thị trang lỗi mặc định (hoặc bạn có thể chuyển hướng về trang /Error)
                throw;
            }
        }

        private async Task HandleExceptionAsync(DoAn_HotelBookingContext context, Exception ex, HttpContext httpContext)
        {
            try
            {
                // 1. Phân loại Level động (Dựa vào loại Exception)
                string logLevel = "Error"; // Mặc định

                if (ex is UnauthorizedAccessException || ex is ArgumentException || ex is InvalidOperationException)
                {
                    logLevel = "Warning";
                }
                else if (ex is NullReferenceException || ex.GetType().Name.Contains("SqlException"))
                {
                    logLevel = "Critical";
                }

                // 2. Lấy đường dẫn (URL) nơi người dùng gặp lỗi
                string requestUrl = $"{httpContext.Request.Path}{httpContext.Request.QueryString}";

                // 3. Khởi tạo đối tượng khớp chính xác với Model của bạn
                var log = new SystemLog
                {
                    Level = logLevel,

                    // Ghép URL vào Message để Admin biết lỗi xảy ra ở trang nào
                    Message = $"[Tại: {requestUrl}] {ex.Message}",

                    // Đổ toàn bộ lịch sử lỗi (dòng code gây lỗi) vào cột Exception
                    Exception = ex.ToString(),

                    Timestamp = DateTime.UtcNow,
                    DaXuLy = false
                };

                context.SystemLogs.Add(log);
                await context.SaveChangesAsync();
            }
            catch
            {
                // Nếu việc lưu lỗi vào DB cũng thất bại, im lặng bỏ qua để không sập web
            }
        }
    }
}