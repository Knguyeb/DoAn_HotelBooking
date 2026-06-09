using Microsoft.EntityFrameworkCore;
using DoAn_HotelBooking.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Kết nối DB
builder.Services.AddDbContext<DoAn_HotelBookingContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DoAn_HotelBookingContext")
        ?? throw new InvalidOperationException("Connection string 'DoAn_HotelBookingContext' not found.")
    ));

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
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

var app = builder.Build();

// Seed dữ liệu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
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