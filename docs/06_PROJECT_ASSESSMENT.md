# 📊 TỔNG HỢP DỰ ÁN & ĐÁNH GIÁ TIẾN ĐỘ – CINEMA PROJECT

> **Mục đích**: File này là **nguồn tham chiếu duy nhất** khi bắt đầu làm việc. Không cần đọc lại `00` → `05`.  
> **Cập nhật lần cuối**: 2026-05-29  
> **File theo dõi chi tiết tasks**: [`07_TASK_CHECKLIST.md`](./07_TASK_CHECKLIST.md)

---

## 1. TỔNG QUAN DỰ ÁN

| Hạng mục | Chi tiết |
|----------|---------|
| **Loại** | Dự án cá nhân – Rạp chiếu phim online |
| **Backend** | C# .NET 8, Clean Architecture, Minimal API (Carter) |
| **Frontend** | Angular 18 (Netflix-style design, chưa bắt đầu) |
| **Database** | SQL Server (Code First – EF Core) |
| **Cache** | Redis (data cache + LUA scripts cho holding seat) |
| **Realtime** | SignalR (broadcast seat status tới client) |
| **Kiến trúc** | Clean Architecture, CQRS (Command/Query), Mediator pattern |

---

## 2. KIẾN TRÚC & QUY TẮC (tóm tắt từ `01_ARCHITECTURE_RULES.md`)

### 2.1 Layer Diagram

```
┌──────────────────────────────────────────────────────────┐
│                    API (Host Layer)                        │
│         Program.cs, DI, Middleware, Hubs, CORS            │
├──────────────────────────────────────────────────────────┤
│                  Presentation Layer                        │
│    Carter Modules, Request/Response DTOs, ApiResponse     │
├──────────────────────────────────────────────────────────┤
│                  Application Layer                         │
│  UseCases (Commands/Queries), Handlers, Validators,       │
│  Manual Mappers, Behaviors, Abstractions (interfaces)     │
├──────────────────────────────────────────────────────────┤
│          Infrastructure + Persistence Layer               │
│  Redis, SignalR, JWT, Email (Infra)                       │
│  EF Core, DbContext, Repos, Configs, Migrations (Persist) │
├──────────────────────────────────────────────────────────┤
│                    Domain Layer                            │
│      Entities, IRepository interfaces, Exceptions         │
├──────────────────────────────────────────────────────────┤
│                   Contract Layer                           │
│  ICommand, IQuery, Result<T>, Error, Settings, Events     │
└──────────────────────────────────────────────────────────┘
```

### 2.2 Dependency Rule (Bắt buộc)

```
Domain ← Application ← Infrastructure/Persistence ← Presentation ← API
```

- **Domain**: KHÔNG phụ thuộc layer nào khác
- **Application**: chỉ phụ thuộc Domain + Contract
- **Persistence**: phụ thuộc Domain (để map entity)
- **Infrastructure**: implement abstractions từ Application/Contract
- **Presentation**: gọi Application (ISender.Send)
- **API**: composition root, tham chiếu tất cả

### 2.3 Quy tắc Code Cứng

| Quy tắc | Mô tả |
|---------|-------|
| **Mapper** | ✅ **Manual Mapper** (internal static class) — KHÔNG dùng AutoMapper |
| **DI** | Mỗi layer có 1 file `DependencyInjection.cs` với extension method `AddXyz()` |
| **Repository** | Interface ở Domain, Implementation ở Persistence. KHÔNG inject DbContext trực tiếp |
| **UseCase** | Mỗi Command/Query có: Command/Query.cs + Handler.cs + Validator.cs |
| **Handler (Presentation)** | Chỉ gọi `ISender.Send()`, KHÔNG chứa business logic |
| **Entity** | Kế thừa `BaseEntity`, dùng factory method `Create()`, setter là `private set` |
| **Error handling** | Dùng `Result<T>` pattern, KHÔNG throw exception trong handler |
| **Mỗi file** | Chỉ chứa 1 class/interface |

