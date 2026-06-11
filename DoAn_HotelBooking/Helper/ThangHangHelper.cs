using DoAn_HotelBooking.Data;
using DoAn_HotelBooking.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn_HotelBooking.Helpers
{
    public class ThangHangHelper
    {
        private readonly DoAn_HotelBookingContext _context;

        // Tiêm (Inject) Database Context vào Helper
        public ThangHangHelper(DoAn_HotelBookingContext context)
        {
            _context = context;
        }

        public async Task KiemTraVaNangHangAsync(TaiKhoan taiKhoan)
        {
            if (taiKhoan == null) return;

            // Tìm hạng cao nhất phù hợp với điểm hiện tại
            var hangMoi = await _context.HangThanhVien
                .Where(h => h.MocDiemToiThieu <= taiKhoan.DiemTichLuy)
                .OrderByDescending(h => h.MocDiemToiThieu)
                .FirstOrDefaultAsync();

            // Nếu đủ điều kiện lên hạng
            if (hangMoi != null && taiKhoan.MaHang != hangMoi.ID)
            {
                taiKhoan.MaHang = hangMoi.ID;
                taiKhoan.HangThanhVien = hangMoi;

                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();
            }
        }
    }
}