document.addEventListener("DOMContentLoaded", function () {
    // 1. Lắng nghe sự kiện mở của TẤT CẢ các Modal trên toàn bộ trang web
    document.addEventListener('show.bs.modal', function (event) {

        // Lấy thẻ button vừa được click để mở Modal
        const button = event.relatedTarget;
        if (!button) return;

        // KIỂM TRA: Nút bấm có chứa 'data-url' không? 
        // Nếu không có, đây là Modal bình thường -> Bỏ qua không làm gì cả.
        const fetchUrl = button.getAttribute('data-url');
        if (!fetchUrl) return;

        // Lấy Modal hiện tại đang mở và các thành phần con của nó
        const modal = event.target;
        const modalTitle = modal.querySelector('.modal-title');
        const modalBody = modal.querySelector('.modal-body');

        // Lấy tiêu đề tạm (Hỗ trợ cả 'data-title' mới và 'data-tenphong' cũ để không bị lỗi code)
        const tempTitle = button.getAttribute('data-title') || button.getAttribute('data-tenphong') || 'Đang xử lý...';

        // Hiển thị tiêu đề tạm với icon Loading nhỏ
        if (modalTitle) {
            modalTitle.innerHTML = `<div class="spinner-border spinner-border-sm text-warning me-2" role="status"></div> ${tempTitle}`;
        }

        // Hiển thị vòng xoay Loading lớn ở giữa thân Modal
        if (modalBody) {
            modalBody.innerHTML = `
                <div class="text-center py-5">
                    <div class="spinner-border text-warning" role="status" style="width: 3rem; height: 3rem;"></div>
                    <p class="mt-3 text-muted">Đang tải dữ liệu, vui lòng chờ...</p>
                </div>`;
        }

        // Gọi AJAX bằng URL lấy được từ nút bấm
        fetch(fetchUrl)
            .then(response => {
                if (!response.ok) throw new Error("Lỗi gọi dữ liệu: " + response.status);
                return response.text();
            })
            .then(html => {
                if (modalBody) {
                    // Đổ HTML nhận được vào thân Modal
                    modalBody.innerHTML = html;

                    // Tìm thẻ chứa tiêu đề ẩn trong Partial View để thay thế lên Header
                    const hiddenTitle = modalBody.querySelector('#hidden-modal-title');
                    if (hiddenTitle && modalTitle) {
                        modalTitle.innerHTML = hiddenTitle.innerHTML;
                    }

                    // (Quan trọng) Gọi lại hàm đổi tiền tệ để quy đổi giá trong bảng Popup
                    if (typeof window.updateAllPrices === 'function' && window.currentSelectedCurrency) {
                        window.updateAllPrices(window.currentSelectedCurrency);
                    }
                }
            })
            .catch(error => {
                console.error("Lỗi Fetch Popup AJAX:", error);
                if (modalBody) {
                    modalBody.innerHTML = `
                        <div class="text-center py-5 text-danger">
                            <i class="fas fa-exclamation-circle fa-4x mb-3 opacity-75"></i>
                            <h5 class="fw-bold">Không thể tải dữ liệu</h5>
                            <p class="text-muted">Đã xảy ra lỗi kết nối hoặc dữ liệu không tồn tại. Vui lòng thử lại sau.</p>
                        </div>`;
                }
            });
    });

    // 2. Dọn dẹp Modal khi đóng lại (áp dụng cho tất cả Modal)
    document.addEventListener('hidden.bs.modal', function (event) {
        const modal = event.target;

        // Chỉ dọn dẹp nếu Modal đó có body và title, tránh ảnh hưởng các modal đặc biệt khác
        const modalTitle = modal.querySelector('.modal-title');
        const modalBody = modal.querySelector('.modal-body');

        if (modalTitle) modalTitle.innerHTML = `Đang tải...`;
        if (modalBody) modalBody.innerHTML = '';
    });
});