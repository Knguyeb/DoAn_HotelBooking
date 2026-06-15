$(document).ready(function () {
    // Dùng Event Delegation để bắt sự kiện ổn định
    $(document).on("click", "#btnDanhGiaPopup, .btnDanhGiaPhong", function (e) {
        e.preventDefault();
        const maPhong = $(this).data("maphong");

        if (!maPhong) {
            console.error("Không tìm thấy mã phòng (data-maphong)!");
            return;
        }

        // 🌟 KIỂM TRA TRƯỚC KHI MỞ FORM ĐÁNH GIÁ PHÒNG
        $.ajax({
            url: '/DanhGiaPhongs/KiemTraDanhGia',
            type: 'GET',
            data: { maPhong: parseInt(maPhong) },
            success: function (checkRes) {

                // Nếu chưa đăng nhập hoặc ĐÃ ĐÁNH GIÁ RỒI -> Quăng cảnh báo
                if (!checkRes.hopLe) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Thông báo',
                        text: checkRes.message,
                        background: '#242526',
                        color: '#fff',
                        confirmButtonColor: '#FF4500'
                    });
                    return; // Dừng lại, không mở form 5 sao nữa
                }

                // 🌟 NẾU HỢP LỆ -> MỞ FORM 5 SAO BÌNH THƯỜNG
                Swal.fire({
                    title: "Chọn số sao đánh giá phòng",
                    background: '#242526', // Màu nền tối đồng bộ theme
                    color: '#fff',
                    html: `
                        <form class="rating" id="starRatingForm">
                            ${[5, 4, 3, 2, 1].map(i => `
                                <input class="rating__input" type="radio" id="ratingPhong-${i}" name="rating" value="${i}">
                                <label class="rating__label" for="ratingPhong-${i}" aria-label="${i} sao">
                                    <svg class="rating__star" width="36" height="36" viewBox="0 0 32 32" aria-hidden="true">
                                        <g transform="translate(16,16)">
                                            <circle class="rating__star-ring" fill="none" stroke="#000" stroke-width="16" r="8" transform="scale(0)" />
                                        </g>
                                        <g stroke="#000" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                            <g transform="translate(16,16) rotate(180)">
                                                <polygon class="rating__star-stroke" 
                                                    points="0,15 4.41,6.07 14.27,4.64 7.13,-2.32 8.82,-12.14 0,-7.5 -8.82,-12.14 -7.13,-2.32 -14.27,4.64 -4.41,6.07" 
                                                    fill="none"/>
                                                <polygon class="rating__star-fill" 
                                                    points="0,15 4.41,6.07 14.27,4.64 7.13,-2.32 8.82,-12.14 0,-7.5 -8.82,-12.14 -7.13,-2.32 -14.27,4.64 -4.41,6.07" 
                                                    fill="#ccc"/>
                                            </g>
                                        </g>
                                    </svg>
                                </label>
                            `).join("")}
                        </form>

                        <div id="ratingText" style="text-align:center;margin-top:10px;font-weight:bold;color:#f4a825;font-size:18px;">
                            Hãy chọn số sao đánh giá
                        </div>

                        <style>
                            .rating { display: flex; flex-direction: row-reverse; justify-content: center; gap: 6px; }
                            .rating__input { display: none; }
                            .rating__label { cursor: pointer; transition: transform 0.2s ease; }
                            .rating__label:hover { transform: scale(1.2); }
                            .rating__star-fill { fill: #ccc; transition: fill 0.3s ease; }
                            .rating__input:checked ~ .rating__label .rating__star-fill,
                            .rating__label:hover ~ .rating__label .rating__star-fill { fill: #f4a825; }
                            .rating__input:checked + .rating__label .rating__star-fill { animation: glow 0.4s ease forwards; }
                            @keyframes glow {
                                0% { filter: drop-shadow(0 0 0px #f4a825); }
                                50% { filter: drop-shadow(0 0 8px #f4a825); }
                                100% { filter: drop-shadow(0 0 0px #f4a825); }
                            }
                        </style>
                    `,
                    confirmButtonText: "Lưu đánh giá",
                    showCancelButton: true,
                    cancelButtonText: "Hủy",
                    confirmButtonColor: '#FF4500',
                    didOpen: () => {
                        const texts = { 1: "Rất tệ 😡", 2: "Tệ 😞", 3: "Bình thường 😐", 4: "Tốt 😊", 5: "Tuyệt vời 🤩" };
                        const ratingText = document.getElementById("ratingText");
                        const inputs = document.querySelectorAll('input[name="rating"]');
                        inputs.forEach(input => {
                            const label = input.nextElementSibling;
                            label.addEventListener("mouseenter", () => { ratingText.textContent = texts[input.value]; });
                            input.addEventListener("change", () => { ratingText.textContent = texts[input.value]; });
                        });
                    },
                    preConfirm: () => {
                        const soSao = $('input[name="rating"]:checked').val();
                        if (!soSao) {
                            Swal.showValidationMessage("Vui lòng chọn số sao!");
                            return false;
                        }
                        return soSao;
                    }
                }).then(result => {
                    if (result.isConfirmed) {
                        $.ajax({
                            url: '/DanhGiaPhongs/CreateFromSwal',
                            method: 'POST',
                            contentType: 'application/json',
                            data: JSON.stringify({
                                MaPhong: parseInt(maPhong),
                                SoSao: parseInt(result.value)
                            }),
                            success: function (res) {
                                // Bắt thông báo trả về (ưu tiên message thành công, nếu thất bại thì bắt errorMessage)
                                let msg = res.message || res.errorMessage;
                                Swal.fire({
                                    icon: res.success ? 'success' : 'error',
                                    title: msg,
                                    timer: 1800,
                                    showConfirmButton: false,
                                    background: '#242526',
                                    color: '#fff'
                                }).then(() => {
                                    if (res.success) location.reload();
                                });
                            },
                            error: function (xhr, status, error) {
                                let message = xhr.responseJSON?.errorMessage || xhr.responseJSON?.message || xhr.responseText || error || "Lỗi không xác định!";
                                Swal.fire({
                                    icon: "error",
                                    title: "Lỗi (" + xhr.status + ")",
                                    text: message,
                                    background: '#242526',
                                    color: '#fff',
                                    confirmButtonColor: '#FF4500'
                                });
                            }
                        });
                    }
                });

            },
            error: function () {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Không thể kiểm tra thông tin đánh giá!',
                    background: '#242526', color: '#fff', confirmButtonColor: '#FF4500'
                });
            }
        });
    });
});