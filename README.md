# 🎬 CẨM NANG HƯỚNG DẪN SỬ DỤNG HỆ THỐNG QUẢN TRỊ 🍿
## 📘 (END-USER SYSTEM MANUAL)

Chào mừng bạn đến với hệ thống quản trị rạp chiếu phim hiện đại! Hệ thống của chúng ta được chia làm 2 phân hệ lớn tương thích với từng chức năng công việc: **👑 Quản trị viên hệ thống (System Admin)** và **🎪 Chủ rạp/Quản lý rạp (Theater Owner)**. 

Cẩm nang này sẽ dắt tay bạn đi qua từng bước thao tác trực quan nhất trên giao diện người dùng (UI). Hãy bắt đầu thôi!

---

# 👑 PHẦN 1: PHÂN HỆ QUẢN TRỊ HỆ THỐNG (SYSTEM ADMIN)

> [!NOTE]
> Phân hệ này dành riêng cho **Quản trị viên tối cao** để quản lý cơ sở hạ tầng, kết nối rạp chiếu, lập danh mục phim, cấu hình tài khoản toàn hệ thống và thiết lập ma trận vai trò/quyền hạn.

## 🧭 1. Cách Truy Cập Vào Trang Quản Trị
- **Bước 1**: 🔑 Đăng nhập vào trang chủ bằng tài khoản quản trị hệ thống của bạn.
- **Bước 2**: 👤 Di chuột vào ảnh đại diện (Avatar) của bạn ở góc trên bên phải màn hình.
- **Bước 3**: ⚙️ Chọn dòng **System Admin** từ menu thả xuống.
- **Bước 4**: Màn hình quản trị xuất hiện với **Thanh Menu bên trái (Sidebar)** gồm:
  - 🎬 **Quản lý Phim** (Danh sách và tải phim lên)
  - 📁 **Quản lý Thể Loại** (Danh mục thể loại phim)
  - 🏢 **Quản lý Rạp Chiếu** (Danh sách chuỗi rạp phim)
  - 👤 **Quản lý Tài Khoản** (Cấp phát tài khoản nhân sự)
  - 🛡️ **Quản lý Vai Trò** (Phân quyền chi tiết bằng ma trận)

---

## 🛡️ 2. Quản Lý Vai Trò và Phân Quyền (Role Matrix)
Tính năng này giúp bạn thiết kế các chức danh công việc trong hệ thống (Ví dụ: Kế toán, Nhân viên soát vé, Quản lý rạp) và gán quyền thao tác cụ thể cho họ.

- **Bước 1**: Nhấp chuột vào mục 🛡️ **Quản lý Vai Trò** ở menu bên trái.
- **Bước 2**: Nhấn nút ➕ **Thêm vai trò mới** hoặc biểu tượng ✏️ **Chỉnh sửa** trên dòng vai trò có sẵn.
- **Bước 3**: Điền thông tin cơ bản:
  - 🏷️ **Tên vai trò**: Nhập tên viết hoa không dấu (ví dụ: `QUAN_LY_PHONG_VE`, `ADMIN_RAP`...).
  - 📝 **Mô tả**: Nhập tóm tắt công việc của vai trò này.
- **Bước 4**: **Tích Chọn Bảng Ma Trận Quyền Hạn**:
  - Giao diện sẽ hiển thị một bảng lưới gồm các dòng tài nguyên (Phim, Vé, Nhân viên, Lịch chiếu...) và các cột hành động tương ứng (Xem, Thêm, Sửa, Xóa, Nhập, Xuất, Duyệt).
  - 🔲 Tích vào ô giao nhau để cấp quyền cụ thể. Ví dụ: Dòng `Phim` + Cột `Xem` nghĩa là vai trò này được phép xem phim nhưng không được sửa hoặc xóa.
  - ⚡ Bạn có thể nhấn nút **Chọn tất cả (Select All)** ở đầu dòng để cấp toàn bộ các quyền của tài nguyên đó trong một click.
- **Bước 5**: Nhấn 💾 **Lưu** để hệ thống ghi nhận cấu hình.

---