### 2.4 Cấu trúc Projects (đã sửa typos)

```
StructureCode_Clean_Architecture/
├── Movie_StructureCode.API/              → Host, DI, Middleware, Hubs
├── Movie_StructureCode.Presentation/     → Carter Modules, Request/Response DTOs
├── Movie_StructureCode.Application/      → UseCases, Behaviors, Mappers, Abstractions
├── Movie_StructureCode.Domain/           → Entities, Repositories (interfaces), Exceptions
├── Movie_StructureCode.Contract/         → ICommand, IQuery, Result<T>, Error, Settings
├── Movie_StructureCode.Persistence/      → EF Core, Configurations, Migrations, Repos (impl)
├── Movie_StructureCode.Infrastructure/   → Redis, SignalR, JWT, Email
└── Movie_StructureCode.sln
```

---

## 3. ENTITY MODEL (tóm tắt từ `02_REPOSITORY_PLAN.md`)

### 3.1 BaseEntity

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
```

### 3.2 Tất cả Entities

| Entity | FK / Quan hệ | Trạng thái |
|--------|-------------|-----------|
| **Category** | has many Movie | ✅ Đã có |
| **Movie** | FK → Category, has many Showing | ✅ Đã có |
| **Theater** | FK → User (OwnerId), has many Room | ✅ Đã có |
| **Room** | FK → Theater, has many Seat, has many Showing | ✅ Đã có |
| **Seat** | FK → Room. Có SeatType entity riêng | ✅ Đã có |
| **SeatType** | Entity riêng (Regular, VIP, Couple) | ✅ Đã có (bonus) |
| **Showing** | FK → Movie, FK → Room, has many ShowingSeat | ✅ Đã có |
| **ShowingSeat** | FK → Showing, FK → Seat (bảng trung gian) | ✅ Đã có (bonus) |
| **Booking** | FK → User, has many BookingDetail | ✅ Đã có |
| **BookingDetail** | FK → Booking, FK → Showing, FK → Seat | ⚠️ Cần đổi tên từ `Ticket` |
| **AppUser** | ASP.NET Identity | ✅ Đã có (bonus) |
| **RefreshToken** | FK → User | ✅ Đã có (bonus) |

### 3.3 IRepository Interfaces (tất cả đã có trong Domain/Repositories/)

| Interface | Methods cần có | Trạng thái |
|-----------|----------------|-----------|
| `ICategoryRepository` | GetById, GetAll, Add, Update | ✅ |
| `IMovieRepository` | GetById, GetByIdWithShowings, GetAll (filter), Add, Update | ✅ |
| `ITheaterRepository` | GetById, GetAll (search, isActive, page), Add, Update | ✅ |
| `IRoomRepository` | GetById, **GetByIdWithSeats**, GetAll, Add, Update | ⚠️ Cần thêm GetByIdWithSeats |
| `ISeatRepository` | GetById, GetByRoomId, GetByIds, AddRange, Update | ✅ |
| `IShowingRepository` | GetById, GetByIdWithDetails, GetAllAdmin, GetByMovieAndTheater, **GetBookedSeatIds**, Add, Update | ⚠️ Cần thêm GetBookedSeatIds |
| `IBookingRepository` | GetById, GetByIdWithDetails, GetByUserId, IsSeatBooked, Add, Update | ✅ |
| `IUnitOfWork` | SaveChangesAsync | ✅ |

---

## 4. USECASE PLAN (tóm tắt từ `03_USECASE_PLAN.md`)

### 4.1 Admin — Room UseCases

| UseCase | Type | Response | Trạng thái |
|---------|------|----------|-----------|
| GetAllRoomsAdmin | Query | PagedResult\<RoomResponse\> | ❌ |
| GetRoomByIdAdmin | Query | RoomDetailResponse | ❌ |
| GetRoomLayout | Query | RoomLayoutResponse (có cache) | ❌ |
| CreateRoom | Command | Guid (roomId) | ❌ |
| UpdateRoom | Command | Result (invalidate cache) | ❌ |

### 4.2 Admin — Seat UseCases

| UseCase | Type | Response | Trạng thái |
|---------|------|----------|-----------|
| CreateSeatsInBulk | Command | int (số ghế tạo) | ❌ |
| UpdateSeatStatus | Command | Result | ❌ |

### 4.3 Admin — Showing UseCases

| UseCase | Type | Response | Trạng thái |
|---------|------|----------|-----------|
| GetAllShowingsAdmin | Query | PagedResult\<ShowingResponse\> | ❌ |
| GetShowingDetail | Query | ShowingDetailResponse (vé đã bán, ghế booked) | ❌ |
| CreateShowing | Command | Guid (check room conflict, EndTime = Start + Duration) | ❌ |
| UpdateShowing | Command | Result | ❌ |

### 4.4 Admin — Movie UseCases

| UseCase | Type | Response | Trạng thái |
|---------|------|----------|-----------|
| GetAllMoviesAdmin | Query | PagedResult\<MovieResponse\> (filter search, category, isActive) | ❌ |
| GetMovieDetailAdmin | Query | MovieDetailResponse | ❌ |
| CreateMovie | Command | Guid | ❌ |
| UpdateMovie | Command | Result | ❌ |

### 4.5 Admin — Theater + Category UseCases

| UseCase | Type | Response | Trạng thái |
|---------|------|----------|-----------|
| GetAllTheaters | Query | PagedResult\<TheaterResponse\> | ❌ |
| GetAllCategories | Query | List\<CategoryResponse\> | ❌ |
| CreateCategory | Command | Guid | ❌ |

### 4.6 User UseCases

| UseCase | Type | Mô tả | Trạng thái |
|---------|------|-------|-----------|
| GetAllMoviesUser | Query | Homepage, chỉ IsActive=true, cache | ❌ |
| GetMovieDetailsUser | Query | Chi tiết + rạp + số suất chiếu | ❌ |
| GetAllShowingsUser | Query | Theo movieId + theaterId, kèm availableSeats | ❌ |
| GetLayoutRoomUser | Query | Layout theo showingId, merge DB (booked) + Redis (held) → DisplayStatus | ❌ |
| CreateBooking | Command | **Phức tạp nhất**: validate → Redis LUA hold → DB Booking → SignalR broadcast | ❌ |
| GetBookingHistory | Query | Lịch sử đặt vé của user | ❌ |

### 4.7 Auth UseCases (BONUS — đã hoàn thành 100%)

| UseCase | Type | Trạng thái |
|---------|------|-----------|
| Register | Command | ✅ |
| Login | Query | ✅ |
| Logout | Command | ✅ |
| RefreshAccessToken | Command | ✅ |
| ConfirmEmail | Command | ✅ |
| ResendConfirmEmail | Command | ✅ |
| SendOtp | Command | ✅ |
| VerifyOtp | Command | ✅ |

---

## 5. API ENDPOINTS (tóm tắt từ `04_API_HANDLER_PLAN.md`)

### 5.1 Cấu trúc API Module (đã quyết định)

```
Presentation/APIs/
├── Admin/
│   ├── System/              ← SystemAdmin role
│   │   ├── SystemMoviesModule.cs       (POST, PUT)
│   │   ├── SystemTheatersModule.cs     (GET)
│   │   └── SystemCategoriesModule.cs   (GET, POST)
│   └── Theater/             ← TheaterOwner role
│       ├── TheaterRoomsModule.cs       (GET/, GET/{id}, GET/{id}/layout, POST, PUT/{id})
│       ├── TheaterSeatsModule.cs       (POST bulk, PUT status)
│       └── TheaterShowingsModule.cs    (GET/, GET/{id}, POST, PUT/{id})
├── User/                    ← User / Anonymous
│   ├── UserMoviesModule.cs             (GET/, GET/{id})
│   ├── UserShowingsModule.cs           (GET)
│   ├── UserRoomsModule.cs              (GET /layout/{showingId})
│   └── UserBookingsModule.cs           (POST, GET /history)
└── Auth/                    ← Đã hoàn thành
    └── AuthApi.cs
