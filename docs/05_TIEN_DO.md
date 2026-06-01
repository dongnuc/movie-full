# ✅ FILE TIẾN ĐỘ – CINEMA PROJECT

> **Cách dùng**: Đánh dấu `[x]` khi hoàn thành mỗi đầu việc.  
> **Quy tắc vàng**: Chỉ chuyển sang bước tiếp theo khi bước hiện tại đã `[x]` đầy đủ.  
> Cập nhật `📅 Ngày hoàn thành` sau mỗi task.

---

## 🔷 GIAI ĐOẠN 0 – SETUP DỰ ÁN

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 0.1 | Tạo solution + các project (API, Application, Contract, Domain, Infrastructure, Persistence, Presentation) | `[ ]` | |
| 0.2 | Cấu hình project references đúng dependency rule | `[ ]` | |
| 0.3 | Cài packages: MediatR, FluentValidation, Carter, EFCore, StackExchange.Redis, SignalR | `[ ]` | |
| 0.4 | Cấu hình appsettings.json (ConnectionStrings, Redis, JWT) | `[ ]` | |
| 0.5 | Viết BaseEntity, Result<T>, Error, ICommand, IQuery, ICommandHandler, IQueryHandler | `[ ]` | |
| 0.6 | Viết ApiResponse wrapper, PagedResult | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 1 – DOMAIN ENTITIES & IREPOSITORY

> **Nguyên tắc**: Viết Entity trước → IRepository sau → KHÔNG implement ở đây.

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 1.1 | Entity: `Category` + `ICategoryRepository` | `[ ]` | |
| 1.2 | Entity: `Movie` + `IMovieRepository` | `[ ]` | |
| 1.3 | Entity: `Theater` + `ITheaterRepository` | `[ ]` | |
| 1.4 | Entity: `Room` + `IRoomRepository` | `[ ]` | |
| 1.5 | Entity: `Seat` (enum SeatType, SeatStatus) + `ISeatRepository` | `[ ]` | |
| 1.6 | Entity: `Showing` (enum ShowingStatus) + `IShowingRepository` | `[ ]` | |
| 1.7 | Entity: `Booking` + `BookingDetail` (enum BookingStatus) + `IBookingRepository` | `[ ]` | |
| 1.8 | Interface `IUnitOfWork` | `[ ]` | |
| 1.9 | Domain Exceptions (NotFoundException base, các entity exception) | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 2 – PERSISTENCE: CONFIGURATION + MIGRATION

> **Nguyên tắc**: Config Fluent API cho từng entity → DbContext → Migration.

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 2.1 | `ApplicationDbContext` với DbSet cho tất cả entity | `[ ]` | |
| 2.2 | `CategoryConfiguration` (Fluent API) | `[ ]` | |
| 2.3 | `MovieConfiguration` (Fluent API) | `[ ]` | |
| 2.4 | `TheaterConfiguration` (Fluent API) | `[ ]` | |
| 2.5 | `RoomConfiguration` (Fluent API) | `[ ]` | |
| 2.6 | `SeatConfiguration` (Fluent API) | `[ ]` | |
| 2.7 | `ShowingConfiguration` (Fluent API) | `[ ]` | |
| 2.8 | `BookingConfiguration` + `BookingDetailConfiguration` (Fluent API) | `[ ]` | |
| 2.9 | Chạy migration `InitialCreate` thành công | `[ ]` | |
| 2.10 | `DatabaseSeeder` – seed dữ liệu mẫu (Category, Theater, Room, Movie) | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 3 – PERSISTENCE: REPOSITORY IMPLEMENTATION