## 👤 3. Quản Lý Tài Khoản Nhân Sự (User Accounts)
Quản trị viên có toàn quyền kiểm soát danh sách tài khoản, khóa hoặc phân quyền cho thành viên mới.

- **Bước 1**: Nhấp chuột vào mục 👤 **Quản lý Tài Khoản** từ menu bên trái.
- **Bước 2**: Nhấn nút ➕ **Tạo tài khoản**.
- **Bước 3**: Nhập đầy đủ các thông tin:
  - 📧 **Username, Email và Mật khẩu** đăng nhập của nhân sự.
  - 🎭 **Gán vai trò**: Chọn vai trò công việc đã thiết lập từ trước (ví dụ: `ADMIN-THEATER`).
  - 🏢 **Liên kết rạp**: Khi gán vai trò thuộc rạp (chủ rạp, nhân viên rạp), một danh sách thả xuống sẽ xuất hiện để bạn **chọn rạp chiếu tương ứng** mà nhân sự này làm việc.
- **Bước 4**: Nhấn 💾 **Lưu**.
- **Mẹo nhanh**: Nhấp nút 🔓 **Mở khóa** hoặc 🔒 **Khóa** trên dòng tài khoản để tạm thời chặn quyền đăng nhập của nhân viên khi cần thiết.

---

## 🏢 4. Quản Lý Hệ Thống Rạp Chiếu (Theaters)
- **Bước 1**: Nhấp chọn mục 🏢 **Quản lý Rạp Chiếu** từ menu bên trái.
- **Bước 2**: Nhấn nút ➕ **Thêm rạp mới**.
- **Bước 3**: Nhập các thông tin chi tiết:
  - 🏷️ Tên rạp chiếu phim (ví dụ: Netflix Cinema Center).
  - 📍 Địa chỉ, số điện thoại, email của rạp.
  - 🖼️ Tải lên hình ảnh đại diện của rạp chiếu.
- **Bước 4**: Nhấn 💾 **Lưu**.

---

## 🎬 5. Đăng Phim Mới và Thể Loại Phim
- 📁 **Thể Loại**: Chọn mục **Quản lý Thể Loại** -> Nhấn **Thêm thể loại** để nhập các dòng phim mới (Hành động, Hoạt hình, Kinh dị...).
- 🎬 **Quản lý Phim**:
  - Nhập thông tin phim: Tên phim, thời lượng, đạo diễn, mô tả nội dung, ngày khởi chiếu.
  - 🖼️ **Tải hình ảnh**: Tải lên tệp ảnh Poster đứng (phục vụ hiển thị danh sách phim) và Banner ngang (phục vụ hiển thị trang chủ lớn).

---
---

# 🎪 PHẦN 2: PHÂN HỆ QUẢN TRỊ RẠP CHIẾU PHIM (THEATER OWNER)

> [!NOTE]
> Phân hệ này dành cho các **Chủ rạp, Quản lý chi nhánh** để thiết lập sơ đồ ghế ngồi, tạo lịch chiếu phim và quản trị trực tiếp danh sách nhân sự tại rạp của mình.

## 🧭 1. Cách Truy Cập Vào Trang Quản Trị Rạp
- **Bước 1**: 🔑 Đăng nhập tài khoản quản trị rạp của bạn tại trang chủ.
- **Bước 2**: 👤 Di chuột vào ảnh đại diện (Avatar) ở góc trên bên phải màn hình.
- **Bước 3**: 🎪 Chọn dòng **Theater Owner** từ menu thả xuống.
- **Bước 4**: Menu quản trị của rạp sẽ hiển thị ở Sidebar bên trái gồm:
  - 🚪 **Quản lý Phòng Chiếu** (Cấu hình ghế ngồi phòng chiếu)
  - 🕒 **Lịch Trình Suất Chiếu** (Tạo khung giờ chiếu phim)
  - 👥 **Quản lý Nhân Viên** (Quản trị nhân sự chi nhánh rạp)

---

## 🚪 2. Cấu Hình Phòng Chiếu và Ghế Ngồi (Rooms & Seats)
Tạo sơ đồ phòng chiếu trực quan giúp khách hàng dễ dàng đặt vé trực tuyến.