```

### 5.2 Bảng Tổng Hợp 22+ Endpoints

| # | Method | Route | Auth | UseCase | Trạng thái |
|---|--------|-------|------|---------|-----------|
| 1 | GET | `/api/admin/rooms` | TheaterOwner | GetAllRoomsQuery | ❌ |
| 2 | GET | `/api/admin/rooms/{id}` | TheaterOwner | GetRoomByIdQuery | ❌ |
| 3 | GET | `/api/admin/rooms/{id}/layout` | TheaterOwner | GetRoomLayoutQuery | ❌ |
| 4 | POST | `/api/admin/rooms` | TheaterOwner | CreateRoomCommand | ❌ |
| 5 | PUT | `/api/admin/rooms/{id}` | TheaterOwner | UpdateRoomCommand | ❌ |
| 6 | POST | `/api/admin/rooms/{roomId}/seats/bulk` | TheaterOwner | CreateSeatsInBulkCommand | ❌ |
| 7 | PUT | `/api/admin/seats/{id}/status` | TheaterOwner | UpdateSeatStatusCommand | ❌ |
| 8 | GET | `/api/admin/showings` | TheaterOwner | GetAllShowingsQuery | ❌ |
| 9 | GET | `/api/admin/showings/{id}` | TheaterOwner | GetShowingDetailQuery | ❌ |
| 10 | POST | `/api/admin/showings` | TheaterOwner | CreateShowingCommand | ❌ |
| 11 | PUT | `/api/admin/showings/{id}` | TheaterOwner | UpdateShowingCommand | ❌ |
| 12 | GET | `/api/admin/movies` | TheaterOwner | GetAllMoviesAdminQuery | ❌ |
| 13 | GET | `/api/admin/movies/{id}` | TheaterOwner | GetMovieDetailAdminQuery | ❌ |
| 14 | POST | `/api/admin/movies` | SystemAdmin | CreateMovieCommand | ❌ |
| 15 | PUT | `/api/admin/movies/{id}` | SystemAdmin | UpdateMovieCommand | ❌ |
| 16 | GET | `/api/admin/theaters` | SystemAdmin | GetAllTheatersQuery | ❌ |
| 17 | GET | `/api/admin/categories` | SystemAdmin | GetAllCategoriesQuery | ❌ |
| 18 | POST | `/api/admin/categories` | SystemAdmin | CreateCategoryCommand | ❌ |
| 19 | GET | `/api/movies` | Anonymous | GetAllMoviesUserQuery | ❌ |
| 20 | GET | `/api/movies/{id}` | Anonymous | GetMovieDetailsUserQuery | ❌ |
| 21 | GET | `/api/showings` | Anonymous | GetAllShowingsUserQuery | ❌ |
| 22 | GET | `/api/rooms/layout/{showingId}` | User | GetLayoutRoomUserQuery | ❌ |
| 23 | POST | `/api/bookings` | User | CreateBookingCommand | ❌ |
| 24 | GET | `/api/bookings/history` | User | GetBookingHistoryQuery | ❌ |

### 5.3 Handler Pattern (tất cả module theo mẫu này)

```csharp
// Carter Module pattern
public class AdminRoomsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/rooms")
            .RequireAuthorization("TheaterOwner")
            .WithTags("Admin - Rooms");

        group.MapGet("/", GetAllRooms);
        group.MapPost("/", CreateRoom);
    }

    private static async Task<IResult> GetAllRooms(
        [AsParameters] GetAllRoomsRequest req, ISender sender, CancellationToken ct)
    {
        var query = new GetAllRoomsQuery(req.Search, req.IsActive, req.Page, req.PageSize);
        var result = await sender.Send(query, ct);
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(result.Error));
    }
}
```

### 5.4 ApiResponse Wrapper (chuẩn)

```csharp
public sealed record ApiResponse<T>(
    bool IsSuccess, T? Data, string? ErrorCode, string? ErrorMessage, DateTime Timestamp)
{
    public static ApiResponse<T> Success(T data) => new(true, data, null, null, DateTime.UtcNow);
    public static ApiResponse<T> Failure(Error error) => new(false, default, error.Code, error.Message, DateTime.UtcNow);
}
```

---

## 6. INFRASTRUCTURE PLAN (tóm tắt)

### 6.1 Redis — Seat Management

```
Infrastructure/
├── Redis/
│   ├── RedisCacheService.cs         → ICacheService (Get/Set/Remove cache)
│   ├── RedisSeatService.cs          → IRedisService (HoldSeat, ReleaseSeat, GetHeldSeats)
│   └── LuaScripts/
│       ├── HoldSeat.lua             → atomic check + hold (TTL)
│       └── ReleaseSeat.lua          → atomic release
├── SignalR/
│   ├── SeatHub.cs                   → Hub cho /hubs/seat
│   └── SignalRSeatService.cs        → broadcast seat status changed
└── BackgroundJobs/
    └── SessionCleanupJob.cs         → dọn session expired → release ghế
