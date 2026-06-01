📦 src/app/
├── 📂 core/                     # Logic cốt lõi & API Services
│   ├── 📂 auth/                 # Xử lý JWT Token, Login/Register cho cả 3 Role
│   ├── 📂 guards/               # Phân quyền: UserGuard, SystemAdminGuard, TheaterAdminGuard
│   ├── 📂 interceptors/         # Gắn token, xử lý mã lỗi HTTP
│   └── 📂 services/             # BookingService, MovieService, TheaterService...
│
├── 📂 shared/                   # Các UI Component dùng chung
│   ├── 📂 components/
│   │   ├── 📂 admin-table/      # 🆕 Bảng động (Table + Search + Select Options + Actions: Edit/Active/Inactive)
│   │   │   ├── 📄 admin-table.component.ts
│   │   │   ├── 📄 admin-table.component.html
│   │   │   └── 📄 admin-table.component.scss
│   │   ├── 📂 movie-cards/      # Thẻ hiển thị poster phim (kế thừa UI/UX điện ảnh)
│   │   ├── 📂 date-picker/      # Thanh cuộn chọn ngày (14 ngày tới)
│   │   ├── 📂 seat-map/         # Sơ đồ ghế ngồi (Dùng cho User chọn ghế & Admin setup loại ghế)
│   │   └── 📂 ui-elements/      # Buttons, Modals, Badges (bo góc 4px, màu chủ đạo)
│   ├── 📂 models/               
│   │   └── 📄 table-config.ts   # 🆕 Interface định nghĩa Columns, Filters, Actions cho bảng động
│   └── 📂 pipes/                # Format tiền tệ (VND), format thời lượng phim, ngày giờ
│
├── 📂 features/                 # Các Module nghiệp vụ chính
│   │
│   ├── 📂 user/                 # 🎬 LUỒNG KHÁCH HÀNG (B2C)
│   │   ├── 📂 home/             # Trang chủ: Danh sách phim (Đang chiếu, Sắp chiếu)
│   │   ├── 📂 movie/            
│   │   │   └── 📄 detail/       # Chi tiết phim + Chọn ngày (14 ngày) + Danh sách rạp
│   │   ├── 📂 booking/          
│   │   │   ├── 📄 showtimes/    # Chọn suất chiếu (sau khi click vào rạp)
│   │   │   ├── 📄 seat-select/  # Layout room: Chọn ghế ngồi
│   │   │   └── 📄 checkout/     # Thanh toán & Xác nhận vé
│   │   └── 📂 profile/          
│   │       ├── 📄 info/         # Xem/Sửa thông tin cá nhân
│   │       ├── 📄 history/      # Lịch sử đặt vé (booking detail)
│   │       └── 📄 ticket/       # Chi tiết vé (Mã QR, thông tin ghế, rạp)
│   │
│   ├── 📂 admin-system/         # ⚙️ LUỒNG QUẢN TRỊ HỆ THỐNG (Super Admin)
│   │   ├── 📂 movies/           
│   │   │   ├── 📄 list/         # 🆕 Gọi <app-admin-table> (Search tên phim, Select Filter thể loại, Actions: Edit/Inactive)
│   │   │   └── 📄 form/         # Thêm/Sửa phim
│   │   ├── 📂 categories/       
│   │   │   ├── 📄 list/         # 🆕 Gọi <app-admin-table> (Danh sách thể loại)
│   │   │   └── 📄 form/         
│   │   ├── 📂 theaters/         
│   │   │   ├── 📄 list/         # 🆕 Gọi <app-admin-table> (Search tên rạp, Filter khu vực, Actions: Edit/Inactive)
│   │   │   └── 📄 form/         # Chỉ thêm/sửa/xóa rạp
│   │   └── 📂 accounts/         
│   │       └── 📄 list/         # 🆕 Gọi <app-admin-table> (Search email/tên, Select Filter Role, Actions: Ban/Active)
│   │
│   └── 📂 admin-theater/        # 🏢 LUỒNG QUẢN LÝ RẠP (Theater Admin)
│       ├── 📂 showtimes/        
│       │   ├── 📄 list/         # 🆕 Gọi <app-admin-table> (Search suất chiếu, Select Filter phim/ngày/phòng, Actions: Edit/Inactive)
│       │   └── 📄 form/         # Thêm/Sửa suất chiếu
│       └── 📂 rooms/            
│           ├── 📄 list/         # 🆕 Gọi <app-admin-table> (Danh sách phòng chiếu của rạp, Actions: Edit/Inactive)
│           ├── 📄 layout/       # Quyết định layout của phòng (Xếp số hàng, số cột)
│           └── 📄 seats/        # Chỉnh sửa loại ghế của từng seat (VIP, Thường, Đôi, Bảo trì)
│
├── 📄 app.routes.ts             # Định tuyến cấp cao nhất (lazy load 3 module user, admin-system, admin-theater)
└── 📄 app.config.ts             # Cấu hình providers toàn cục