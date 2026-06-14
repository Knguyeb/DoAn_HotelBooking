using Microsoft.EntityFrameworkCore;
using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Helpers;
using DotNetEnv;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

Env.Load("../.env");

Console.WriteLine("GOOGLE_CLIENT_ID: " +
    Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"));

Console.WriteLine("EMAIL: " +
    Environment.GetEnvironmentVariable("EMAIL"));

Console.WriteLine("DATABASE_URL: " +
    Environment.GetEnvironmentVariable("DATABASE_URL"));

var builder = WebApplication.CreateBuilder(args);

// Kết nối DB
var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new Exception("DATABASE_URL not found");

builder.Services.AddDbContext<DoAn_HotelBookingContext>(options =>
    options.UseNpgsql(connectionString));

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Thêm Authentication (Google + Cookie)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})

.AddCookie()
.AddGoogle(options =>
{
    // ⚠️ Dùng đúng ClientId và ClientSecret bạn copy từ Google Cloud
    options.ClientId =
     Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");

    options.ClientSecret =
        Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
});

builder.Services.AddScoped<ThangHangHelper>();

var app = builder.Build();

// ==========================================
// TỰ ĐỘNG CHẠY NGROK TRONG MÔI TRƯỜNG DEV
// ==========================================
if (app.Environment.IsDevelopment())
{
    string port = "7292";
    string ngrokPath = @"D:\App\Ngrok\ngrok-v3-stable-windows-amd64\ngrok.exe";

    // Mẹo: Chỉ chạy Ngrok nếu máy tính thực sự có file này ở ổ D (tức là đang chạy bằng F5).
    // Nếu đang chạy trong Docker (Linux), nó sẽ không tìm thấy ổ D, nên sẽ tự động bỏ qua mà không báo lỗi!
    if (System.IO.File.Exists(ngrokPath))
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = ngrokPath,
                Arguments = $"http https://localhost:{port}",
                CreateNoWindow = false,
                UseShellExecute = true
            });
            Console.WriteLine("Đã tự động gọi Ngrok thành công!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi gọi Ngrok: " + ex.Message);
        }
    }
}

// Seed dữ liệu với cơ chế thử lại (Retry)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DoAn_HotelBookingContext>();

    // Tự động tạo DB nếu chưa có
    context.Database.Migrate();

    // Seed dữ liệu
    SeedData.Initialize(services);
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// ✅ Bắt buộc: thêm Authentication trước Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();