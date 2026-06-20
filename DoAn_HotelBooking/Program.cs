using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Helpers;
using DoAn_HotelBooking.Security;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using NpgsqlTypes;
using DoAn_HotelBooking.Services;

Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

// 1. CẤU HÌNH PROXY RENDER
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHttpClient<IAI_ReviewService, AI_ReviewService>();

// 2. KHAI BÁO KẾT NỐI DATABASE TRƯỚC
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new Exception("DATABASE_URL not found");

// ==========================================
// 🌟 TÍCH HỢP SERILOG GHI LỖI VÀO POSTGRESQL
// ==========================================
IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
    {"Timestamp", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
    {"Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    {"Message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    {"Exception", new ExceptionColumnWriter(NpgsqlDbType.Text) }
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Error() // Chỉ lưu lỗi (Error) để tránh rác Database
    .WriteTo.PostgreSQL(
        connectionString: connectionString,
        tableName: "SystemLogs",
        columnOptions: columnWriters,
        needAutoCreateTable: true) // Tự động tạo bảng nếu chưa có
    .CreateLogger();

builder.Host.UseSerilog(); // Kích hoạt Serilog
// ==========================================

builder.Services.AddDbContext<DoAn_HotelBookingContext>(options =>
    options.UseNpgsql(connectionString));

// 3. SAU ĐÓ MỚI GỌI DATA PROTECTION
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

// Authentication (Google + Cookie)
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

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ChanQuyen>();
});

var app = builder.Build();

// FIX LỖI ĐĂNG NHẬP GOOGLE TRÊN RENDER
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

    context.Database.Migrate();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();