- **Bước 1**: Nhấp chọn mục 🚪 **Quản lý Phòng Chiếu** ở menu bên trái.
- **Bước 2**: Nhấn nút ➕ **Thêm phòng mới** và điền tên phòng (ví dụ: Phòng chiếu 01 - IMAX 3D).
- **Bước 3**: **Thiết lập sơ đồ ghế ngồi**:
  - Nhập số lượng hàng ghế và cột ghế mong muốn để hệ thống tự vẽ lưới ô vuông.
  - 💺 Nhấp chọn vào từng ghế hoặc quét chọn nhiều ghế để đặt loại ghế ngồi tương ứng:
    - **Ghế Thường** (màu xám mặc định).
    - **Ghế VIP** (màu vàng nổi bật - phụ thu thêm).
    - **Ghế Đôi Sweetbox** (ghế đôi nằm ở hàng cuối - phụ thu đặc biệt).
- **Bước 4**: Nhấn 💾 **Lưu** phòng chiếu.

---

## 🕒 3. Sắp Xếp Lịch Trình Suất Chiếu Phim (Showtimes)
- **Bước 1**: Nhấp chọn mục 🕒 **Lịch Trình Suất Chiếu** ở menu bên trái.
- **Bước 2**: Nhấn nút ➕ **Tạo lịch chiếu mới**.
- **Bước 3**: Thiết lập thông tin suất chiếu:
  - 🎬 **Chọn phim**: Lựa chọn bộ phim muốn phát từ danh sách có sẵn.
  - 🚪 **Chọn phòng chiếu**: Chọn phòng chiếu thích hợp.
  - 📅 **Thời gian bắt đầu**: Chọn ngày chiếu và khung giờ chiếu phim.
  - 💵 **Giá vé gốc**: Nhập mức giá vé cơ sở cho suất chiếu.
  - 🟢 **Kích hoạt**: Bật tùy chọn hoạt động để suất chiếu hiển thị lên trang bán vé cho khách hàng.
- **Bước 4**: Nhấn 💾 **Lưu**. 

> [!WARNING]
> Hệ thống thông minh sẽ tự động từ chối nếu phát hiện lịch chiếu mới bị **trùng lặp phòng chiếu** hoặc **trùng khung thời gian** với các suất chiếu đã có sẵn trong phòng đó.

---

## 👥 4. Quản Lý Đội Ngũ Nhân Viên Rạp (Theater Members)
Chủ rạp có toàn quyền tự thêm mới hoặc sa thải nhân viên trực thuộc chi nhánh quản lý của mình.

### A. ➕ Thêm nhân viên mới vào rạp:
- **Bước 1**: Nhấp chọn mục 👥 **Quản lý Nhân Viên** ở menu bên trái.
- **Bước 2**: Nhấn nút **Thêm nhân viên mới**.
- **Bước 3**: Nhập thông tin tuyển dụng:
  - 👤 **Tên đăng nhập (Username)**: Nhập chính xác Username tài khoản cá nhân của nhân viên đó (nhân viên cần đăng ký trước tài khoản trên trang web).
  - 🎭 **Vai trò**: Chọn công việc phân bổ cho nhân viên tại rạp (ví dụ: `BOX-OFFICE-THEATER` - nhân viên quầy bán vé).
- **Bước 4**: Nhấn 💾 **Lưu** để nhân viên nhận quyền làm việc tại chi nhánh.

### B. ✏️ Thay đổi vai trò hoặc ❌ Sa thải nhân viên:
- **Thay đổi vai trò**: Nhấp biểu tượng ✏️ **Sửa** cạnh tên nhân viên để phân bổ họ sang công việc khác trong rạp.
- **Sa thải nhân viên**: Nhấp nút 🗑️ **Xóa (Remove)** trên dòng của nhân viên. Tài khoản của họ sẽ lập tức bị gỡ bỏ quyền truy cập hệ thống quản trị rạp và chuyển đổi về quyền khách hàng thông thường (`USER`).
