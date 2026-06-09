// Hàm này giờ đây nhận 4 tham số từ HTML truyền vào
function showDetail(id, fetchUrl, currentRole, tenKhachSan) {
    fetch(fetchUrl + '/' + id)
        .then(response => {
            if (!response.ok) throw new Error("Không tìm thấy dữ liệu");
            return response.json();
        })
        .then(data => {
            let khachSanHtml = "";
            // Nếu không phải khách hàng thì hiển thị thêm dòng Khách sạn
            if (data.quyenHan !== "Khách hàng") {
                // Ưu tiên dùng tenKhachSan truyền từ C# session, nếu không có thì lấy từ database
                let tenKS = tenKhachSan || data.khachSan || "Không có";
                khachSanHtml = `<p><b>Khách sạn:</b> ${tenKS}</p>`;
            }

            const canEdit = true;
            const cancelText = (currentRole === "Khách hàng") ? "Sửa" : "Đổi mật khẩu";

            Swal.fire({
                title: "Thông tin tài khoản",
                html: `
                    <div style="text-align: left; padding: 10px;">
                        <p><b>Họ và tên:</b> <span class="text-warning">${data.hoVaTen}</span></p>
                        <p><b>Tên đăng nhập:</b> ${data.tenDangNhap}</p>
                        <p><b>Email:</b> ${data.email ?? "Không có"}</p>
                        <p><b>SĐT:</b> ${data.soDienThoai ?? "Không có"}</p>
                        <p><b>Quyền hạn:</b> <span class="badge bg-danger">${data.quyenHan}</span></p>
                        ${khachSanHtml}
                    </div>
                `,
                icon: "info",
                showCancelButton: canEdit,
                cancelButtonText: cancelText,
                confirmButtonText: "Đóng",
                reverseButtons: true,
                background: '#25252b',
                color: '#fff',
                confirmButtonColor: '#FF4500'
            }).then((result) => {
                if (result.dismiss === Swal.DismissReason.cancel && canEdit) {
                    if (currentRole === "Khách hàng") {
                        window.location.href = "/TaiKhoans/Edit/" + id +
                            "?returnUrl=" + encodeURIComponent(window.location.pathname);
                    } else {
                        // Gọi hàm từ file doimatkhau.js
                        if (typeof DoiMatKhau === "function") {
                            DoiMatKhau(data.tenDangNhap);
                        }
                    }
                }
            });
        })
        .catch(error => {
            Swal.fire({ title: "Lỗi", text: error.message, icon: "error", background: '#25252b', color: '#fff' });
        });
}