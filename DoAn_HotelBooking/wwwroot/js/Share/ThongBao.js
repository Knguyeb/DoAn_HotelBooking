document.addEventListener("DOMContentLoaded", () => {
    const modal = document.getElementById("thongBaoModal");

    modal.addEventListener("show.bs.modal", async () => {
        const body = document.getElementById("thongBaoBody");

        // 1. Tắt ngay dấu chấm đỏ trên giao diện chuông cho mượt mắt
        const notiDot = document.querySelector('.noti-dot');
        if (notiDot) notiDot.style.display = 'none';

        try {
            // Hiển thị loading trong lúc đợi mạng
            body.innerHTML = `<div class="text-center text-warning py-3"><div class="spinner-border spinner-border-sm"></div> Đang tải...</div>`;

            // 2. Lấy dữ liệu thông báo
            const response = await fetch('/ThongBao/GetThongBao');
            const data = await response.json();
            let html = "";

            if (data.length === 0) {
                html = `
                    <div class="text-center text-muted py-4">
                        <i class="fas fa-box-open fs-3 mb-2 opacity-50"></i><br>
                        Không có thông báo nào
                    </div>
                `;
            }
            else {
                data.forEach(tb => {
                    // 1. Logic phân loại Icon & Màu sắc dựa theo nội dung
                    let iconHTML = '<i class="fas fa-bell text-warning"></i>'; // Mặc định nếu không khớp
                    let textContent = tb.noiDung.toLowerCase();

                    if (textContent.includes('hủy')) {
                        // Hủy phòng (Màu Đỏ)
                        iconHTML = '<i class="fas fa-calendar-times text-danger"></i>';
                    }
                    else if (textContent.includes('đặt')) {
                        // Đặt phòng (Màu Xanh lá)
                        iconHTML = '<i class="fas fa-calendar-check text-success"></i>';
                    }
                    else if (textContent.includes('thanh toán')) {
                        // Thanh toán (Màu Xanh dương sáng)
                        iconHTML = '<i class="fas fa-file-invoice-dollar text-info"></i>';
                    }

                    // 2. Phân biệt CSS tin MỚI (chưa đọc) và CŨ (đã đọc)
                    const unreadStyle = tb.daDoc ? "" : "background-color: rgba(244, 168, 37, 0.08); border-left: 4px solid #f4a825;";
                    const opacityStyle = tb.daDoc ? "opacity: 0.7;" : "opacity: 1;";

                    html += `
                        <a href="#" class="text-decoration-none d-block border-bottom py-3 px-3 noti-hover" style="transition: all 0.2s; ${unreadStyle}">
                            <div class="d-flex align-items-center gap-3" style="${opacityStyle}">
                                <div class="fs-4 mt-1" style="width: 30px; text-align: center;">
                                    ${iconHTML}
                                </div>
                
                                <div class="flex-grow-1">
                                    <div class="fw-bold text-light" style="font-size: 0.95rem;">
                                        ${tb.noiDung}
                                    </div>
                                    <small class="text-muted d-block mt-1">
                                        <i class="far fa-clock me-1"></i> ${tb.ngayTao}
                                    </small>
                                </div>

                                <div class="text-secondary noti-arrow">
                                    <i class="fas fa-chevron-right fs-6"></i>
                                </div>
                            </div>
                        </a>
                    `;
                });

                                // Thêm Footer "Xem tất cả" ở cuối danh sách
                html += `
                    <div class="text-center p-3">
                        <a href="/ThongBao/Index" class="text-warning text-decoration-none fw-bold" style="font-size: 0.9rem;">
                            Xem tất cả thông báo <i class="fas fa-arrow-right ms-1"></i>
                        </a>
                    </div>
                `;
            }
            body.innerHTML = html;

            // 3. Gọi API chạy ngầm để đánh dấu "Đã đọc toàn bộ" trong Database
            // Lệnh này không cần await để không làm chậm giao diện của người dùng
            fetch('/ThongBao/DanhDauDaDocToanBo', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            }).catch(err => console.error("Lỗi cập nhật trạng thái đọc:", err));

        }
        catch {
            body.innerHTML = `
                <div class="text-danger text-center py-3">
                    <i class="fas fa-exclamation-triangle mb-2"></i><br>
                    Không tải được thông báo. Vui lòng thử lại.
                </div>
            `;
        }
    });
});