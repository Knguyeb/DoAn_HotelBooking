using Microsoft.EntityFrameworkCore;
using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Helpers;
using DotNetEnv;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;

Env.Load("../.env");

Console.WriteLine("GOOGLE_CLIENT_ID: " + Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"));
Console.WriteLine("EMAIL: " + Environment.GetEnvironmentVariable("EMAIL"));
Console.WriteLine("DATABASE_URL: " + Environment.GetEnvironmentVariable("DATABASE_URL"));

var builder = WebApplication.CreateBuilder(args);

// Cấu hình trung gian ép hệ thống nhận diện HTTPS khi chạy qua Proxy của Render
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
});

// Kết nối DB
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
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
    options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
    options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
});

builder.Services.AddScoped<ThangHangHelper>();

var app = builder.Build();

// ==========================================
// FIX LỖI ĐĂNG NHẬP GOOGLE TRÊN RENDER
// Đảm bảo request luôn được hiểu là HTTPS
// ==========================================
app.UseForwardedHeaders();
app.Use((context, next) =>
{
    context.Request.Scheme = "https";
    return next();
});

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