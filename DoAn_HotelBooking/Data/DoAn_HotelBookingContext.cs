using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DoAn_HotelBooking.Models;

namespace DoAn_HotelBooking.Data
{
    public class DoAn_HotelBookingContext : DbContext
    {
        public DoAn_HotelBookingContext (DbContextOptions<DoAn_HotelBookingContext> options)
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
    }
}
