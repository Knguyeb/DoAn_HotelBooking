document.addEventListener("DOMContentLoaded", function () {
    const successEl = document.getElementById("successMessage");
    const errorEl = document.getElementById("errorMessage");

    const successMessage = successEl?.value?.trim();
    const errorMessage = errorEl?.value?.trim();

    // Cấu hình chung cho Dark Mode
    const darkModeConfig = {
        background: '#1e1e1e', // Nền đen nhám
        color: '#ffffff',       // Chữ trắng
        backdrop: `rgba(0,0,0,0.4)` // Làm tối vùng bên ngoài popup
    };

    if (successMessage && successMessage !== "null" && successMessage !== "" && successMessage !== "undefined") {
        Swal.fire({
            ...darkModeConfig,
            icon: 'success',
            title: 'Thành công',
            text: successMessage,
            showConfirmButton: false,
            timer: 2000
        });
        return;
    }

    if (errorMessage && errorMessage !== "null" && errorMessage !== "" && errorMessage !== "undefined") {
        Swal.fire({
            ...darkModeConfig,
            icon: 'error',
            title: 'Lỗi',
            text: errorMessage,
            confirmButtonText: "Đóng",
            confirmButtonColor: '#ffc107' // Đổi sang màu vàng cho chuẩn Luxury
        });
        return;
    }
});