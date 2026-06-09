// Truyền tham số logoutUrl từ HTML vào để file JS biết đường dẫn cần gọi
function confirmLogoutSwal(logoutUrl) {
    Swal.fire({
        title: 'Bạn có chắc chắn?',
        text: "Bạn muốn đăng xuất khỏi hệ thống?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#FF4500',
        cancelButtonColor: '#32323b',
        confirmButtonText: 'Đăng xuất',
        cancelButtonText: 'Hủy',
        background: '#25252b',
        color: '#fff'
    }).then((result) => {
        if (result.isConfirmed) {
            window.location.href = logoutUrl;
        }
    });
    return false;
}