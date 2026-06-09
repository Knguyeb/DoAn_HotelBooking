using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAn_HotelBooking.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn_HotelBooking.Controllers
{
    public class BaoCaoController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;

        public BaoCaoController(DoAn_HotelBookingContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy toàn bộ danh sách khách sạn
            var khachSanList = await _context.KhachSan.ToListAsync();

            // 2. Lấy danh sách đặt phòng thỏa mãn 2 điều kiện:
            // - Đã hoàn thành/xác nhận (TrangThaiDatPhong)
            // - VÀ Đã thanh toán (TrangThaiThanhToan)
            // Lưu ý: Hãy điều chỉnh chuỗi so sánh ("Đã thanh toán", "Hoàn thành") cho khớp với data thật của bạn
            var hoaDonList = await _context.DatPhong
                .Include(d => d.Phong)
                .ThenInclude(p => p.KhachSan)
                .Where(d => d.TrangThaiThanhToan == "Đã thanh toán")
                .ToListAsync();

            // 3. Tính tổng doanh thu toàn bộ hệ thống
            // Sử dụng luôn trường TongTien có sẵn trong bảng DatPhong
            decimal tongDoanhThu = hoaDonList.Sum(d => d.TongTien);

            // 4. Lấy điểm đánh giá trung bình theo khách sạn
            var danhGiaDict = await _context.DanhGiaKS
                .GroupBy(d => d.MaKhachSan)
                .Select(g => new
                {
                    MaKhachSan = g.Key,
                    DiemTrungBinh = g.Average(x => x.SoSao)
                })
                .ToDictionaryAsync(x => x.MaKhachSan, x => x.DiemTrungBinh);

            // 5. Kết hợp toàn bộ khách sạn + doanh thu + đánh giá
            var baoCaoList = khachSanList.Select(ks =>
            {
                // Tính doanh thu của khách sạn này dựa trên TongTien đã lưu
                var doanhThu = hoaDonList
                    .Where(d => d.Phong != null && d.Phong.MaKhachSan == ks.MaKhachSan)
                    .Sum(d => d.TongTien);

                // Tính điểm trung bình
                var diem = danhGiaDict.ContainsKey(ks.MaKhachSan)
                    ? Math.Round(danhGiaDict[ks.MaKhachSan], 1)
                    : 0;

                return new
                {
                    TenKhachSan = ks.TenKhachSan,
                    TongDoanhThu = doanhThu,
                    DiemTrungBinh = diem
                };
            })
            .OrderByDescending(x => x.TongDoanhThu) // Sắp xếp theo doanh thu cao nhất
            .ToList();

            // 6. Tính toán các chỉ số cho khách sạn dẫn đầu
            var topKhachSan = baoCaoList.FirstOrDefault(x => x.TongDoanhThu > 0);

            string tenTop = topKhachSan?.TenKhachSan ?? "Chưa có dữ liệu";
            decimal doanhThuTop = topKhachSan?.TongDoanhThu ?? 0;
            double phanTram = tongDoanhThu > 0
                ? Math.Round((double)(doanhThuTop / tongDoanhThu) * 100, 2)
                : 0;

            // 7. Truyền sang View
            ViewBag.TongDoanhThu = tongDoanhThu;
            ViewBag.TopKhachSan = tenTop;
            ViewBag.DoanhThuTop = doanhThuTop;
            ViewBag.PhanTram = phanTram;
            ViewBag.BaoCaoList = baoCaoList;

            return View();
        }
    }
}