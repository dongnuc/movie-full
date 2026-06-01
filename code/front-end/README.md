# 🎬 Cinema Project - Angular 18 Client

Dự án Frontend được xây dựng bằng **Angular 18** và **Tailwind CSS**, thiết kế theo phong cách Netflix tối giản và sang trọng.

---

## 🚀 Tính năng & Công nghệ đã Thiết Lập

1. **Angular 18 Standalone**: Khởi tạo cấu trúc dự án hiện đại không sử dụng NgModules.
2. **State Management với Signals**: Quản lý trạng thái đăng nhập, phân quyền người dùng thông qua hệ thống Angular Signals phản ứng trực tiếp.
3. **Phân quyền dựa trên vai trò (Role-based Guards)**:
   - `UserGuard`: Bảo vệ các tính năng đặt vé và thông tin cá nhân.
   - `SystemAdminGuard`: Bảo vệ phân hệ quản trị hệ thống (Super Admin).
   - `TheaterOwnerGuard`: Bảo vệ phân hệ quản trị rạp (Theater Owner).
4. **Hệ thống Interceptors**:
   - `AuthInterceptor`: Tự động gắn Token JWT vào Header và tự động thực hiện Refresh Token ngầm khi Token hết hạn để duy trì phiên đăng nhập.
   - `ErrorInterceptor`: Bắt lỗi HTTP tập trung và định dạng lại thông báo lỗi thân thiện.
5. **Tailwind CSS**: Cấu hình bảng màu chuẩn Netflix:
   - Canvas Background: `#141414` (netflix-black)
   - Primary Brand Red: `#e50914` (netflix-red)
   - Font: Netflix Sans

---

## 📂 Cấu Trúc Thư Mục

```text
src/app/
├── core/                     # Logic cốt lõi & API Services
│   ├── auth/                 # AuthService xử lý JWT Token, Login/Register
│   ├── guards/               # Các Route Guards phân quyền (User, Admin, Owner)
│   ├── interceptors/         # Gắn Token (Auth) & Bắt lỗi (Error)
│   └── services/             # API Services
├── shared/                   # Các UI Component / Pipe dùng chung
│   ├── components/
│   │   ├── admin-table/      # Bảng động dùng chung cho toàn bộ Admin pages
│   │   └── unauthorized/     # Trang lỗi 403 khi truy cập trái phép
│   ├── models/               # Interface dùng chung (TableConfig...)
│   └── pipes/                # Pipes format tiền tệ, duration...
└── features/                 # Các Module nghiệp vụ chính
    ├── user/                 # Luồng khách hàng (Homepage, Movie Detail, Booking)
    ├── admin-system/         # Quản trị hệ thống (Phim, Thể loại, Rạp, Tài khoản)
    └── admin-theater/        # Quản lý rạp (Phòng chiếu, Suất chiếu, Ghế ngồi)
```

---

## 🛠️ Cài Đặt & Phát Triển

### Khởi chạy Client ở chế độ Development

```bash
# Di chuyển vào thư mục client
cd code/front-end

# Cài đặt thư viện nếu cần
npm install

# Chạy server development local
npm run start
```

Mở trình duyệt truy cập: `http://localhost:4200`

### Biên dịch production

```bash
npm run build
```
Bản build sẽ được xuất ra thư mục `dist/client`.
