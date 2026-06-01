# ✅ DANH SÁCH CÔNG VIỆC – CINEMA PROJECT (Movie_Owner)

> **Cách dùng**: Đánh dấu `[x]` khi hoàn thành, `[/]` đang làm, `[ ]` chưa bắt đầu.  
> **Cập nhật lần cuối**: 2026-05-29  
> **Quy tắc**: Chỉ chuyển sang bước tiếp theo khi bước hiện tại hoàn thành.

---

## 🔷 GIAI ĐOẠN 0 – SETUP DỰ ÁN (5/6)

- [x] 0.1 – Tạo solution + projects (API, Application, Contract, Domain, Infrastructure, Persistence, Presentation)
- [x] 0.2 – Cấu hình project references đúng dependency rule
- [x] 0.3 – Cài packages: MediatR, FluentValidation, Carter, EF Core, StackExchange.Redis, SignalR
- [x] 0.4 – Cấu hình appsettings.json (ConnectionStrings, Redis, JWT)
- [x] 0.5 – BaseEntity, Result<T>, Error, ICommand, IQuery, ICommandHandler, IQueryHandler
- [x] 0.6 – ApiResponse wrapper chuẩn theo `04_API_HANDLER_PLAN.md` (hiện chưa đúng format)

---

## 🔷 GIAI ĐOẠN 0.5 – SỬA TYPOS + REFACTOR CƠ BẢN

- [x] 0.5.1 – Sửa typo: `Movie_StructrueCode.API` → `Movie_StructureCode.API`
- [x] 0.5.2 – Sửa typo: `Infracstructure` → `Infrastructure` (folder + csproj + namespaces)
- [x] 0.5.3 – Sửa typo: `Respositories` → `Repositories` (folder + namespaces)
- [x] 0.5.4 – Đổi tên entity `Ticket` → `BookingDetail` (entity, IRepo, Repo, Config, SeedData)
- [x] 0.5.5 – Xóa AutoMapper (`ServiceProfile.cs`) → chuyển sang Manual Mapper
- [x] 0.5.6 – Chuẩn hóa `ApiResponse<T>` wrapper theo docs

---

## 🔷 GIAI ĐOẠN 1 – DOMAIN ENTITIES & IREPOSITORY (9/9)

- [x] 1.1 – Entity: `Category` + `ICategoryRepository`
- [x] 1.2 – Entity: `Movie` + `IMovieRepository`
- [x] 1.3 – Entity: `Theater` + `ITheaterRepository`
- [x] 1.4 – Entity: `Room` + `IRoomRepository`
- [x] 1.5 – Entity: `Seat` + `SeatType` + `ISeatRepository` + `ISeatTypeRepository`
- [x] 1.6 – Entity: `Showing` + `IShowingRepository`
- [x] 1.7 – Entity: `Booking` + `BookingDetail` (cần đổi từ Ticket) + `IBookingRepository`
- [x] 1.8 – Interface `IUnitOfWork`
- [x] 1.9 – Domain Exceptions (BadRequest, Domain, NotFound, Product)

### Bonus (ngoài kế hoạch, đã hoàn thành):
- [x] 1.B1 – Entity: `AppUser` (ASP.NET Identity)
- [x] 1.B2 – Entity: `RefreshToken`
- [x] 1.B3 – Entity: `ShowingSeat` (bảng trung gian)

---

## 🔷 GIAI ĐOẠN 2 – PERSISTENCE: CONFIG + MIGRATION (10/10 ✅)

- [x] 2.1 – `ApplicationDbContext` với DbSet
- [x] 2.2 – `CategoryConfiguration`
- [x] 2.3 – `MovieConfiguration`
- [x] 2.4 – `TheaterConfiguration`
- [x] 2.5 – `RoomConfiguration`
- [x] 2.6 – `SeatConfiguration`
- [x] 2.7 – `ShowingConfiguration`
- [x] 2.8 – `BookingConfiguration` + `ShowingSeatConfiguration`
- [x] 2.9 – Migration chạy thành công (7 migrations)
- [x] 2.10 – Seed Data (7 seed files)

---

## 🔷 GIAI ĐOẠN 3 – PERSISTENCE: REPOSITORY IMPL (8/9)

- [x] 3.1 – `CategoryRepository`
- [x] 3.2 – `MovieRepository`
- [x] 3.3 – `TheaterRepository`
- [/] 3.4 – `RoomRepository` (cần thêm `GetByIdWithSeatsAsync`)
- [x] 3.5 – `SeatRepository`
- [/] 3.6 – `ShowingRepository` (cần thêm `GetBookedSeatIdsAsync`)
- [x] 3.7 – `BookingRepository`
- [x] 3.8 – `UnitOfWork`
- [x] 3.9 – `Persistence/DependencyInjection.cs`