```

### 6.2 Application Abstractions (interfaces)

```csharp
// Application/Abstractions/IRedisService.cs
public interface IRedisService
{
    Task<bool> HoldSeatAsync(string showingId, string seatId, string sessionId, int ttlSeconds);
    Task<bool> ReleaseSeatAsync(string showingId, string seatId, string sessionId);
    Task<IEnumerable<string>> GetHeldSeatsAsync(string showingId);
    Task ExtendSessionAsync(string sessionId, int ttlSeconds);
}

// Application/Abstractions/ICacheService.cs
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
}
```

### 6.3 Booking Flow (luồng phức tạp nhất)

```
User chọn ghế → Frontend gửi POST /api/bookings
     ↓
[1] Validate: seat tồn tại, chưa booked trong DB
     ↓
[2] Redis LUA HoldSeat (atomic): check + hold với TTL
     → Nếu bất kỳ seat nào đã hold/booked → return failure (409 Conflict)
     ↓
[3] Tạo Booking + BookingDetails trong DB (Status = Pending)
     ↓
[4] SignalR broadcast: seat status changed → tất cả client thấy ghế bị hold
     ↓
[5] Return BookingId → Frontend chuyển sang thanh toán
     ↓
[6] Background Job: nếu session expire → cleanup → release ghế → SignalR broadcast
```

---

## 7. QUYẾT ĐỊNH ĐÃ XÁC NHẬN

| # | Vấn đề | Quyết định | Ngày |
|---|--------|-----------|------|
| 1 | Mapper | ✅ **Manual Mapper** — xóa AutoMapper | 2026-05-29 |
| 2 | Ticket entity | ✅ **Đổi thành BookingDetail** | 2026-05-29 |
| 3 | API structure | ✅ **Admin (System + Theater) / User** | 2026-05-29 |
| 4 | Typos | ✅ **Đã sửa 3 typos** (StructrueCode, Infracstructure, Respositories) | 2026-05-29 |
| 5 | Category CRUD | ✅ **Thêm vào kế hoạch** | 2026-05-29 |
| 6 | SeatType | ✅ **Giữ nguyên là entity** (linh hoạt hơn enum) | 2026-05-29 |
| 7 | ShowingSeat | ✅ **Giữ nguyên** bảng trung gian (bổ sung docs) | 2026-05-29 |

---

## 8. TRẠNG THÁI HIỆN TẠI — CÁI GÌ ĐÃ LÀM

### ✅ Đã hoàn thành

| Phần | Chi tiết |
|------|---------|
| **Solution + Projects** | 7 projects đúng cấu trúc Clean Architecture, dependency đúng |
| **Packages** | MediatR, FluentValidation, Carter, EF Core, StackExchange.Redis, SignalR |
| **Config** | appsettings.json (3 files: default, Development, Docker) |
| **Contract Layer** | ICommand, IQuery, ICommandHandler, IQueryHandler, Result<T>, Error, PagedResult |
| **Domain Entities** | 12 entities (7 core + 5 bonus: AppUser, RefreshToken, ShowingSeat, SeatType, Ticket→BookingDetail) |
| **Domain IRepository** | 12 interfaces + IUnitOfWork + IRepository (generic base) |
| **Domain Exceptions** | BadRequestException, DomainException, NotFoundException, ProductException |
| **Persistence Configs** | 12 Fluent API configurations |
| **Persistence Repos** | 13 repository implementations + UnitOfWork + generic Repository base |
| **Persistence DI** | Đăng ký đầy đủ |
| **Migrations** | 7 migrations thành công (InitDatabase → AddTheaterIdInShowing) |
| **Seed Data** | 7 files (Category, Movie, Room, Seat, SeatType, Showing, Theater) |
| **Auth Module** | **Hoàn chỉnh**: Register, Login, Logout, RefreshToken, ConfirmEmail, ResendConfirmEmail, SendOtp, VerifyOtp |
| **JWT Services** | TokenProvider, RefreshTokenService, TokenServiceComposite, TokenCacheService |
| **Middleware** | ExceptionHandlingMiddleware, TokenValidationMiddleware |
| **SeatHub** | SignalR Hub cơ bản |
| **Program.cs** | DI bootstrap, Swagger, SignalR mapping |
| **API Auth** | AuthApi.cs — đầy đủ endpoints (~10.5KB) |
| **API Skeletons** | 10 Carter modules có group route nhưng chưa có handler |
| **Authorization Policies** | Cấu hình 3 policies (SystemAdmin, TheaterOwner, User) trong Program.cs |

### ⚠️ Cần hoàn thiện / refactor

| Phần | Chi tiết |
|------|---------|
| **Ticket → BookingDetail** | Đổi tên entity, IRepo, Repo, Config, SeedData |
| **AutoMapper → Manual** | Xóa `ServiceProfile.cs`, xóa package AutoMapper |
| **ApiResponse wrapper** | Chuẩn hóa theo format docs |
| **RoomRepo** | Thêm `GetByIdWithSeatsAsync` (Include Seats) |
| **ShowingRepo** | Thêm `GetBookedSeatIdsAsync` |
| **Redis interfaces** | ICacheService + IRedisService hoàn chỉnh |
| **CacheDataService** | Review và chuẩn hóa |

### ❌ Chưa làm

| Phần | Số tasks |
|------|---------|
| **Admin UseCases** (Room, Seat, Showing, Movie, Category, Theater) | ~23 tasks |
| **User UseCases** (Browse, Booking) | 7 tasks |
| **API Endpoint Handlers** (Admin + User) | 10 modules |
| **Redis LUA Scripts** (HoldSeat, ReleaseSeat) | 2 scripts |
| **RedisSeatService** | 1 service |
| **SignalRSeatService** | 1 service |
| **SessionCleanupJob** | 1 background job |
| **CORS for Angular** | 1 config |
| **E2E Testing** | 8 test flows |
| **Frontend Angular 17** | 26 tasks |

---

## 9. LUỒNG CODE BẮT BUỘC (khi implement feature mới)

```
[1] Domain Entity + IRepository interface
         ↓
