# 🎬 PHÂN TÍCH & ĐÁNH GIÁ DỰ ÁN CINEMA (Movie_Owner)

---

## 1. TỔNG QUAN DỰ ÁN

| Hạng mục | Chi tiết |
|---|---|
| **Loại** | Dự án cá nhân – Rạp chiếu phim online |
| **Backend** | C# .NET 8, Clean Architecture, Minimal API (Carter) |
| **Frontend** | Angular 17 |
| **Database** | SQL Server (Code First – EF Core) |
| **Cache** | Redis (data cache + LUA scripts cho holding seat) |
| **Realtime** | SignalR (broadcast seat status tới client) |
| **Kiến trúc** | Clean Architecture, CQRS (Command/Query), Mediator pattern |

---

## 2. PHÂN TÍCH KIẾN TRÚC DỰ ÁN

### 2.1 Sơ đồ Layer

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│         (API Handlers, Carter Modules, DTOs)            │
├─────────────────────────────────────────────────────────┤
│                   Application Layer                      │
│   (UseCases, Commands, Queries, Behaviors, Mappers)     │
├─────────────────────────────────────────────────────────┤
│              Infrastructure + Persistence                │
│     (Repository Impl, Redis, SignalR, EF Core)          │
├─────────────────────────────────────────────────────────┤
│                     Domain Layer                         │
│       (Entities, IRepository interfaces, Events)        │
└─────────────────────────────────────────────────────────┘
         ↕ Contract Layer (shared types, messages)
```

### 2.2 Cấu trúc Projects

```
CinemaApp/
├── API/                     → DI, Middleware, Program.cs
├── Application/             → UseCases, Abstractions, Behaviors, Mappers
├── Contract/                → ICommand, IQuery, Result<T>, Shared messages
├── Domain/                  → Entities, IRepository, Domain Exceptions
├── Infrastructure/          → Redis, SignalR, Email services
├── Persistence/             → EF Core, Configurations, Migrations, Repositories
└── Presentation/            → Carter Modules (Handlers + API endpoints)
```

---

## 3. DANH SÁCH ENTITY PHÂN TÍCH

Dựa trên usecase, các entity chính cần thiết:

| Entity | Mô tả | Quan hệ |
|---|---|---|
| **Theater** | Rạp chiếu phim | has many Room |
| **Room** | Phòng chiếu trong rạp | belongs to Theater, has many Seat, has many Showing |
| **Seat** | Ghế trong phòng | belongs to Room |
| **Movie** | Phim | has many Category, has many Showing |
| **Category** | Thể loại phim | has many Movie |
| **Showing** | Suất chiếu | belongs to Room, belongs to Movie |
| **Booking** | Đơn đặt vé | belongs to User, has many BookingDetail |
| **BookingDetail** | Chi tiết vé | belongs to Booking, belongs to Seat, belongs to Showing |
| **User** | Người dùng / Theater Owner | – |

---

## 4. ĐÁNH GIÁ ĐIỂM MẠNH

✅ **Clean Architecture rõ ràng** – tách biệt Domain, Application, Infrastructure, Presentation giúp code dễ test, dễ maintain.

✅ **CQRS pattern** – phân chia Command/Query rõ ràng, phù hợp với các nghiệp vụ phức tạp như booking, holding seat.

✅ **Minimal API với Carter** – giảm boilerplate, code endpoint gọn gàng, dễ group route theo module.

✅ **Redis cho seat management** – sử dụng LUA script để đảm bảo atomic operation khi hold seat, tránh race condition.

✅ **SignalR broadcast** – UX tốt, người dùng thấy trạng thái ghế realtime mà không cần polling.

✅ **Version tag cache** – chiến lược invalidation cache thông minh cho layout phòng.

---

## 5. ĐÁNH GIÁ ĐIỂM CẦN LƯU Ý / RỦI RO

⚠️ **Session holding seat** – cần xác định rõ TTL mặc định cho session, và cơ chế extend khi chuyển sang trạng thái "paying". Phải có Background Job cleanup để tránh ghế bị giữ mãi.

⚠️ **Đồng bộ Redis ↔ SQL Server** – khi booking thành công cần đảm bảo cập nhật cả Redis lẫn DB trong 1 transaction logic (ít nhất eventual consistency).

⚠️ **Authorization phân tầng** – cần phân biệt rõ 3 role: `SystemAdmin`, `TheaterOwner`, `User`. Middleware/Policy phải được cấu hình chặt chẽ.

⚠️ **Fluent API configuration** – mỗi entity cần 1 file riêng, tránh để configuration trực tiếp trong DbContext.

⚠️ **Mapper** – nên dùng manual mapper hoặc Mapster thay vì AutoMapper để tránh overhead reflection ảnh hưởng performance trong các query phân trang.

---

## 6. QUY TRÌNH LUỒNG CODE BẮT BUỘC

```
[Domain Entity + IRepository]
         ↓
[Persistence: Configuration + Migration]
         ↓
[Persistence: Repository Implementation]
         ↓
[Application: Command/Query + Handler (UseCase)]
         ↓
[Presentation: Carter Module Handler + Endpoint]
         ↓
[Test end-to-end]
```

> **Quy tắc vàng**: KHÔNG viết UseCase trước khi có Repository. KHÔNG viết API trước khi có UseCase.

---

## 7. CÁC FILE KẾ HOẠCH ĐI KÈM

| File | Nội dung |
|---|---|
| `01_ARCHITECTURE_RULES.md` | Quy tắc code, nơi khai báo interface, method |
| `02_REPOSITORY_PLAN.md` | Kế hoạch chi tiết Repository layer |
| `03_USECASE_PLAN.md` | Kế hoạch chi tiết UseCase (Command/Query) |
| `04_API_HANDLER_PLAN.md` | Kế hoạch chi tiết API + Carter Handler |
| `05_TIEN_DO.md` | File theo dõi tiến độ từng bước |
