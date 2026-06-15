using DoAn_HotelBooking.Data;
using Microsoft.AspNetCore.Mvc;

namespace DoAn_HotelBooking.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly DoAn_HotelBookingContext _context;

        public ThongBaoController(DoAn_HotelBookingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetThongBao()
        {
            var maKS = HttpContext.Session.GetString("MaKhachSan");

            var data = _context.ThongBao
                .Where(x => x.MaKhachSan == maKS)
                .OrderByDescending(x => x.NgayTao)
                .Take(20)
                .Select(x => new
                {
                    tieuDe = x.TieuDe,
                    ngayTao = x.NgayTao.ToString("dd/MM/yyyy HH:mm")
                })
                .ToList();

            return Json(data);
        }
    }
}
