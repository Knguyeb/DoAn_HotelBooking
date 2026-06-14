class TablePaginator {
    constructor(options) {
        // Gán các element từ ID truyền vào
        this.table = document.getElementById(options.tableId);
        this.searchInput = document.getElementById(options.searchInputId);
        this.paginationContainer = document.getElementById(options.paginationId);
        this.visibleCountLabel = document.getElementById(options.visibleCountId);
        this.noResultsDiv = document.getElementById(options.noResultsId);

        // Cấu hình
        this.rowsPerPage = options.rowsPerPage || 10;
        this.currentPage = 1;

        if (!this.table) return;

        this.tbody = this.table.querySelector("tbody");
        this.allRows = Array.from(this.tbody.querySelectorAll("tr"));
        this.filteredRows = [...this.allRows];

        this.init();
    }

    init() {
        // Bắt sự kiện tìm kiếm
        if (this.searchInput) {
            this.searchInput.addEventListener("keyup", (e) => {
                const keyword = e.target.value.toLowerCase().trim();
                this.filteredRows = this.allRows.filter(row => {
                    const text = row.innerText.toLowerCase();
                    return text.includes(keyword);
                });
                this.currentPage = 1;
                this.renderTable();
            });
        }

        // Render lần đầu
        this.renderTable();
    }

    renderTable() {
        // Ẩn tất cả các dòng
        this.allRows.forEach(row => row.style.display = "none");

        // Tính toán hiển thị
        const startIndex = (this.currentPage - 1) * this.rowsPerPage;
        const endIndex = startIndex + this.rowsPerPage;
        const rowsToShow = this.filteredRows.slice(startIndex, endIndex);

        // Hiển thị các dòng thuộc trang hiện tại
        rowsToShow.forEach(row => row.style.display = "");

        // Cập nhật số lượng hiển thị
        if (this.visibleCountLabel) {
            this.visibleCountLabel.innerText = this.filteredRows.length;
        }

        // Xử lý khi không có kết quả
        if (this.filteredRows.length === 0 && this.searchInput && this.searchInput.value.trim() !== "") {
            if (this.noResultsDiv) this.noResultsDiv.style.display = "block";
            this.table.style.display = "none";
        } else {
            if (this.noResultsDiv) this.noResultsDiv.style.display = "none";
            this.table.style.display = "table";
        }

        this.renderPagination();
    }

    renderPagination() {
        if (!this.paginationContainer) return;
        this.paginationContainer.innerHTML = "";

        const totalPages = Math.max(1, Math.ceil(this.filteredRows.length / this.rowsPerPage));

        // Hàm helper giúp tạo nhanh một thẻ <li> trang
        const createPageButton = (text, pageIndex, isActive = false, isDisabled = false) => {
            const li = document.createElement("li");
            li.className = `page-item ${isActive ? "active" : ""} ${isDisabled ? "disabled" : ""}`;
            li.innerHTML = `<a class="page-link" style="cursor: ${isDisabled ? "default" : "pointer"};">${text}</a>`;

            if (!isDisabled && pageIndex !== null) {
                li.addEventListener("click", () => {
                    this.currentPage = pageIndex;
                    this.renderTable();
                });
            }
            this.paginationContainer.appendChild(li);
        };

        // Nút Trở về trước (Prev)
        createPageButton('<i class="bi bi-chevron-left"></i>', this.currentPage - 1, false, this.currentPage === 1);

        // NẾU TỔNG SỐ TRANG <= 10: HIỂN THỊ TẤT CẢ
        if (totalPages <= 10) {
            for (let i = 1; i <= totalPages; i++) {
                createPageButton(i, i, i === this.currentPage);
            }
        }
        // NẾU TỔNG SỐ TRANG > 10: HIỂN THỊ RÚT GỌN VỚI DẤU 3 CHẤM (...)
        else {
            // Trường hợp 1: Đang ở những trang đầu (1, 2, 3...)
            if (this.currentPage <= 3) {
                for (let i = 1; i <= 4; i++) {
                    createPageButton(i, i, i === this.currentPage);
                }
                createPageButton("...", null, false, true);
                createPageButton(totalPages, totalPages, false);
            }
            // Trường hợp 2: Đang ở những trang cuối (... N-2, N-1, N)
            else if (this.currentPage >= totalPages - 2) {
                createPageButton(1, 1, false);
                createPageButton("...", null, false, true);
                for (let i = totalPages - 3; i <= totalPages; i++) {
                    createPageButton(i, i, i === this.currentPage);
                }
            }
            // Trường hợp 3: Đang ở lưng chừng (1 ... 5 6 7 ... N)
            else {
                createPageButton(1, 1, false);
                createPageButton("...", null, false, true);
                createPageButton(this.currentPage - 1, this.currentPage - 1, false);
                createPageButton(this.currentPage, this.currentPage, true);
                createPageButton(this.currentPage + 1, this.currentPage + 1, false);
                createPageButton("...", null, false, true);
                createPageButton(totalPages, totalPages, false);
            }
        }

        // Nút Tiếp theo (Next)
        createPageButton('<i class="bi bi-chevron-right"></i>', this.currentPage + 1, false, this.currentPage === totalPages);
    }

    // Hàm clear tìm kiếm gọi từ bên ngoài
    clearSearch() {
        if (this.searchInput) this.searchInput.value = "";
        this.filteredRows = [...this.allRows];
        this.currentPage = 1;
        this.renderTable();
    }
}