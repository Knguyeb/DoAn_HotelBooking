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
                        DiaChi = "Nguyễn Huệ, Bến Nghé, Quận 1, Hồ Chí Minh",
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
                        DiaChi = "22-36 Nguyễn Huệ, Bến Nghé, Quận 1, Hồ Chí Minh",
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
                            GiaPhong = 1800000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/phong1.jpg; /Images/phong2.jpg",
                            MaKhachSan = "AAA000",
                            MoTa = "Phòng tiêu chuẩn ấm cúng mang nét kiến trúc cổ điển Pháp. Với cửa sổ hướng phố Đồng Khởi, không gian cung cấp sự thoải mái trọn vẹn để du khách thư giãn sau ngày dài dạo bước quanh trung tâm Quận 1."
                        },
                        new Phong
                        {
                            SoPhong = 102,
                            Tang = 1,
                            LoaiPhong = "Deluxe",
                            SucChua = 2,
                            GiaPhong = 2500000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/phong3.jpg; /Images/phong4.jpg",
                            MaKhachSan = "AAA000",
                            MoTa = "Phòng Deluxe rộng rãi, thiết kế sang trọng pha lẫn nét hoài cổ. Trang bị đầy đủ tiện nghi cao cấp bao gồm bồn tắm nằm và khu vực ghế sofa thư giãn, tận hưởng nhịp sống nhộn nhịp của Bến Nghé."
                        },
                        new Phong
                        {
                            SoPhong = 201,
                            Tang = 2,
                            LoaiPhong = "VIP",
                            SucChua = 4,
                            GiaPhong = 6500000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/phong5.jpg; /Images/phong6.webp",
                            MaKhachSan = "AAA001",
                            MoTa = "Trải nghiệm không gian sống thượng lưu tại khu vực An Khánh với phòng VIP đẳng cấp. Nội thất hiện đại, không gian riêng tư tối đa và ban công thoáng đãng với tầm nhìn bao quát Xa lộ Hà Nội."
                        },
                        new Phong
                        {
                            SoPhong = 501,
                            Tang = 5,
                            LoaiPhong = "President",
                            SucChua = 6,
                            GiaPhong = 25000000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/phong7.jpg; /Images/phong8.jpg",
                            MaKhachSan = "AAA001",
                            MoTa = "Căn hộ Tổng thống (Presidential Suite) độc bản nằm trên tầng cao nhất. Cung cấp không gian xa hoa, dịch vụ tiện ích cá nhân 24/7 và hệ thống cách âm tuyệt đối để tận hưởng kỳ nghỉ thượng hạng."
                        },
                        new Phong
                        {
                            SoPhong = 501,
                            Tang = 5,
                            LoaiPhong = "President",
                            SucChua = 4,
                            GiaPhong = 35000000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/srt1.jpg; /Images/srt2.jpg",
                            MaKhachSan = "SRT101",
                            MoTa = "Căn Tổng thống đỉnh cao tọa lạc ngay giữa lòng Cần Thơ nhộn nhịp. Tận hưởng đặc quyền với tầm nhìn panorama ôm trọn dòng sông Hậu thơ mộng, quầy bar mini riêng và không gian phòng khách tinh xảo."
                        },
                        new Phong
                        {
                            SoPhong = 501,
                            Tang = 5,
                            LoaiPhong = "President",
                            SucChua = 6,
                            GiaPhong = 55000000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/srt1.jpg; /Images/srt2.jpg",
                            MaKhachSan = "SRT202",
                            MoTa = "Tuyệt tác Tổng thống sát bờ biển Bãi Dài hoang sơ. Mang đến đặc quyền nghỉ dưỡng đẳng cấp quốc tế với hồ bơi vô cực ngắm hoàng hôn Phú Quốc, sân hiên tắm nắng riêng và dịch vụ quản gia cao cấp."
                        },
                        new Phong
                        {
                            SoPhong = 201,
                            Tang = 2,
                            LoaiPhong = "VIP",
                            SucChua = 2,
                            GiaPhong = 8500000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/rex1.jpg; /Images/rex2.webp",
                            MaKhachSan = "REX101",
                            MoTa = "Phòng VIP phong cách Đông Dương ngay trên phố đi bộ Nguyễn Huệ hoa lệ. Không gian bài trí tinh tế, hoa tươi được thay mới mỗi ngày cùng cửa kính lớn đón trọn nhịp đập sôi động của thành phố không ngủ."
                        },
                        new Phong
                        {
                            SoPhong = 102,
                            Tang = 1,
                            LoaiPhong = "Deluxe",
                            SucChua = 2,
                            GiaPhong = 3200000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/lvl1.jpg; /Images/lvl2.jpg",
                            MaKhachSan = "LVL101",
                            MoTa = "Không gian Deluxe đương đại nổi bật trên trục Nam Kỳ Khởi Nghĩa sầm uất. Sự kết hợp hoàn hảo giữa thiết kế sang trọng, giường king-size chuẩn quốc tế và tiện ích giải trí ngay tại Quận 3."
                        },
                        new Phong
                        {
                            SoPhong = 101,
                            Tang = 1,
                            LoaiPhong = "Standard",
                            SucChua = 2,
                            GiaPhong = 3800000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/new1.jpg; /Images/new2.jpg",
                            MaKhachSan = "NEW101",
                            MoTa = "Phòng Standard tối ưu hóa công năng và mang lại sự yên tĩnh tuyệt đối ngay cạnh không khí tấp nập của khu vực chợ Bến Thành. Sự lựa chọn hoàn hảo và cực kỳ thuận tiện để tham quan các di tích lân cận."
                        },
                        new Phong
                        {
                            SoPhong = 501,
                            Tang = 5,
                            LoaiPhong = "President",
                            SucChua = 4,
                            GiaPhong = 45000000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/srt1.jpg; /Images/srt2.jpg",
                            MaKhachSan = "MEL101",
                            MoTa = "Căn phòng biểu tượng của sự uy quyền ngay trung tâm Lý Thường Kiệt. Thể hiện sự hòa quyện giữa di sản văn hóa Hà Nội cổ kính và thiết kế lộng lẫy, trang bị công nghệ phòng ngủ tiên tiến nhất."
                        },
                        new Phong
                        {
                            SoPhong = 501,
                            Tang = 5,
                            LoaiPhong = "President",
                            SucChua = 4,
                            GiaPhong = 95000000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/srt1.jpg; /Images/srt2.jpg",
                            MaKhachSan = "PKH101",
                            MoTa = "Đẳng cấp không thể chối từ tại Công trường Lam Sơn, Quận 1. Căn Tổng thống cung cấp trải nghiệm hoàng gia thực thụ với những bức tranh nghệ thuật sơn mài đắt giá và đặc quyền thượng lưu tối thượng."
                        },
                        new Phong
                        {
                            SoPhong = 501,
                            Tang = 5,
                            LoaiPhong = "President",
                            SucChua = 4,
                            GiaPhong = 150000000,
                            TrangThai = "Còn trống",
                            HinhAnh = "/Images/srt1.jpg; /Images/srt2.jpg",
                            MaKhachSan = "RVR101",
                            MoTa = "Đỉnh cao của sự phồn hoa với nội thất dát vàng thủ công nhập khẩu từ Ý. Nằm trên tầng cao đường Nguyễn Huệ, phòng Tổng thống này thu trọn vẻ đẹp lộng lẫy của sông Sài Gòn qua những ô kính chạm trần kỳ vĩ."
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