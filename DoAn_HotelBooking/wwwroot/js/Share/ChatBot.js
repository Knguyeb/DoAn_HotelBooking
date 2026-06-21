// Bắt sự kiện nhấn Enter
function handleEnter(event) {
    if (event.key === 'Enter') {
        sendChat();
    }
}

// Xử lý gửi tin nhắn
async function sendChat() {
    const inputField = document.getElementById('chatInput');
    const chatBody = document.getElementById('chatbotBody');
    const message = inputField.value.trim();

    if (message === '') return;

    chatBody.innerHTML += `<div class="chat-message user-message">${message}</div>`;
    inputField.value = '';
    chatBody.scrollTop = chatBody.scrollHeight;

    const loadingId = 'loading-' + Date.now();
    chatBody.innerHTML += `<div id="${loadingId}" class="typing-indicator">
        AI đang suy nghĩ <i class="fas fa-spinner fa-spin ms-1"></i>
    </div>`;
    chatBody.scrollTop = chatBody.scrollHeight;

    try {
        const response = await fetch('/AI/AskChatbot', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ message: message })
        });

        const data = await response.json();
        document.getElementById(loadingId).remove();

        if (response.ok) {
            let formattedAnswer = data.answer.replace(/\n/g, '<br>');

            // Bước 4.2: Dùng Regex tìm [ROOM:id:tên] và biến thành thẻ Link gọi Modal
            formattedAnswer = formattedAnswer.replace(
                /\[ROOM:(\d+):(.*?)\]/g,
                `<a onclick="openRoomDetailsFromAI('/Phongs/DetailsPartial?id=$1', '$2')" 
                    class="fw-bold" style="color: #b533d6; text-decoration: none; cursor: pointer;">
                    <i class="fas fa-door-open"></i> $2
                </a>`
            );

            formattedAnswer = formattedAnswer.replace(
                /\[HOTEL:(.*?)\]/g,
                `<strong style="color: #ffc107 !important; font-weight: bold !important;">$1</strong>`
            );

            chatBody.innerHTML += `<div class="chat-message ai-message">${formattedAnswer}</div>`;
        } else {
            chatBody.innerHTML += `<div class="chat-message ai-message text-danger">Lỗi: ${data.error}</div>`;
        }
    } catch (error) {
        document.getElementById(loadingId).remove();
        chatBody.innerHTML += `<div class="chat-message ai-message text-danger">Không thể kết nối đến Lễ tân AI. Vui lòng thử lại sau.</div>`;
    }

    chatBody.scrollTop = chatBody.scrollHeight;
}

// ========================================================
// HÀM MỚI: Xử lý mở Modal độc lập, không lo xung đột Bootstrap
// ========================================================
window.openRoomDetailsFromAI = function (url, title) {
    // 1. (Tùy chọn) Ẩn khung Chatbot đi để nhường chỗ cho Popup chi tiết phòng
    let offcanvasEl = document.getElementById('chatbotOffcanvas');
    let offcanvasInstance = bootstrap.Offcanvas.getInstance(offcanvasEl);
    if (offcanvasInstance) {
        offcanvasInstance.hide();
    }

    // 2. Lấy Modal và thiết lập giao diện Loading
    let modalEl = document.getElementById('dynamicAjaxModal');
    let modalTitle = modalEl.querySelector('.modal-title');
    let modalBody = modalEl.querySelector('.modal-body');

    if (modalTitle) modalTitle.innerHTML = `<div class="spinner-border spinner-border-sm text-warning me-2"></div> ${title}`;
    if (modalBody) modalBody.innerHTML = `<div class="text-center py-5"><div class="spinner-border text-warning"></div><p class="mt-2">Đang tải...</p></div>`;

    // 3. Mở Modal lên ngay lập tức
    let myModal = bootstrap.Modal.getOrCreateInstance(modalEl);
    myModal.show();

    // 4. Fetch dữ liệu từ Controller và đổ vào Modal (giống hệt logic popupchitiet.js)
    fetch(url)
        .then(response => {
            if (!response.ok) throw new Error("Lỗi mạng");
            return response.text();
        })
        .then(html => {
            if (modalBody) modalBody.innerHTML = html;

            // Cập nhật lại tiêu đề nếu trong PartialView có thẻ hidden-modal-title
            const hiddenTitle = modalBody.querySelector('#hidden-modal-title');
            if (hiddenTitle && modalTitle) {
                modalTitle.innerHTML = hiddenTitle.innerHTML;
            }
        })
        .catch(error => {
            if (modalBody) modalBody.innerHTML = `
                <div class="text-center py-5 text-danger">
                    <i class="fas fa-exclamation-circle fa-4x mb-3 opacity-75"></i>
                    <h5 class="fw-bold">Lỗi tải dữ liệu</h5>
                </div>`;
        });
};