> **Nguyên tắc**: Implement đúng interface khai báo ở Domain. Internal sealed class.

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 3.1 | `CategoryRepository` implement `ICategoryRepository` | `[ ]` | |
| 3.2 | `MovieRepository` implement `IMovieRepository` | `[ ]` | |
| 3.3 | `TheaterRepository` implement `ITheaterRepository` | `[ ]` | |
| 3.4 | `RoomRepository` implement `IRoomRepository` (incl. `GetByIdWithSeatsAsync`) | `[ ]` | |
| 3.5 | `SeatRepository` implement `ISeatRepository` | `[ ]` | |
| 3.6 | `ShowingRepository` implement `IShowingRepository` (incl. `GetBookedSeatIdsAsync`) | `[ ]` | |
| 3.7 | `BookingRepository` implement `IBookingRepository` | `[ ]` | |
| 3.8 | `UnitOfWork` implement `IUnitOfWork` | `[ ]` | |
| 3.9 | `Persistence/DependencyInjection.cs` – đăng ký tất cả repository | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 4 – INFRASTRUCTURE: REDIS + SIGNALR

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 4.1 | Interface `ICacheService` + `IRedisService` trong Application/Abstractions | `[ ]` | |
| 4.2 | `RedisCacheService` implement `ICacheService` | `[ ]` | |
| 4.3 | LUA script: `HoldSeat.lua` (atomic check + hold) | `[ ]` | |
| 4.4 | LUA script: `ReleaseSeat.lua` | `[ ]` | |
| 4.5 | `RedisSeatService` implement `IRedisService` (dùng LUA scripts) | `[ ]` | |
| 4.6 | `SeatHub` (SignalR Hub) | `[ ]` | |
| 4.7 | `SignalRSeatService` – broadcast seat status | `[ ]` | |
| 4.8 | Background Job: `SessionCleanupJob` (Quartz.NET / Hangfire) | `[ ]` | |
| 4.9 | `Infrastructure/DependencyInjection.cs` | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 5 – APPLICATION: ROOM USECASES (Admin)

> **Thứ tự trong mỗi usecase**: Query/Command → Handler → Validator → Mapper

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 5.1 | `GetAllRoomsQuery` + Handler | `[ ]` | |
| 5.2 | `GetRoomByIdQuery` + Handler | `[ ]` | |
| 5.3 | `GetRoomLayoutQuery` + Handler (có cache) | `[ ]` | |
| 5.4 | `CreateRoomCommand` + Handler + Validator | `[ ]` | |
| 5.5 | `UpdateRoomCommand` + Handler + Validator (invalidate cache) | `[ ]` | |
| 5.6 | `RoomMapper` (ToResponse, ToDetailResponse, ToLayoutResponse) | `[ ]` | |
| 5.7 | `RoomErrors` static class | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 6 – APPLICATION: SEAT USECASES (Admin)

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 6.1 | `CreateSeatsInBulkCommand` + Handler + Validator | `[ ]` | |
| 6.2 | `UpdateSeatStatusCommand` + Handler | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 7 – APPLICATION: SHOWING USECASES (Admin)

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 7.1 | `GetAllShowingsQuery` + Handler | `[ ]` | |
| 7.2 | `GetShowingDetailQuery` + Handler | `[ ]` | |
| 7.3 | `CreateShowingCommand` + Handler + Validator (check room conflict) | `[ ]` | |
| 7.4 | `UpdateShowingCommand` + Handler + Validator | `[ ]` | |
| 7.5 | `ShowingMapper` | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 8 – APPLICATION: MOVIE USECASES (Admin + System)

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 8.1 | `GetAllMoviesAdminQuery` + Handler | `[ ]` | |
| 8.2 | `GetMovieDetailAdminQuery` + Handler | `[ ]` | |
| 8.3 | `CreateMovieCommand` + Handler + Validator | `[ ]` | |
| 8.4 | `UpdateMovieCommand` + Handler + Validator | `[ ]` | |
| 8.5 | `GetAllTheatersQuery` + Handler | `[ ]` | |
| 8.6 | `MovieMapper` + `TheaterMapper` | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 9 – APPLICATION: USER USECASES

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 9.1 | `GetAllMoviesUserQuery` + Handler (cache homepage) | `[ ]` | |
| 9.2 | `GetMovieDetailsUserQuery` + Handler (kèm rạp + suất chiếu) | `[ ]` | |
| 9.3 | `GetAllShowingsUserQuery` + Handler | `[ ]` | |
| 9.4 | `GetLayoutRoomUserQuery` + Handler (merge DB + Redis) | `[ ]` | |
| 9.5 | `CreateBookingCommand` + Handler (atomic Redis LUA + DB) + Validator | `[ ]` | |
| 9.6 | `GetBookingHistoryQuery` + Handler | `[ ]` | |
| 9.7 | `BookingMapper`, `MovieUserMapper` | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 10 – PRESENTATION: API MODULES (Admin)

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 10.1 | `AdminRoomsModule` (5 endpoints) | `[ ]` | |
| 10.2 | `AdminSeatsModule` (2 endpoints) | `[ ]` | |
| 10.3 | `AdminShowingsModule` (4 endpoints) | `[ ]` | |
| 10.4 | `AdminMoviesModule` (4 endpoints) | `[ ]` | |
| 10.5 | `AdminTheatersModule` (1 endpoint) | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 11 – PRESENTATION: API MODULES (User)

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 11.1 | `UserMoviesModule` (2 endpoints) | `[ ]` | |
| 11.2 | `UserShowingsModule` (1 endpoint) | `[ ]` | |
| 11.3 | `UserRoomsModule` (1 endpoint) | `[ ]` | |
| 11.4 | `UserBookingsModule` (2 endpoints) | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 12 – API PROJECT: BOOTSTRAP

