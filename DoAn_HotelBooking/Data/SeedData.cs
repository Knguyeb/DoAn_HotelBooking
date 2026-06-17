using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Helper;
using DoAn_HotelBooking.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using DoAn_HotelBooking.Helper;
using System.Text;

namespace DoAn_HotelBooking.Data
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new DoAn_HotelBookingContext(serviceProvider.
                GetRequiredService<DbContextOptions<DoAn_HotelBookingContext>>()))
            {
                if (context == null)
                {
                    throw new ArgumentNullException("Null DBContext");
                }

                // Dữ liệu khách sạn (chỉ thêm nếu chưa có)
                if (!context.KhachSan.Any(k => k.MaKhachSan == "SRT101"))
                {
                    context.KhachSan.Add(new KhachSan
                    {
                        MaKhachSan = "SRT101",
                        TenKhachSan = "Sheraton CanTho",
                        DiaChi = "30 Tháng 4, Xuân Khánh, Ninh Kiều, Cần Thơ",
                        ViDo = "10.024168234946258",
                        KinhDo = "105.7742316",
                        SoDienThoai = "02923761888",
                        HinhAnh = "/Images/sheratonct.jpg; /Images/sheratonct1.jpg"
                    });
                }

                if (!context.KhachSan.Any(k => k.MaKhachSan == "SRT202"))
                {
                    context.KhachSan.Add(new KhachSan
                    {
                        MaKhachSan = "SRT202",
                        TenKhachSan = "Sheraton PhuQuoc",
                        DiaChi = "Bai Dai area, Tp. Phú Quốc",
                        ViDo = "10.337650784264875",
                        KinhDo = "103.84982415582076",
                        SoDienThoai = "02973619999",
                        HinhAnh = "/Images/pq.jpg; /Images/pq1.jpg"
                    });
                }

                if (!context.KhachSan.Any(k => k.MaKhachSan == "REX101"))
                {
                    context.KhachSan.Add(new KhachSan
                    {
                        MaKhachSan = "REX101",
                        TenKhachSan = "Rex SaiGon",
                        DiaChi = "Nguyễn Huệ, Bến Nghé, Quận 1, Hồ Chí Minh",
                        ViDo = "10.7758664792898",
                        KinhDo = "106.70127532883585",
                        SoDienThoai = "0917590900",
                        HinhAnh = "/Images/rexsg.jpg; /Images/rexsg1.jpg"
                    });
                }

                if (!context.KhachSan.Any(k => k.MaKhachSan == "LVL101"))
                {
                    context.KhachSan.Add(new KhachSan
                    {
                        MaKhachSan = "LVL101",
                        TenKhachSan = "La Vela SaiGon",
                        DiaChi = "Nam Kỳ Khởi Nghĩa, Phường Xuân Hòa, Quận 3, Hồ Chí Minh",
                        ViDo = "10.788659299999999",
                        KinhDo = "106.68548369999999",
                        SoDienThoai = "02838299201",
                        HinhAnh = "/Images/lavelasg.jpg; /Images/lavelasg1.jpg"
                    });
                }

                if (!context.KhachSan.Any(k => k.MaKhachSan == "NEW101"))
                {
                    context.KhachSan.Add(new KhachSan
                    {
                        MaKhachSan = "NEW101",
                        TenKhachSan = "New World SaiGon",
                        DiaChi = "76 Lê Lai, Phường Bến Thành, Quận 1, Hồ Chí Minh",
                        ViDo = "10.770871899999994",
                        KinhDo = "106.6951897",
                        SoDienThoai = "02838228888",
                        HinhAnh = "/Images/newworldsg.jpg; /Images/newworldsg1.jpg"
                    });
                }

                if (!context.KhachSan.Any(k => k.MaKhachSan == "MEL101"))
                {
                    context.KhachSan.Add(new KhachSan
                    {
                        MaKhachSan = "MEL101",
                        TenKhachSan = "Meliá Hanoi",
                        DiaChi = "44 P. Lý Thường Kiệt, Trần Hưng Đạo, Hoàn Kiếm, Hà Nội",
                        ViDo = "21.02433698540324",
                        KinhDo = "105.84862319999999",
                        SoDienThoai = "02439343343",
                        HinhAnh = "/Images/hn.jpg; /Images/hn1.jpg"
                    });
                }

                if (!context.KhachSan.Any(k => k.MaKhachSan == "PKH101"))
                {
                    context.KhachSan.Add(new KhachSan
                    {
                        MaKhachSan = "PKH101",
                        TenKhachSan = "Park Hyatt Saigon",
                        DiaChi = "2 Công trường Lam Sơn, Bến Nghé, Quận 1, Hồ Chí Minh",
                        ViDo = "10.777772479156324",
                        KinhDo = "106.70338020000001",
                        SoDienThoai = "02838241234",
                        HinhAnh = "/Images/parksg.jpg; /Images/parksg1.jpg"
                    });
                }

                if (!context.KhachSan.Any(k => k.MaKhachSan == "RVR101"))
                {
                    context.KhachSan.Add(new KhachSan
                    {
                        MaKhachSan = "RVR101",
                        TenKhachSan = "The Reverie Saigon",
                        DiaChi = "22-36 Nguyễn Huệ, Bến Nghé, Quận 1, Hồ Chí Minh",
                        ViDo = "10.774066089559152",
                        KinhDo = "106.70420124338432",
                        SoDienThoai = "02838236688",
                        HinhAnh = "/Images/reveriesg.jpg; /Images/reveriesg1.jpg"
                    });
                }

                if (!context.KhachSan.Any(k => k.MaKhachSan == "AAA000"))
                {
                    context.KhachSan.Add(new KhachSan
                    {
                        MaKhachSan = "AAA000",
                        TenKhachSan = "Continental SaiGon",
                        DiaChi = "Đồng Khởi, Bến Nghé, Quận 1, Hồ Chí Minh",
                        ViDo = "10.776915979216522",
                        KinhDo = "106.70250249999978",
                        SoDienThoai = "02838299201",
                        HinhAnh = "/Images/ContinentalSG.jpg"
                    });
                }

                if (!context.KhachSan.Any(k => k.MaKhachSan == "AAA001")) 
                {
                    context.KhachSan.Add(new KhachSan
                    {
                        MaKhachSan = "AAA001",
                        TenKhachSan = "Continental New York",
                        DiaChi = "70-72, Xa lộ Hà Nội, An Khánh, Hồ Chí Minh",
                        ViDo = "10.37164524646615",
                        KinhDo = "105.43233353558205",
                        SoDienThoai = "0895123647",
                        HinhAnh = "/Images/ContinentalNY.jpg; /Images/SG.jpg"
                    });
                }

                // Tài khoản mặc định (chỉ thêm nếu chưa có)
                if (!context.TaiKhoan.Any(t => t.TenDangNhap == "system"))
                {
                    context.TaiKhoan.Add(new TaiKhoan
                    {
                        HoVaTen = "Admin",
                        TenDangNhap = "system",
                        Email = "admin@hotel.com",
                        SoDienThoai = "000000000",
                        MatKhau = PasswordHelper.HashPassword("123456"),
                        QuyenHan = "Admin"
                    });
                }

                if (!context.TaiKhoan.Any(t => t.TenDangNhap == "manager"))
                {
                    context.TaiKhoan.Add(new TaiKhoan
                    {
                        HoVaTen = "Quản lý",
                        TenDangNhap = "manager",
                        Email = "manager@hotel.com",
                        SoDienThoai = "111111111",
                        MatKhau = PasswordHelper.HashPassword("123456"),
                        QuyenHan = "Quản lý",
                        MaKhachSan = "AAA000"
                    });
                }

                if (!context.TaiKhoan.Any(t => t.TenDangNhap == "kn"))
                {
                    context.TaiKhoan.Add(new TaiKhoan
                    {
                        HoVaTen = "Khôi Nguyên",
                        TenDangNhap = "kn",
                        Email = "kn@gmail.com",
                        SoDienThoai = "0521478963",
                        MatKhau = PasswordHelper.HashPassword("1"),
                        QuyenHan = "Quản lý",
                        MaKhachSan = "AAA000"
                    });
                }

                if (!context.TaiKhoan.Any(t => t.TenDangNhap == "np"))
                {
                    context.TaiKhoan.Add(new TaiKhoan
                    {
                        HoVaTen = "Ngọc Phượng",
                        TenDangNhap = "np",
                        Email = "np@gmail.com",
                        SoDienThoai = "0215469873",
                        MatKhau = PasswordHelper.HashPassword("1"),
                        QuyenHan = "Quản lý",
                        MaKhachSan = "AAA001"
                    });
                }

                if (!context.TaiKhoan.Any(t => t.TenDangNhap == "tn"))
                {
                    context.TaiKhoan.Add(new TaiKhoan
                    {
                        HoVaTen = "Trí Nhàn",
                        TenDangNhap = "tn",
                        Email = "tn@gmail.com",
                        SoDienThoai = "0654128793",
                        MatKhau = PasswordHelper.HashPassword("1"),
                        QuyenHan = "Quản lý",
                        MaKhachSan = "AAA001"
                    });
                }

                if (!context.TaiKhoan.Any(t => t.TenDangNhap == "employee"))
                {
                    context.TaiKhoan.Add(new TaiKhoan
                    {
                        HoVaTen = "Nhân viên",
                        TenDangNhap = "employee",
                        Email = "emlpoyee@hotel.com",
                        SoDienThoai = "222222222",
                        MatKhau = PasswordHelper.HashPassword("123456"),
                        QuyenHan = "Nhân viên",
                        MaKhachSan = "AAA001"
                    });
                }

                if (!context.TaiKhoan.Any(t => t.TenDangNhap == "hk"))
                {
                    context.TaiKhoan.Add(new TaiKhoan
                    {
                        HoVaTen = "Hữu Khang",
                        TenDangNhap = "hk",
                        Email = "hk@gmail.com",
                        SoDienThoai = "0987451263",
                        MatKhau = PasswordHelper.HashPassword("1"),
                        QuyenHan = "Nhân viên",
                        MaKhachSan = "AAA001"
                    });
                }

                if (!context.TaiKhoan.Any(t => t.TenDangNhap == "tp"))
                {
                    context.TaiKhoan.Add(new TaiKhoan
                    {
                        HoVaTen = "Thanh Phong",
                        TenDangNhap = "tp",
                        Email = "tp@gmail.com",
                        SoDienThoai = "0658974123",
                        MatKhau = PasswordHelper.HashPassword("1"),
                        QuyenHan = "Nhân viên",
                        MaKhachSan = "AAA000"
                    });
                }

                if (!context.TaiKhoan.Any(t => t.TenDangNhap == "vn"))
                {
                    context.TaiKhoan.Add(new TaiKhoan
                    {
                        HoVaTen = "Văn Nhờ",
                        TenDangNhap = "vn",
                        Email = "vn@gmail.com",
                        SoDienThoai = "054126884",
                        MatKhau = PasswordHelper.HashPassword("1"),
                        QuyenHan = "Nhân viên",
                        MaKhachSan = "AAA000"
                    });
                }

                var hotels = context.KhachSan.ToList();
                foreach (var ks in hotels)
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        string username = $"ql_{ks.MaKhachSan}_{i}";
                        if (!context.TaiKhoan.Any(t => t.TenDangNhap == username))
                        {
                            context.TaiKhoan.Add(new TaiKhoan
                            {
                                HoVaTen = $"QL{i} - {ks.TenKhachSan}",
                                TenDangNhap = username,
                                Email = $"{username}@hotel.com",
                                SoDienThoai = $"09{i}00000{ks.MaKhachSan.GetHashCode().ToString().Substring(0, 2).Replace("-", "0")}",
                                MatKhau = PasswordHelper.HashPassword("1"),
                                QuyenHan = "Quản lý",
                                MaKhachSan = ks.MaKhachSan
                            });
                        }
                    }

                    // 2 nhân viên
                    for (int i = 1; i <= 2; i++)
                    {
                        string username = $"nv_{ks.MaKhachSan}_{i}";
                        if (!context.TaiKhoan.Any(t => t.TenDangNhap == username))
                        {
                            context.TaiKhoan.Add(new TaiKhoan
                            {
                                HoVaTen = $"NV{i} - {ks.TenKhachSan}",
                                TenDangNhap = username,
                                Email = $"{username}@hotel.com",
                                SoDienThoai = $"08{i}00000{ks.MaKhachSan.GetHashCode().ToString().Substring(0, 2).Replace("-", "0")}",
                                MatKhau = PasswordHelper.HashPassword("1"),
                                QuyenHan = "Nhân viên",
                                MaKhachSan = ks.MaKhachSan
                            });
                        }
                    }
                }

                if (!context.TaiKhoan.Any(t => t.TenDangNhap == "customer"))
                {
                    context.TaiKhoan.Add(new TaiKhoan
                    {
                        HoVaTen = "Khách hàng",
                        TenDangNhap = "customer",
                        Email = "khoinguyenag2004@gmail.com",
                        SoDienThoai = "222222222",
                        MatKhau = PasswordHelper.HashPassword("1"),
                        QuyenHan = "Khách hàng"
                    });
                }

                if (!context.TaiKhoan.Any(t => t.TenDangNhap == "cv"))
                {
                    context.TaiKhoan.Add(new TaiKhoan
                    {
                        HoVaTen = "Cô Vy",
                        TenDangNhap = "cv",
                        Email = "khoinguyenag2004@gmail.com",
                        SoDienThoai = "0987451236",
                        MatKhau = PasswordHelper.HashPassword("1"),
                        QuyenHan = "Khách hàng"
                    });
                }

                // Dữ liệu phòng (chỉ thêm nếu chưa có)
                if (!context.Phong.Any())
                {
                    context.Phong.AddRange(
                        new Phong
                        {
                            SoPhong = 101,
                            Tang = 1,
                            LoaiPhong = "Standard",
                            SucChua = 2,
                            GiaPhong = 500000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/phong1.jpg; /Images/phong2.jpg",
                            MaKhachSan = "AAA000"
                        },
                        new Phong
                        {
                            SoPhong = 102,
                            Tang = 1,
                            LoaiPhong = "Deluxe",
                            SucChua = 3,
                            GiaPhong = 800000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/phong3.jpg; /Images/phong4.jpg",
                            MaKhachSan = "AAA000"
                        },
                        new Phong
                        {
                            SoPhong = 201,
                            Tang = 2,
                            LoaiPhong = "VIP",
                            SucChua = 4,
                            GiaPhong = 1200000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/phong5.jpg; /Images/phong6.webp",
                            MaKhachSan = "AAA001"
                        },
                        new Phong
                        {
                            SoPhong = 501,
                            Tang = 5,
                            LoaiPhong = "President",
                            SucChua = 2,
                            GiaPhong = 2500000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/phong7.jpg; /Images/phong8.jpg",
                            MaKhachSan = "AAA001"
                        },
                        new Phong
                        {
                            SoPhong = 501,
                            Tang = 5,
                            LoaiPhong = "President",
                            SucChua = 2,
                            GiaPhong = 2500000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/srt1.jpg; /Images/srt2.jpg",
                            MaKhachSan = "SRT101"
                        },
                        new Phong
                        {
                            SoPhong = 501,
                            Tang = 5,
                            LoaiPhong = "President",
                            SucChua = 2,
                            GiaPhong = 2500000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/srt1.jpg; /Images/srt2.jpg",
                            MaKhachSan = "SRT202"
                        },
                        new Phong
                        {
                            SoPhong = 201,
                            Tang = 2,
                            LoaiPhong = "VIP",
                            SucChua = 4,
                            GiaPhong = 1200000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/rex1.jpg; /Images/rex2.webp",
                            MaKhachSan = "REX101"
                        },
                        new Phong
                        {
                            SoPhong = 102,
                            Tang = 1,
                            LoaiPhong = "Deluxe",
                            SucChua = 3,
                            GiaPhong = 800000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/lvl1.jpg; /Images/lvl2.jpg",
                            MaKhachSan = "LVL101"
                        },
                        new Phong
                        {
                            SoPhong = 101,
                            Tang = 1,
                            LoaiPhong = "Standard",
                            SucChua = 2,
                            GiaPhong = 500000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/new1.jpg; /Images/new2.jpg",
                            MaKhachSan = "NEW101"
                        },
                        new Phong
                        {
                            SoPhong = 501,
                            Tang = 5,
                            LoaiPhong = "President",
                            SucChua = 2,
                            GiaPhong = 2500000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/srt1.jpg; /Images/srt2.jpg",
                            MaKhachSan = "MEL101"
                        },
                        new Phong
                        {
                            SoPhong = 501,
                            Tang = 5,
                            LoaiPhong = "President",
                            SucChua = 2,
                            GiaPhong = 2500000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/srt1.jpg; /Images/srt2.jpg",
                            MaKhachSan = "PKH101"
                        },
                        new Phong
                        {
                            SoPhong = 501,
                            Tang = 5,
                            LoaiPhong = "President",
                            SucChua = 2,
                            GiaPhong = 2500000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/srt1.jpg; /Images/srt2.jpg",
                            MaKhachSan = "RVR101"
                        }
                    );
                }

                // Seed hạng thành viên
                if (!context.HangThanhVien.Any())
                {
                    context.HangThanhVien.AddRange(
                        new HangThanhVien
                        {
                            TenHang = "Đồng",
                            MocDiemToiThieu = 1,
                            TyLeGiamGia = 0
                        },
                        new HangThanhVien
                        {
                            TenHang = "Bạc",
                            MocDiemToiThieu = 100,
                            TyLeGiamGia = 3
                        },
                        new HangThanhVien
                        {
                            TenHang = "Vàng",
                            MocDiemToiThieu = 500,
                            TyLeGiamGia = 5
                        },
                        new HangThanhVien
                        {
                            TenHang = "Bạch Kim",
                            MocDiemToiThieu = 1000,
                            TyLeGiamGia = 8
                        },
                        new HangThanhVien
                        {
                            TenHang = "Kim Cương",
                            MocDiemToiThieu = 2000,
                            TyLeGiamGia = 15
                        }
                    );

                    context.SaveChanges();
                }

                if (!context.SystemLogs.Any())
                {
                    context.SystemLogs.AddRange(
                        new SystemLog
                        {
                            Message = "An error occurred while saving the entity changes. See the inner exception for details.",
                            Level = "error",
                            Timestamp = DateTime.UtcNow.AddHours(-2), // Giả lập lỗi xảy ra cách đây 2 tiếng
                            DaXuLy = false,
                            Exception = @"Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes.
 ---> Npgsql.PostgresException (0x80004005): 23503: insert or update on table ""DatPhongs"" violates foreign key constraint ""FK_DatPhongs_KhachSans_MaKhachSan""
   DETAIL: Key (MaKhachSan)=(999) is not present in table ""KhachSans"".
   at Npgsql.Internal.NpgsqlConnector.<ReadMessage>g__ReadMessageLong|233_0(NpgsqlConnector connector, Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)
   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)
   at Npgsql.NpgsqlCommand.ExecuteReader(CommandBehavior behavior, Boolean async, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   --- End of inner exception stack trace ---
   at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
   at DoAn_HotelBooking.Controllers.DatPhongsController.TaoMoiDatPhong(DatPhong model) in C:\Users\Admin\source\repos\DoAn_HotelBooking\Controllers\DatPhongsController.cs:line 145"
                        },
                        new SystemLog
                        {
                            Message = "NullReferenceException: Object reference not set to an instance of an object.",
                            Level = "critical",
                            Timestamp = DateTime.UtcNow.AddDays(-1), // Lỗi từ hôm qua
                            DaXuLy = true, // Lỗi này đã được đánh dấu xử lý
                            Exception = @"System.NullReferenceException: Object reference not set to an instance of an object.
   at DoAn_HotelBooking.Controllers.TaiKhoansController.LayThongTinCaNhan(Int32 id) in C:\Users\Admin\source\repos\DoAn_HotelBooking\Controllers\TaiKhoansController.cs:line 56
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)"
                        }
                    );

                    context.SaveChanges();
                }

                context.SaveChanges();
            }
        }
    }
}
