using DoAn_HotelBooking.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn_HotelBooking.Data
{
    // Đã sửa IDataProtectionProvider thành IDataProtectionKeyContext
    public class DoAn_HotelBookingContext : DbContext, IDataProtectionKeyContext
    {
        public DoAn_HotelBookingContext(DbContextOptions<DoAn_HotelBookingContext> options)
            : base(options)
        {
        }

        public DbSet<DoAn_HotelBooking.Models.KhachSan> KhachSan { get; set; } = default!;
        public DbSet<DoAn_HotelBooking.Models.TaiKhoan> TaiKhoan { get; set; } = default!;
        public DbSet<DoAn_HotelBooking.Models.Phong> Phong { get; set; } = default!;
        public DbSet<DoAn_HotelBooking.Models.DatPhong> DatPhong { get; set; } = default!;
        public DbSet<DoAn_HotelBooking.Models.DanhGiaKS> DanhGiaKS { get; set; } = default!;
        public DbSet<DoAn_HotelBooking.Models.DanhGiaPhong> DanhGiaPhong { get; set; } = default!;
        public DbSet<DoAn_HotelBooking.Models.HangThanhVien> HangThanhVien { get; set; } = default!;
        public DbSet<DoAn_HotelBooking.Models.ThongBao> ThongBao { get; set; } = default!;
        // Bảng dùng để lưu chìa khóa bảo mật
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}