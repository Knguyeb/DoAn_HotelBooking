function confirmDelete(id) {
    Swal.fire({
        title: 'Bạn có chắc muốn xóa?',
        text: "Dữ liệu sẽ không thể khôi phục!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545', // Màu đỏ sang trọng cho nút Xóa
        cancelButtonColor: '#6c757d',  // Màu xám trung tính cho nút Hủy
        confirmButtonText: 'Xóa ngay',
        cancelButtonText: 'Hủy bỏ',
        background: '#1e1e1e',         // Nền đen nhám đồng bộ hệ thống
        color: '#ffffff',               // Chữ trắng
        backdrop: `rgba(0,0,0,0.6)`    // Làm tối nền phía sau khi hiện popup
    }).then((result) => {
        if (result.isConfirmed) {
            // Hiển thị trạng thái đang xử lý để tăng trải nghiệm người dùng
            Swal.fire({
                title: 'Đang xử lý...',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                },
                background: '#1e1e1e',
                color: '#ffffff'
            });

            document.getElementById('deleteForm-' + id).submit();
        }
    });
}