| # | Đầu việc | Trạng thái | Ngày hoàn thành |
|---|----------|-----------|-----------------|
| 12.1 | `Program.cs` – đăng ký tất cả DI (Application, Persistence, Infrastructure, Carter) | `[ ]` | |
| 12.2 | Middleware: Global Exception Handler | `[ ]` | |
| 12.3 | Authorization: cấu hình 3 policies (SystemAdmin, TheaterOwner, User) | `[ ]` | |
| 12.4 | Swagger/OpenAPI với bearer token + grouped by tags | `[ ]` | |
| 12.5 | SignalR Hub endpoint map (`/hubs/seat`) | `[ ]` | |
| 12.6 | CORS cấu hình cho Angular dev | `[ ]` | |

---

## 🔷 GIAI ĐOẠN 13 – TEST END-TO-END

| # | Luồng test | Trạng thái | Ngày hoàn thành |
|---|-----------|-----------|-----------------|
| 13.1 | **Admin**: Login → CreateRoom → CreateSeatsInBulk → GetLayoutRoom | `[ ]` | |
| 13.2 | **Admin**: CreateShowing → GetAllShowings → GetShowingDetail | `[ ]` | |
| 13.3 | **User**: GetAllMovies (homepage) → GetMovieDetails (với rạp + suất) | `[ ]` | |
| 13.4 | **User**: GetAllShowings → GetLayoutRoom (có held + booked) | `[ ]` | |
| 13.5 | **User**: CreateBooking → SignalR broadcast → kiểm tra ghế bị lock | `[ ]` | |
| 13.6 | **User**: GetBookingHistory | `[ ]` | |
| 13.7 | **Redis**: Test session expire → Background Job cleanup → ghế tự release | `[ ]` | |
| 13.8 | **Cache**: Test layout room cache hit/miss + invalidate khi update | `[ ]` | |

---

## 📊 TỔNG KẾT TIẾN ĐỘ

| Giai đoạn | Tổng tasks | Hoàn thành | % |
|-----------|-----------|-----------|---|
| 0 – Setup | 6 | 0 | 0% |
| 1 – Domain | 9 | 0 | 0% |
| 2 – Config + Migration | 10 | 0 | 0% |
| 3 – Repository Impl | 9 | 0 | 0% |
| 4 – Infrastructure | 9 | 0 | 0% |
| 5 – Room UseCases | 7 | 0 | 0% |
| 6 – Seat UseCases | 2 | 0 | 0% |
| 7 – Showing UseCases | 5 | 0 | 0% |
| 8 – Movie UseCases | 6 | 0 | 0% |
| 9 – User UseCases | 7 | 0 | 0% |
| 10 – Admin APIs | 5 | 0 | 0% |
| 11 – User APIs | 4 | 0 | 0% |
| 12 – Bootstrap | 6 | 0 | 0% |
| 13 – E2E Test | 8 | 0 | 0% |
| **Tổng** | **93** | **0** | **0%** |

---

## 📝 GHI CHÚ / ISSUES

| Ngày | Vấn đề | Giải pháp | Trạng thái |
|------|--------|-----------|-----------|
| | | | |

---

## 🚦 LUỒNG PHỤ THUỘC (QUAN TRỌNG)

```
Giai đoạn 0 (Setup)
    ↓
Giai đoạn 1 (Domain)
    ↓
Giai đoạn 2 (Config + Migration)
    ↓
Giai đoạn 3 (Repository)    ←── song song với ──→    Giai đoạn 4 (Infrastructure)
    ↓                                                           ↓
Giai đoạn 5, 6, 7, 8 (Admin UseCases)  ←── cần 4 xong ──→ Giai đoạn 9 (User UseCases)
    ↓
Giai đoạn 10 (Admin APIs)
    ↓
Giai đoạn 11 (User APIs)
    ↓
Giai đoạn 12 (Bootstrap)
    ↓
Giai đoạn 13 (E2E Test)
```
