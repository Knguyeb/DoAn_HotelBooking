function closeSidebar() {
    const sidebar = document.getElementById('adminSidebar');
    const main = document.getElementById('adminMain');
    const overlay = document.getElementById('sidebarOverlay');

    if (sidebar) sidebar.classList.remove('show');
    if (main) main.classList.remove('shifted');
    if (overlay) overlay.classList.remove('show');
}

function toggleSidebar() {
    const sidebar = document.getElementById('adminSidebar');
    const main = document.getElementById('adminMain');
    const overlay = document.getElementById('sidebarOverlay');

    if (sidebar) sidebar.classList.toggle('show');
    if (main) main.classList.toggle('shifted');
    if (overlay) overlay.classList.toggle('show');
}

// Tự động đóng khi click ra ngoài (hỗ trợ mobile & desktop)
document.addEventListener('click', function (event) {
    const sidebar = document.getElementById('adminSidebar');
    const toggleBtn = document.querySelector('.header-toggle-btn');

    if (sidebar && toggleBtn &&
        sidebar.classList.contains('show') &&
        !sidebar.contains(event.target) &&
        !toggleBtn.contains(event.target)) {
        closeSidebar();
    }
});