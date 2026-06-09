// 🔝 Nút cuộn lên đầu trang
document.addEventListener("DOMContentLoaded", function () {
    const btnScrollTop = document.getElementById("btnScrollTop");

    if (!btnScrollTop) return; // Không có nút thì thoát

    // Ẩn/hiện nút khi cuộn
    window.addEventListener("scroll", () => {
        if (window.scrollY > 200) {
            btnScrollTop.classList.add("show");
        } else {
            btnScrollTop.classList.remove("show");
        }
    });

    // Cuộn mượt lên đầu trang
    btnScrollTop.addEventListener("click", () => {
        window.scrollTo({ top: 0, behavior: "smooth" });
    });
});