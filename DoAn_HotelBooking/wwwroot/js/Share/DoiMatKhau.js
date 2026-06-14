function DoiMatKhau(tenDangNhap) {
    Swal.fire({
        title: 'Đổi mật khẩu',
        background: '#25252b', // Nền đen
        color: '#fff',         // Chữ trắng
        html: `
            <div style="display:flex;flex-direction:column;gap:10px;text-align:left;">
                <label for="matkhauCu" style="font-weight:600;">Mật khẩu cũ</label>
                <input id="matkhauCu" class="swal2-input" type="password" style="color: #fff; background: #3b3b41; border: 1px solid #555;">

                <label for="matkhauMoi" style="font-weight:600;">Mật khẩu mới</label>
                <input id="matkhauMoi" class="swal2-input" type="password" style="color: #fff; background: #3b3b41; border: 1px solid #555;">

                <label for="xacNhan" style="font-weight:600;">Xác nhận mật khẩu mới</label>
                <input id="xacNhan" class="swal2-input" type="password" style="color: #fff; background: #3b3b41; border: 1px solid #555;">
            </div>
        `,
        confirmButtonText: 'Xác nhận',
        showCancelButton: true,
        cancelButtonText: 'Hủy',
        focusConfirm: false,
        preConfirm: () => {
            const mkCu = document.getElementById('matkhauCu').value.trim();
            const mkMoi = document.getElementById('matkhauMoi').value.trim();
            const xn = document.getElementById('xacNhan').value.trim();

            if (!mkCu || !mkMoi || !xn) {
                Swal.showValidationMessage('⚠️ Vui lòng nhập đầy đủ thông tin!');
                return false;
            }
            if (mkMoi !== xn) {
                Swal.showValidationMessage('⚠️ Mật khẩu xác nhận không khớp!');
                return false;
            }

            return { mkCu, mkMoi };
        }
    }).then(result => {
        if (result.isConfirmed) {
            fetch('/TaiKhoans/DoiMatKhauJson', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    tenDangNhap: tenDangNhap,
                    matKhauCu: result.value.mkCu,
                    matKhauMoi: result.value.mkMoi,
                    xacNhanMatKhau: result.value.mkMoi
                })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        Swal.fire({
                            title: '✅ Thành công',
                            text: 'Mật khẩu đã được đổi. Vui lòng đăng nhập lại.',
                            icon: 'success',
                            timer: 2500,
                            showConfirmButton: false,
                            background: '#25252b', // Nền đen cho popup thành công
                            color: '#fff'
                        }).then(() => {
                            window.location.href = '/DangKy_DangNhap/DangXuat';
                        });
                    } else {
                        Swal.fire({
                            title: '❌ Lỗi',
                            text: data.message,
                            icon: 'error',
                            background: '#25252b', // Nền đen cho popup lỗi
                            color: '#fff'
                        });
                    }
                })
                .catch(() => {
                    Swal.fire({
                        title: '❌ Lỗi',
                        text: 'Không thể kết nối máy chủ',
                        icon: 'error',
                        background: '#25252b', // Nền đen cho popup lỗi mạng
                        color: '#fff'
                    });
                });
        }
    });
}