---

## 🔷 GIAI ĐOẠN 4 – INFRASTRUCTURE: REDIS + SIGNALR (3/9)

- [/] 4.1 – Interface `ICacheService` + `IRedisService` trong Application/Abstractions
- [/] 4.2 – `CacheDataService` / `RedisCacheService` implement `ICacheService`
- [ ] 4.3 – LUA script: `HoldSeat.lua` (atomic check + hold)
- [ ] 4.4 – LUA script: `ReleaseSeat.lua`
- [ ] 4.5 – `RedisSeatService` implement `IRedisService` (dùng LUA scripts)
- [x] 4.6 – `SeatHub` (SignalR Hub)
- [ ] 4.7 – `SignalRSeatService` – broadcast seat status
- [ ] 4.8 – Background Job: `SessionCleanupJob`
- [x] 4.9 – `Infrastructure/DependencyInjection.cs`

---

## 🔷 GIAI ĐOẠN 5 – APPLICATION: ROOM USECASES – Admin (0/7)

- [ ] 5.1 – `GetAllRoomsQuery` + Handler
- [ ] 5.2 – `GetRoomByIdQuery` + Handler
- [ ] 5.3 – `GetRoomLayoutQuery` + Handler (có cache)
- [ ] 5.4 – `CreateRoomCommand` + Handler + Validator
- [ ] 5.5 – `UpdateRoomCommand` + Handler + Validator (invalidate cache)
- [ ] 5.6 – `RoomMapper` (ToResponse, ToDetailResponse, ToLayoutResponse) — **Manual Mapper**
- [ ] 5.7 – `RoomErrors` static class

---

## 🔷 GIAI ĐOẠN 6 – APPLICATION: SEAT USECASES – Admin (0/2)

- [ ] 6.1 – `CreateSeatsInBulkCommand` + Handler + Validator
- [ ] 6.2 – `UpdateSeatStatusCommand` + Handler

---

## 🔷 GIAI ĐOẠN 7 – APPLICATION: SHOWING USECASES – Admin (0/5)

- [ ] 7.1 – `GetAllShowingsQuery` + Handler
- [ ] 7.2 – `GetShowingDetailQuery` + Handler
- [ ] 7.3 – `CreateShowingCommand` + Handler + Validator (check room conflict)
- [ ] 7.4 – `UpdateShowingCommand` + Handler + Validator
- [ ] 7.5 – `ShowingMapper` — **Manual Mapper**

---

## 🔷 GIAI ĐOẠN 8 – APPLICATION: MOVIE + THEATER + CATEGORY USECASES (0/9)

- [ ] 8.1 – `GetAllMoviesAdminQuery` + Handler
- [ ] 8.2 – `GetMovieDetailAdminQuery` + Handler
- [ ] 8.3 – `CreateMovieCommand` + Handler + Validator
- [ ] 8.4 – `UpdateMovieCommand` + Handler + Validator
- [ ] 8.5 – `GetAllTheatersQuery` + Handler
- [ ] 8.6 – `MovieMapper` + `TheaterMapper` — **Manual Mapper**
- [ ] 8.7 – `GetAllCategoriesQuery` + Handler *(mới thêm)*
- [ ] 8.8 – `CreateCategoryCommand` + Handler + Validator *(mới thêm)*
- [ ] 8.9 – `CategoryMapper` — **Manual Mapper** *(mới thêm)*

---

## 🔷 GIAI ĐOẠN 9 – APPLICATION: USER USECASES (0/7)

- [ ] 9.1 – `GetAllMoviesUserQuery` + Handler (cache homepage)
- [ ] 9.2 – `GetMovieDetailsUserQuery` + Handler (kèm rạp + suất chiếu)
- [ ] 9.3 – `GetAllShowingsUserQuery` + Handler
- [ ] 9.4 – `GetLayoutRoomUserQuery` + Handler (merge DB + Redis)
- [ ] 9.5 – `CreateBookingCommand` + Handler (atomic Redis LUA + DB) + Validator
- [ ] 9.6 – `GetBookingHistoryQuery` + Handler
- [ ] 9.7 – `BookingMapper`, `MovieUserMapper` — **Manual Mapper**

---

## 🔷 GIAI ĐOẠN 10 – PRESENTATION: API MODULES – Admin (0/6)

> **Cấu trúc mới**: Phân tách thành `Admin/System/` và `Admin/Theater/`

### Admin – System (SystemAdmin role)
- [ ] 10.1 – `SystemMoviesModule` (POST /api/admin/movies, PUT /api/admin/movies/{id})
- [ ] 10.2 – `SystemTheatersModule` (GET /api/admin/theaters)
- [ ] 10.3 – `SystemCategoriesModule` (GET, POST /api/admin/categories) *(mới thêm)*

