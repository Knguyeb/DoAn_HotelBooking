using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace DoAn_HotelBooking.Security
{
    public class ChanQuyen : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            var method = context.HttpContext.Request.Method;

            // ✅ Các trang public không cần đăng nhập
            if (controller == "DangKy_DangNhap" || controller == "Home")
                return;

            if (controller == "KhachSans" &&
               (action == "Details"))
                return;

            if (controller == "Phongs" &&
               (action == "DetailsPartial" || action == "TatCaPhong"))
                return;

            if (controller == "DanhGiaPhongs" &&
                (action == "GetBinhLuanByPhong"))
                return;

            var session = context.HttpContext.Session;
            var quyenHan = session.GetString("QuyenHan");

            // ❌ Chưa đăng nhập → về login
            if (string.IsNullOrEmpty(quyenHan))
            {
                context.Result = new RedirectToActionResult(
                    "DangNhap",
                    "DangKy_DangNhap",
                    null);

                return;
            }

            // ===== LUẬT CHO QUẢN LÝ VÀ NHÂN VIÊN =====
            if (quyenHan == "Quản lý" || quyenHan == "Nhân viên")
            {
                if (controller == "BaoCao")
                {
                    context.Result = new ForbidResult();
                    return;
                }

                if (controller == "KhachSans" &&
                    (action == "Create" ||
                    action == "Edit" ||
                    action == "Delete" ||
                    method == "POST"))
                {
                    context.Result = new ForbidResult();
                    return;
                }

                if (controller == "HangThanhViens")
                {
                    context.Result = new ForbidResult();
                    return;
                }

                if (controller == "SystemLogs")
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            // ===== LUẬT CHO KHÁCH HÀNG =====
            if (quyenHan == "Khách hàng")
            {
                if (controller == "BaoCao" ||
                    controller == "HangThanhViens")
                {
                    context.Result = new ForbidResult();
                    return;
                }

                if (controller == "KhachSans" &&
                    (action == "Create" ||
                    action == "Edit" ||
                    action == "Delete" ||
                    method == "POST"))
                {
                    context.Result = new ForbidResult();
                    return;
                }

                if (controller == "Phongs" &&
                    (action == "Create" ||
                    action == "Edit" ||
                    action == "Delete" ||
                    method == "POST"))
                {
                    context.Result = new ForbidResult();
                    return;
                }

                if (controller == "DatPhongs" &&
                    (action == "Edit" ||
                    action == "Delete" ||
                    action == "XacNhan" ||
                    action == "CheckIn" ||
                    action == "CheckOut" ||
                    method == "Delete" ||
                    method == "POST"))
                {
                    context.Result = new ForbidResult();
                    return;
                }

                if (controller == "TaiKhoans" &&
                    (action == "Create" ||
                    action == "Delete" ||
                    method == "POST"))
                {
                    context.Result = new ForbidResult();
                    return;
                }

                if (controller == "SystemLogs")
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}