[2] Persistence: Fluent API Configuration + Migration
         ↓
[3] Persistence: Repository Implementation
         ↓
[4] Application: Command/Query + Handler + Validator + Manual Mapper
         ↓
[5] Presentation: Carter Module Handler + Endpoint
         ↓
[6] Test end-to-end
```

> **Quy tắc vàng**: KHÔNG viết UseCase trước khi có Repository. KHÔNG viết API trước khi có UseCase.

---

## 10. THỨ TỰ ƯU TIÊN THỰC HIỆN

```
[ĐÃ XONG] ✅ Nền tảng (GĐ 0-3) + Auth + Sửa typos
     ↓
[LÀM NGAY] GĐ 0.5 — Refactor cơ bản:
     • Ticket → BookingDetail
     • AutoMapper → Manual Mapper
     • ApiResponse chuẩn hóa
     ↓
[TIẾP THEO] GĐ 4 — Infrastructure:
     • Redis LUA scripts (HoldSeat, ReleaseSeat)
     • RedisSeatService
     • SignalRSeatService
     • SessionCleanupJob
     ↓
[SAU ĐÓ] GĐ 5-8 — Admin UseCases:
     Room → Seat → Showing → Movie/Category/Theater
     (mỗi nhóm: Query/Command → Handler → Validator → Mapper)
     ↓
