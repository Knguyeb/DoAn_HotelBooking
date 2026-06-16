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

            // ✅ BỎ QUA LOGIN + HOME
            if (controller == "DangKy_DangNhap" || controller == "Home")
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

                if (controller == "KhachSan" &&
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

                if (controller == "KhachSan" &&
                    (action == "Create" ||
                    action == "Edit" ||
                    action == "Delete" ||
                    method == "POST"))
                {
                    context.Result = new ForbidResult();
                    return;
                }

                if (controller == "Phong" &&
                    (action == "Create" ||
                    action == "Edit" ||
                    action == "Delete" ||
                    method == "POST"))
                {
                    context.Result = new ForbidResult();
                    return;
                }

                if (controller == "DatPhong" &&
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

                if (controller == "TaiKhoan" &&
                    (action == "Create" ||
                    action == "Delete" ||
                    method == "POST"))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}