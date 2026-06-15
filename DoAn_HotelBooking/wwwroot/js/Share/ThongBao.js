document.addEventListener("DOMContentLoaded", () => {

    const modal = document.getElementById("thongBaoModal");

    modal.addEventListener("show.bs.modal", async () => {

        const body = document.getElementById("thongBaoBody");

        try {

            const response = await fetch('/ThongBao/GetThongBao');

            const data = await response.json();

            let html = "";

            if (data.length === 0) {

                html = `
                    <div class="text-center text-muted">
                        Không có thông báo nào
                    </div>
                `;
            }
            else {

                data.forEach(tb => {

                    html += `
                        <div class="border-bottom py-2">
                            <div class="fw-bold">
                                ${tb.tieuDe}
                            </div>

                            <small class="text-muted">
                                ${tb.ngayTao}
                            </small>
                        </div>
                    `;
                });
            }

            body.innerHTML = html;

        }
        catch {

            body.innerHTML = `
                <div class="text-danger text-center">
                    Không tải được thông báo
                </div>
            `;
        }

    });

});