[TIẾP] GĐ 9 — User UseCases:
     Browse movies → View showings → Room layout → Create booking → History
     (cần GĐ 4 xong cho Redis/SignalR integration)
     ↓
[TIẾP] GĐ 10-11 — API Modules:
     Refactor folder structure → Thêm handlers theo Admin System/Theater + User
     ↓
[TIẾP] GĐ 12 — Bootstrap:
     Authorization policies + CORS
     ↓
[TIẾP] GĐ 13 — E2E Testing
     ↓
[CUỐI CÙNG] GĐ 14 — Frontend Angular 17 + Netflix Design
```

---

## 11. FRONTEND PLAN (tóm tắt)

### 11.1 Tech Stack
- Angular 17, Netflix-style design (`netflix.md`)
- Canvas: `#141414`, Brand Red: `#e50914`, Netflix Sans font

### 11.2 FE Project Structure

```
src/app/
├── core/                     # Auth, guards, interceptors, API services
├── shared/                   # Buttons, cards, carousels, badges, modals, player
│   ├── components/
│   ├── directives/           # hover-delay (0.75s)
│   └── styles/               # CSS tokens (#141414, #e50914)
├── features/
│   ├── user/                 # Browse, search, watch, seat selection, booking
│   │   ├── layout/           # Top nav (transparent→solid), footer
│   │   ├── browse/           # Billboard hero + carousel rows
│   │   └── watch/            # Fullscreen player
│   └── admin/                # Dashboard, movies/rooms/showings CRUD
├── app.routes.ts             # Lazy loading /admin split
└── app.config.ts
```

