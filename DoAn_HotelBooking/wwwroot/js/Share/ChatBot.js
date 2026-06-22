// ========================================================
// 1. HÀM TẢI VÀ LƯU LỊCH SỬ CHAT
// ========================================================
function loadChatHistory() {
    let savedHistory = localStorage.getItem('aiChatHistory');
    let chatBody = document.getElementById('chatbotBody');

    if (savedHistory && chatBody) {
        chatBody.innerHTML = savedHistory;
        chatBody.scrollTop = chatBody.scrollHeight;
    }
}

function saveChatHistory() {
    let chatBody = document.getElementById('chatbotBody');
    if (chatBody) {
        localStorage.setItem('aiChatHistory', chatBody.innerHTML);
    }
}

// ========================================================
// 2. XỬ LÝ SỰ KIỆN NHẤN NÚT ENTER
// ========================================================
function handleEnter(event) {
    if (event.key === 'Enter') {
        sendChat();
    }
}

// ========================================================
// 3. XỬ LÝ GỬI TIN NHẮN (GỌI API)
// ========================================================
async function sendChat() {
    const inputField = document.getElementById('chatInput');
    const chatBody = document.getElementById('chatbotBody');
    const message = inputField.value.trim();

    if (message === '') return;

    chatBody.innerHTML += `<div class="chat-message user-message">${message}</div>`;
    saveChatHistory(); // Lưu câu hỏi của khách

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

            formattedAnswer = formattedAnswer.replace(
                /\[ROOM:(\d+):(.*?)\]/g,
                `<a onclick="openRoomDetailsFromAI('/Phongs/DetailsPartial?id=$1', '$2')" 
                    class="fw-bold" style="color: #b533d6; text-decoration: none; cursor: pointer;">
                    <i class="fas fa-door-open"></i> $2
                </a>`
            );

            formattedAnswer = formattedAnswer.replace(
                /\[HOTEL:([^:]+):(.*?)\]/g,
                `<a href="/KhachSans/Details?id=$1" target="_blank" 
                    class="fw-bold" style="color: #ffc107; text-decoration: none; cursor: pointer;">
                    <i class="fas fa-building"></i> $2
                </a>`
            );

            formattedAnswer = formattedAnswer.replace(
                /giá:\s*([\d.,]+\s*VNĐ(?:\/đêm)?)/gi,
                `giá: <span class="fw-bold" style="color: #28a745;">$1</span>`
            );

            chatBody.innerHTML += `<div class="chat-message ai-message">${formattedAnswer}</div>`;
            saveChatHistory(); // Lưu câu trả lời của AI

        } else {
            chatBody.innerHTML += `<div class="chat-message ai-message text-danger">Lỗi: ${data.error}</div>`;
            saveChatHistory(); // Lưu thông báo lỗi
        }
    } catch (error) {
        document.getElementById(loadingId).remove();
        chatBody.innerHTML += `<div class="chat-message ai-message text-danger">Không thể kết nối đến Lễ tân AI. Vui lòng thử lại sau.</div>`;
        saveChatHistory(); // Lưu thông báo mất kết nối
    }

    chatBody.scrollTop = chatBody.scrollHeight;
} // <-- Kết thúc hàm sendChat tại đây, không để các hàm khác chui vào ruột nó!

// ========================================================
// 4. KHỞI ĐỘNG LẠI TRÍ NHỚ CỦA AI KHI VỪA VÀO TRANG MỚI
// ========================================================
function initChatbot() {
    loadChatHistory();
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initChatbot);
} else {
    initChatbot();
}

// ========================================================
// 5. MỞ MODAL CHI TIẾT PHÒNG
// ========================================================
window.openRoomDetailsFromAI = function (url, title) {
    let offcanvasEl = document.getElementById('chatbotOffcanvas');
    let offcanvasInstance = bootstrap.Offcanvas.getInstance(offcanvasEl);
    if (offcanvasInstance) {
        offcanvasInstance.hide();
    }

    let modalEl = document.getElementById('dynamicAjaxModal');
    let modalTitle = modalEl.querySelector('.modal-title');
    let modalBody = modalEl.querySelector('.modal-body');

    if (modalTitle) modalTitle.innerHTML = `<div class="spinner-border spinner-border-sm text-warning me-2"></div> ${title}`;
    if (modalBody) modalBody.innerHTML = `<div class="text-center py-5"><div class="spinner-border text-warning"></div><p class="mt-2">Đang tải...</p></div>`;

    let myModal = bootstrap.Modal.getOrCreateInstance(modalEl);
    myModal.show();

    fetch(url)
        .then(response => {
            if (!response.ok) throw new Error("Lỗi mạng");
            return response.text();
        })
        .then(html => {
            if (modalBody) modalBody.innerHTML = html;
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