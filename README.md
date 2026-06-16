# 🏨 Luxury Hotel Booking System

![.NET Core](https://img.shields.io/badge/.NET%20Core-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET MVC](https://img.shields.io/badge/ASP.NET%20MVC-512BD4?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Neon](https://img.shields.io/badge/Neon-00E599?style=for-the-badge&logo=neon&logoColor=black)
![Bootstrap](https://img.shields.io/badge/Bootstrap_5-7952B3?style=for-the-badge&logo=bootstrap)

Một hệ thống quản lý đặt phòng khách sạn toàn diện (Full-stack) được phát triển bằng **ASP.NET Core MVC** và **Entity Framework Core**. Dự án được xây dựng nhằm giải quyết các bài toán nghiệp vụ cốt lõi trong vận hành khách sạn: ngăn chặn đặt trùng phòng (Overbooking), tự động hóa luồng thông báo/email, tích hợp thanh toán trực tuyến và phân tích doanh thu trực quan. Cấu trúc dữ liệu được triển khai trên nền tảng Serverless Database **Neon (PostgreSQL)**.

🔗 **[Xem Live Demo Tại Đây](https://doan-hotelbooking.onrender.com/))**

> **Tài khoản trải nghiệm (Demo Accounts):**
> * **Admin (Chủ hệ thống):** `system` / `123456` *(Quản lý danh mục khách sạn, xem báo cáo doanh thu tổng, thiết lập hệ thống)*
> * **Quản lý / Nhân viên:** `kn` / `1` *(Trực quầy lễ tân: Duyệt đơn đặt phòng, xử lý Check-in/Check-out, quét mã QR thanh toán)*
> * **Khách hàng:** Đăng nhập bằng Google hoặc `cv` / `1` *(Tìm kiếm phòng, đặt chỗ, thanh toán trực tuyến, theo dõi lịch sử)*

---

## ✨ Tính năng nổi bật (Key Features)

### 1. Thuật toán ngăn chặn Overbooking (Core Logic)
* Xử lý triệt để bài toán đụng độ lịch đặt phòng bằng truy vấn LINQ tối ưu trên Entity Framework Core.
* Tự động khóa lịch phòng ngay khi khách hàng tạo đơn (`Chờ xác nhận`) và tự động giải phóng phòng khi `Hoàn thành` hoặc `Đã hủy`, đảm bảo tính toàn vẹn dữ liệu 100% trong môi trường đa người dùng.

### 2. Quản trị Doanh thu & Phân tích Dữ liệu (Dashboard)
* Giao diện Dark-theme Luxury chuẩn UI/UX dành riêng cho bộ phận quản lý.
* Tích hợp **Chart.js** trực quan hóa dữ liệu tổng doanh thu, tự động tính toán tỷ trọng doanh thu theo từng cơ sở khách sạn kết hợp bộ lọc thời gian thực tế.

### 3. Tự động hóa Thông báo & Email (Automation)
* Xây dựng luồng nhận thông báo Bất đồng bộ (AJAX & Fetch API) đẩy trực tiếp cảnh báo (đơn mới, hủy phòng) lên màn hình Admin kết hợp **SweetAlert2**.
* Tự động gửi Email xác nhận đặt phòng và biên lai điện tử (hỗ trợ tính toán % ưu đãi hạng thẻ) thông qua **Brevo SMTP API**.

### 4. Tích hợp Đăng nhập Google (SSO) & Thanh toán QR
* Áp dụng **Google OAuth 2.0** cho phép khách hàng đăng nhập nhanh chóng và bảo mật.
* Phân tách vòng đời `Trạng thái phòng` và `Trạng thái thanh toán`, quét liên tục trạng thái mã QR để cập nhật giao dịch theo thời gian thực (Real-time update).

---

## 🛠️ Công nghệ sử dụng (Tech Stack)

### Backend & Database
* **Framework:** C#, ASP.NET Core MVC.
* **ORM:** Entity Framework Core (Sử dụng provider `Npgsql`).
* **Cơ sở dữ liệu:** PostgreSQL (Triển khai Serverless trên hệ sinh thái **Neon**).
* **Bảo mật:** Tích hợp `[ValidateAntiForgeryToken]` chống CSRF, cơ chế chống SQL Injection mặc định của EF Core, mã hóa Razor chống XSS.

### Frontend
* **UI Framework:** Bootstrap 5 (Custom Dark Theme & Glassmorphism).
* **Javascript:** Vanilla JS, Fetch API, SweetAlert2 (Interactive Popups), Chart.js (Data Visualization).

---