### Admin – Theater (TheaterOwner role)
- [ ] 10.4 – `TheaterRoomsModule` (5 endpoints: GET /, GET /{id}, GET /{id}/layout, POST /, PUT /{id})
- [ ] 10.5 – `TheaterSeatsModule` (2 endpoints: POST bulk, PUT status)
- [ ] 10.6 – `TheaterShowingsModule` (4 endpoints: GET /, GET /{id}, POST /, PUT /{id})

---

## 🔷 GIAI ĐOẠN 11 – PRESENTATION: API MODULES – User (0/4)

- [ ] 11.1 – `UserMoviesModule` (GET /api/movies, GET /api/movies/{id})
- [ ] 11.2 – `UserShowingsModule` (GET /api/showings)
- [ ] 11.3 – `UserRoomsModule` (GET /api/rooms/layout/{showingId})
- [ ] 11.4 – `UserBookingsModule` (POST /api/bookings, GET /api/bookings/history)

---

## 🔷 GIAI ĐOẠN 12 – API PROJECT: BOOTSTRAP (4/6)

- [x] 12.1 – `Program.cs` – đăng ký tất cả DI
- [x] 12.2 – Middleware: Global Exception Handler
- [x] 12.3 – Authorization: cấu hình 3 policies (`SystemAdmin`, `TheaterOwner`, `User`)
- [x] 12.4 – Swagger/OpenAPI với bearer token + grouped by tags
- [x] 12.5 – SignalR Hub endpoint map
- [ ] 12.6 – CORS cấu hình cho Angular dev

---

## 🔷 GIAI ĐOẠN 13 – TEST END-TO-END (0/8)

- [ ] 13.1 – **Admin**: Login → CreateRoom → CreateSeatsInBulk → GetLayoutRoom
- [ ] 13.2 – **Admin**: CreateShowing → GetAllShowings → GetShowingDetail
- [ ] 13.3 – **User**: GetAllMovies (homepage) → GetMovieDetails
- [ ] 13.4 – **User**: GetAllShowings → GetLayoutRoom (held + booked)
- [ ] 13.5 – **User**: CreateBooking → SignalR broadcast → ghế bị lock
- [ ] 13.6 – **User**: GetBookingHistory
- [ ] 13.7 – **Redis**: Session expire → Cleanup → ghế tự release
- [ ] 13.8 – **Cache**: Layout room cache hit/miss + invalidate

---

## 🔷 BONUS – AUTH MODULE (ĐÃ HOÀN THÀNH ✅)

- [x] B.1 – Register Command + Handler + Validator
- [x] B.2 – Login Query + Handler
- [x] B.3 – Logout Command + Handler
- [x] B.4 – Refresh Access Token Command + Handler
- [x] B.5 – Confirm Email Command + Handler
- [x] B.6 – Resend Confirm Email Command + Handler
- [x] B.7 – Send OTP Command + Handler
- [x] B.8 – Verify OTP Command + Handler
- [x] B.9 – JWT Token Provider Service
- [x] B.10 – Refresh Token Service
- [x] B.11 – Token Validation Middleware
- [x] B.12 – Token Cache Service (Redis)
- [x] B.13 – AuthApi Presentation Module

---

## 🔷 GIAI ĐOẠN 14 – FRONTEND (Angular 18) (0/26)

### Setup
- [ ] 14.1 – Khởi tạo Angular 18 project
- [ ] 14.2 – Cấu hình routing (lazy loading, `/admin` split)
- [ ] 14.3 – Implement design tokens từ `netflix.md` (CSS variables)

### Core
- [ ] 14.4 – Auth module (login, guards, interceptors)
- [ ] 14.5 – API Services (Movies, Showings, Rooms, Bookings)
- [ ] 14.6 – Route guards (admin, user auth)
- [ ] 14.7 – HTTP interceptors (token attach, error handling)

### Shared Components (theo `netflix.md`)
- [ ] 14.8 – Button components (primary-red `#e50914`, secondary-glass, ghost)
- [ ] 14.9 – Poster card (2:3 ratio, 0.75s hover preview `scale(1.4)`)
- [ ] 14.10 – Row carousel (6 posters/row desktop, chevron arrows)
- [ ] 14.11 – Detail modal (`#181818`, 6px radius, hero 16:9)
- [ ] 14.12 – Badges (TOP 10 red, NEW EPISODE green, match %)
- [ ] 14.13 – Player controls (scrubber `#e50914`)

