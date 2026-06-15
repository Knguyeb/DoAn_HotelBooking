using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Helpers;
using DoAn_HotelBooking.Security;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

Env.Load("../.env");

Console.WriteLine("GOOGLE_CLIENT_ID: " + Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"));
Console.WriteLine("EMAIL: " + Environment.GetEnvironmentVariable("EMAIL"));
Console.WriteLine("DATABASE_URL: " + Environment.GetEnvironmentVariable("DATABASE_URL"));

var builder = WebApplication.CreateBuilder(args);

// 1. CẤU HÌNH PROXY RENDER (Đã gộp thành 1 khối duy nhất)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 2. KHAI BÁO KẾT NỐI DATABASE TRƯỚC
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new Exception("DATABASE_URL not found");

builder.Services.AddDbContext<DoAn_HotelBookingContext>(options =>
    options.UseNpgsql(connectionString));

// 3. SAU ĐÓ MỚI GỌI DATA PROTECTION (Vì nó cần Database ở trên để lưu chìa khóa)
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<DoAn_HotelBookingContext>();

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
// ==========================================
app.UseForwardedHeaders();
app.Use((context, next) =>
{
    context.Request.Scheme = "https";
    return next();
});

// Seed dữ liệu và tự động tạo/cập nhật bảng
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DoAn_HotelBookingContext>();

    // Dòng này sẽ xây lại toàn bộ các bảng mới tinh (bao gồm cả bảng chứa chìa khóa)
    context.Database.Migrate();

    // Nạp lại dữ liệu mẫu
    SeedData.Initialize(services);
}

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ChanQuyen>();
});

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