### 11.3 Key Design Rules (từ `netflix.md`)
- Canvas `#141414` (KHÔNG dùng `#000000`)
- Red `#e50914` chỉ dùng cho: Primary CTA, logo, TOP 10 badge, progress bar
- Poster 2:3 portrait, Billboard 16:9
- Hover preview: 0.75s delay → scale(1.4) trong 750ms
- Top nav: transparent → solid sau scrollY > 50px
- Dark-mode only — KHÔNG có light theme

---

## 12. TỔNG KẾT TIẾN ĐỘ

| Giai đoạn | Tasks | Xong | % |
|-----------|-------|------|------|
| 0 – Setup | 6 | 5 | 83% |
| 0.5 – Refactor | 6 | 3 | 50% |
| 1 – Domain | 9 | 8 | 89% |
| 2 – Config | 10 | 10 | **100%** |
| 3 – Repository | 9 | 8 | 89% |
| 4 – Infrastructure | 9 | 3 | **33%** |
| 5-8 – Admin UC | 23 | 0 | **0%** |
| 9 – User UC | 7 | 0 | **0%** |
| 10-11 – APIs | 10 | 0 | **0%** |
| 12 – Bootstrap | 6 | 5 | 83% |
| 13 – E2E Test | 8 | 0 | **0%** |
| Bonus Auth | 13 | 13 | **100%** |
| 14 – Frontend | 26 | 0 | **0%** |
| **TỔNG** | **142** | **55** | **~39%** |

> **Backend nền tảng vững (~85%)** — Business logic chưa làm (0%) — Frontend chưa bắt đầu (0%)