### User Features
- [ ] 14.14 – Top nav (transparent → solid `#141414` sau 50px scroll)
- [ ] 14.15 – Billboard hero (16:9 full-bleed, SVG title, scrim gradient)
- [ ] 14.16 – Browse page (carousel rows, section cadence)
- [ ] 14.17 – Movie detail page (modal + suất chiếu + rạp)
- [ ] 14.18 – Seat selection page (SignalR realtime + Redis status)
- [ ] 14.19 – Booking flow (hold → pay → confirm)
- [ ] 14.20 – Booking history

### Admin Features
- [ ] 14.21 – Admin layout (sidebar + header)
- [ ] 14.22 – Dashboard (thống kê)
- [ ] 14.23 – Movies management (CRUD)
- [ ] 14.24 – Rooms management (CRUD + seat layout editor)
- [ ] 14.25 – Showings management
- [ ] 14.26 – Categories management

---

## 📊 TỔNG KẾT

| Giai đoạn | Tổng | Xong | Đang | Chưa | % |
|-----------|------|------|------|------|------|
| 0 – Setup | 6 | 5 | 0 | 1 | 83% |
| 0.5 – Typos/Refactor | 6 | 3 | 0 | 3 | 50% |
| 1 – Domain | 9 | 8 | 1 | 0 | 89% |
| 2 – Config | 10 | 10 | 0 | 0 | **100%** |
| 3 – Repository | 9 | 7 | 2 | 0 | 78% |
| 4 – Infrastructure | 9 | 2 | 2 | 5 | **22%** |
| 5 – Room UC | 7 | 0 | 0 | 7 | **0%** |
| 6 – Seat UC | 2 | 0 | 0 | 2 | **0%** |
| 7 – Showing UC | 5 | 0 | 0 | 5 | **0%** |
| 8 – Movie UC | 9 | 0 | 0 | 9 | **0%** |
| 9 – User UC | 7 | 0 | 0 | 7 | **0%** |
| 10 – Admin APIs | 6 | 0 | 0 | 6 | **0%** |
| 11 – User APIs | 4 | 0 | 0 | 4 | **0%** |
| 12 – Bootstrap | 6 | 4 | 0 | 2 | 67% |
| 13 – E2E Test | 8 | 0 | 0 | 8 | **0%** |
| Bonus Auth | 13 | 13 | 0 | 0 | **100%** |
| 14 – Frontend | 26 | 0 | 0 | 26 | **0%** |
| **TỔNG** | **142** | **52** | **5** | **85** | **~37%** |

---

## 🚦 THỨ TỰ THỰC HIỆN TIẾP

```
[ĐÃ XONG] GĐ 0-3 ✅ + Auth ✅ + Sửa typos ✅
     ↓
[LÀM NGAY] GĐ 0.5 (Ticket→BookingDetail, AutoMapper→Manual, ApiResponse)
     ↓
[TIẾP THEO] GĐ 4 (Redis LUA + SignalR Service + Background Job)
     ↓
[SAU ĐÓ] GĐ 5-8 (Admin UseCases: Room → Seat → Showing → Movie/Category/Theater)
     ↓
[TIẾP] GĐ 9 (User UseCases: Browse + Booking) ← cần GĐ 4 xong
     ↓
[TIẾP] GĐ 10-11 (API Modules: Admin System/Theater + User) ← cần GĐ 5-9 xong
     ↓
[TIẾP] GĐ 12 (Bootstrap: Auth policies + CORS)
     ↓
[TIẾP] GĐ 13 (E2E Test)
     ↓
[CUỐI CÙNG] GĐ 14 (Frontend Angular 18 + Netflix Design)
```

---

## 📝 GHI CHÚ QUAN TRỌNG

### Quy tắc Mapper (đã quyết định)
> Tất cả mapper dùng **Manual Mapper** (`internal static class`), **KHÔNG dùng AutoMapper**.
> Cần xóa `Application/Mappers/ServiceProfile.cs` và package AutoMapper.

### Quy tắc API Module (đã quyết định)
> Phân tách theo role:
> ```
> Presentation/APIs/
> ├── Admin/
> │   ├── System/       ← SystemAdmin role
> │   │   ├── SystemMoviesModule.cs
> │   │   ├── SystemTheatersModule.cs
> │   │   └── SystemCategoriesModule.cs
> │   └── Theater/      ← TheaterOwner role
> │       ├── TheaterRoomsModule.cs
> │       ├── TheaterSeatsModule.cs
> │       └── TheaterShowingsModule.cs
> ├── User/              ← User / Anonymous role
> │   ├── UserMoviesModule.cs
> │   ├── UserShowingsModule.cs
> │   ├── UserRoomsModule.cs
> │   └── UserBookingsModule.cs
> └── Auth/              ← Đã có
>     └── AuthApi.cs
> ```
