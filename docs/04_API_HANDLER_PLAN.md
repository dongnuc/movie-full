# 🌐 KẾ HOẠCH API & HANDLER (Presentation Layer)

> **Tiên quyết**: UseCase layer phải hoàn thành trước.  
> **Công cụ**: Carter (Minimal API modules), ISender (MediatR), ApiResponse wrapper.

---

## NGUYÊN TẮC HANDLER (Nhắc lại từ Architecture Rules)

1. Handler chỉ làm **1 việc**: map Request DTO → Command/Query → gọi `ISender.Send()` → map result → trả về `IResult`.
2. **Không có business logic** trong handler.
3. Sử dụng **`[AsParameters]`** cho query string params.
4. Mỗi Entity là **1 module** (`ICarterModule`), đặt trong đúng folder Admin/User.
5. Luôn dùng **`ApiResponse<T>` wrapper** cho response.

---

## CẤU TRÚC FOLDER

```
Presentation/
├── Abstractions/
│   ├── ApiResponse.cs
│   └── PaginationRequest.cs    → shared query params
└── APIs/
    ├── Admin/
    │   ├── AdminRoomsModule.cs
    │   ├── AdminSeatsModule.cs
    │   ├── AdminShowingsModule.cs
    │   ├── AdminMoviesModule.cs
    │   └── AdminTheatersModule.cs
    └── User/
        ├── UserMoviesModule.cs
        ├── UserShowingsModule.cs
        ├── UserRoomsModule.cs
        └── UserBookingsModule.cs
```

---

## SHARED TYPES (Abstractions)

### ApiResponse

```csharp
// Presentation/Abstractions/ApiResponse.cs
public sealed record ApiResponse<T>(
    bool IsSuccess,
    T? Data,
    string? ErrorCode,
    string? ErrorMessage,
    DateTime Timestamp)
{
    public static ApiResponse<T> Success(T data) =>
        new(true, data, null, null, DateTime.UtcNow);

    public static ApiResponse<T> Failure(Error error) =>
        new(false, default, error.Code, error.Message, DateTime.UtcNow);
}

// Helper static class
public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data) => ApiResponse<T>.Success(data);
    public static ApiResponse<object> Fail(Error error) => ApiResponse<object>.Failure(error);
}
```

### PaginationRequest

```csharp
// Presentation/Abstractions/PaginationRequest.cs
public sealed record PaginationRequest(
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 10);
```

---

## MODULE 1 – ADMIN ROOMS

```
Presentation/APIs/Admin/AdminRoomsModule.cs
```

### Route Group
```
Base: /api/admin/rooms
Auth: RequireAuthorization("TheaterOwner")
Tags: "Admin - Rooms"
```

### Endpoints

| Method | Route | Handler | Request | Response |
|--------|-------|---------|---------|----------|
| GET | `/` | GetAllRooms | query params | `PagedResult<RoomResponse>` |
| GET | `/{id}` | GetRoomById | route param | `RoomDetailResponse` |
| GET | `/{id}/layout` | GetRoomLayout | route param | `RoomLayoutResponse` |
| POST | `/` | CreateRoom | body | `Guid` (new roomId) |
| PUT | `/{id}` | UpdateRoom | route + body | `204 NoContent` |

### Request DTOs

```csharp
// Query params cho GET /
public sealed record GetAllRoomsRequest(
    [FromQuery] string? Search,
    [FromQuery] bool? IsActive,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 10);

// Body cho POST /
public sealed record CreateRoomRequest(
    string Name,
    Guid TheaterId,
    int TotalRows,
    int TotalColumns);

// Body cho PUT /{id}
public sealed record UpdateRoomRequest(
    string Name,
    int TotalRows,
    int TotalColumns,
    bool IsActive);
```

### Module Code

```csharp
// Presentation/APIs/Admin/AdminRoomsModule.cs
public class AdminRoomsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/rooms")
            .RequireAuthorization("TheaterOwner")
            .WithTags("Admin - Rooms");

        group.MapGet("/", GetAllRooms);
        group.MapGet("/{id:guid}", GetRoomById);
        group.MapGet("/{id:guid}/layout", GetRoomLayout);
        group.MapPost("/", CreateRoom);
        group.MapPut("/{id:guid}", UpdateRoom);
    }

    private static async Task<IResult> GetAllRooms(
        [AsParameters] GetAllRoomsRequest req,
        ISender sender, CancellationToken ct)
    {
        var query = new GetAllRoomsQuery(req.Search, req.IsActive, req.Page, req.PageSize);
        var result = await sender.Send(query, ct);
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(result.Error));
    }

    private static async Task<IResult> GetRoomById(
        Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetRoomByIdQuery(id), ct);
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.NotFound(ApiResponse.Fail(result.Error));
    }

    private static async Task<IResult> GetRoomLayout(
        Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetRoomLayoutQuery(id), ct);
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.NotFound(ApiResponse.Fail(result.Error));
    }

    private static async Task<IResult> CreateRoom(
        CreateRoomRequest req, ISender sender, CancellationToken ct)
    {
        var command = new CreateRoomCommand(req.Name, req.TheaterId, req.TotalRows, req.TotalColumns);
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.Created($"/api/admin/rooms/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(result.Error));
    }

    private static async Task<IResult> UpdateRoom(
        Guid id, UpdateRoomRequest req, ISender sender, CancellationToken ct)
    {
        var command = new UpdateRoomCommand(id, req.Name, req.TotalRows, req.TotalColumns, req.IsActive);
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(result.Error));
    }
}
```

---

## MODULE 2 – ADMIN SEATS

```
Presentation/APIs/Admin/AdminSeatsModule.cs
```

### Endpoints

| Method | Route | Handler | Mô tả |
|--------|-------|---------|-------|
| POST | `/api/admin/rooms/{roomId}/seats/bulk` | CreateSeatsInBulk | Tạo nhiều ghế cho 1 phòng |
| PUT | `/api/admin/seats/{id}/status` | UpdateSeatStatus | Cập nhật trạng thái ghế |

### Request DTOs

```csharp
public sealed record CreateSeatsInBulkRequest(
    IReadOnlyList<SeatItemRequest> Seats);

public sealed record SeatItemRequest(
    string Row,
    int Column,
    SeatType Type,
    decimal Price);

public sealed record UpdateSeatStatusRequest(SeatStatus Status);
```

### Module Code

```csharp
public class AdminSeatsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .RequireAuthorization("TheaterOwner")
            .WithTags("Admin - Seats");

        group.MapPost("/rooms/{roomId:guid}/seats/bulk", CreateSeatsInBulk);
        group.MapPut("/seats/{id:guid}/status", UpdateSeatStatus);
    }

    private static async Task<IResult> CreateSeatsInBulk(
        Guid roomId, CreateSeatsInBulkRequest req, ISender sender, CancellationToken ct)
    {
        var command = new CreateSeatsInBulkCommand(
            roomId,
            req.Seats.Select(s => new SeatItem(s.Row, s.Column, s.Type, s.Price)).ToList());
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(result.Error));
    }

    private static async Task<IResult> UpdateSeatStatus(
        Guid id, UpdateSeatStatusRequest req, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateSeatStatusCommand(id, req.Status), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(result.Error));
    }
}
```

---

## MODULE 3 – ADMIN SHOWINGS

```
Presentation/APIs/Admin/AdminShowingsModule.cs
```

### Endpoints

| Method | Route | Handler | Mô tả |
|--------|-------|---------|-------|
| GET | `/api/admin/showings` | GetAllShowings | Phân trang, filter |
| GET | `/api/admin/showings/{id}` | GetShowingDetail | Chi tiết + vé đã bán |
| POST | `/api/admin/showings` | CreateShowing | Tạo suất chiếu |
| PUT | `/api/admin/showings/{id}` | UpdateShowing | Cập nhật giờ, phòng |

### Request DTOs

```csharp
public sealed record GetAllShowingsRequest(
    [FromQuery] DateTime? DateStart,
    [FromQuery] TimeSpan? Hour,
    [FromQuery] Guid? RoomId,
    [FromQuery] bool? IsActive,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 10);

public sealed record CreateShowingRequest(
    Guid MovieId,
    Guid RoomId,
    DateTime StartTime);

public sealed record UpdateShowingRequest(
    DateTime StartTime,
    Guid RoomId);
```

### Module Code (pattern tương tự AdminRoomsModule)

```csharp
public class AdminShowingsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/showings")
            .RequireAuthorization("TheaterOwner")
            .WithTags("Admin - Showings");

        group.MapGet("/", GetAllShowings);
        group.MapGet("/{id:guid}", GetShowingDetail);
        group.MapPost("/", CreateShowing);
        group.MapPut("/{id:guid}", UpdateShowing);
    }

    private static async Task<IResult> GetAllShowings(
        [AsParameters] GetAllShowingsRequest req, ISender sender, CancellationToken ct)
    {
        var query = new GetAllShowingsQuery(
            req.DateStart, req.Hour, req.RoomId, req.IsActive, req.Page, req.PageSize);
        var result = await sender.Send(query, ct);
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(result.Error));
    }

    private static async Task<IResult> GetShowingDetail(
        Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetShowingDetailQuery(id), ct);
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.NotFound(ApiResponse.Fail(result.Error));
    }

    private static async Task<IResult> CreateShowing(
        CreateShowingRequest req, ISender sender, CancellationToken ct)
    {
        var command = new CreateShowingCommand(req.MovieId, req.RoomId, req.StartTime);
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.Created($"/api/admin/showings/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.BadRequest(ApiResponse.Fail(result.Error));
    }

    private static async Task<IResult> UpdateShowing(
        Guid id, UpdateShowingRequest req, ISender sender, CancellationToken ct)
    {
        var command = new UpdateShowingCommand(id, req.StartTime, req.RoomId);
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(ApiResponse.Fail(result.Error));
    }
}
```

---

## MODULE 4 – ADMIN MOVIES

```
Presentation/APIs/Admin/AdminMoviesModule.cs
```

### Endpoints

| Method | Route | Auth | Mô tả |
|--------|-------|------|-------|
| GET | `/api/admin/movies` | TheaterOwner | Danh sách + filter + phân trang |
| GET | `/api/admin/movies/{id}` | TheaterOwner | Chi tiết |
| POST | `/api/admin/movies` | SystemAdmin | Thêm phim mới |
| PUT | `/api/admin/movies/{id}` | SystemAdmin | Cập nhật phim |

### Request DTOs

```csharp
public sealed record GetAllMoviesAdminRequest(
    [FromQuery] string? Search,
    [FromQuery] Guid? CategoryId,
    [FromQuery] bool? IsActive,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 10);

public sealed record CreateMovieRequest(
    string Title,
    string Description,
    int DurationMinutes,
    string PosterUrl,
    string TrailerUrl,
    DateTime ReleaseDate,
    Guid CategoryId);

public sealed record UpdateMovieRequest(
    string Title,
    string Description,
    int DurationMinutes,
    string PosterUrl,
    string TrailerUrl,
    DateTime ReleaseDate);
```

---

## MODULE 5 – ADMIN THEATERS (System Admin)

```
Presentation/APIs/Admin/AdminTheatersModule.cs
```

### Endpoints

| Method | Route | Mô tả |
|--------|-------|-------|
| GET | `/api/admin/theaters` | Danh sách rạp |

```csharp
public sealed record GetAllTheatersRequest(
    [FromQuery] string? Search,
    [FromQuery] bool? IsActive,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 10);
```

---

## MODULE 6 – USER MOVIES

```
Presentation/APIs/User/UserMoviesModule.cs
```

### Endpoints

| Method | Route | Mô tả |
|--------|-------|-------|
| GET | `/api/movies` | Danh sách phim public (homepage) |
| GET | `/api/movies/{id}` | Chi tiết phim + rạp + số suất chiếu |

```csharp
public class UserMoviesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/movies")
            .WithTags("User - Movies")
            .AllowAnonymous();

        group.MapGet("/", GetAllMovies);
        group.MapGet("/{id:guid}", GetMovieDetails);
    }

    private static async Task<IResult> GetAllMovies(
        [AsParameters] GetAllMoviesUserRequest req, ISender sender, CancellationToken ct)
    {
        var query = new GetAllMoviesUserQuery(req.Search, req.CategoryId, req.Page, req.PageSize);
        var result = await sender.Send(query, ct);
        return Results.Ok(ApiResponse.Ok(result.Value));
    }

    private static async Task<IResult> GetMovieDetails(
        Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetMovieDetailsUserQuery(id), ct);
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.NotFound(ApiResponse.Fail(result.Error));
    }
}

public sealed record GetAllMoviesUserRequest(
    [FromQuery] string? Search,
    [FromQuery] Guid? CategoryId,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 12);
```

---

## MODULE 7 – USER SHOWINGS

```
Presentation/APIs/User/UserShowingsModule.cs
```

### Endpoints

| Method | Route | Mô tả |
|--------|-------|-------|
| GET | `/api/showings` | Các suất chiếu theo movieId + theaterId |

```csharp
public sealed record GetShowingsUserRequest(
    [FromQuery] Guid MovieId,
    [FromQuery] Guid TheaterId);
```

---

## MODULE 8 – USER ROOMS

```
Presentation/APIs/User/UserRoomsModule.cs
```

### Endpoints

| Method | Route | Mô tả |
|--------|-------|-------|
| GET | `/api/rooms/layout/{showingId}` | Layout phòng theo suất chiếu + trạng thái ghế |

```csharp
public class UserRoomsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/rooms/layout/{showingId:guid}", GetRoomLayout)
            .RequireAuthorization()
            .WithTags("User - Rooms");
    }

    private static async Task<IResult> GetRoomLayout(
        Guid showingId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetLayoutRoomUserQuery(showingId), ct);
        return result.IsSuccess
            ? Results.Ok(ApiResponse.Ok(result.Value))
            : Results.NotFound(ApiResponse.Fail(result.Error));
    }
}
```

---

## MODULE 9 – USER BOOKINGS

```
Presentation/APIs/User/UserBookingsModule.cs
```

### Endpoints

| Method | Route | Auth | Mô tả |
|--------|-------|------|-------|
| POST | `/api/bookings` | User | Tạo booking + hold ghế |
| GET | `/api/bookings/history` | User | Lịch sử đặt vé |

### Request DTOs

```csharp
public sealed record CreateBookingRequest(
    Guid ShowingId,
    IReadOnlyList<Guid> SeatIds,
    string SessionId);
```

### Module Code

```csharp
public class UserBookingsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .RequireAuthorization("User")
            .WithTags("User - Bookings");

        group.MapPost("/", CreateBooking);
        group.MapGet("/history", GetBookingHistory);
    }

    private static async Task<IResult> CreateBooking(
        CreateBookingRequest req,
        ISender sender,
        ICurrentUserService currentUser,
        CancellationToken ct)
    {
        var command = new CreateBookingCommand(req.ShowingId, req.SeatIds, req.SessionId);
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.Created($"/api/bookings/{result.Value}", ApiResponse.Ok(result.Value))
            : Results.Conflict(ApiResponse.Fail(result.Error));     // 409 nếu ghế đã bị hold
    }

    private static async Task<IResult> GetBookingHistory(
        ISender sender,
        ICurrentUserService currentUser,
        CancellationToken ct)
    {
        var query = new GetBookingHistoryQuery(currentUser.UserId);
        var result = await sender.Send(query, ct);
        return Results.Ok(ApiResponse.Ok(result.Value));
    }
}
```

---

## ĐĂNG KÝ CARTER TRONG API PROJECT

```csharp
// API/Program.cs
var app = builder.Build();

app.MapCarter();   // tự scan tất cả ICarterModule

// DI trong API/DependencyInjection.cs hoặc trực tiếp builder
builder.Services.AddCarter();
```

---

## BẢNG TỔNG HỢP TẤT CẢ ENDPOINTS

| # | Method | Route | Auth | UseCase |
|---|--------|-------|------|---------|
| 1 | GET | `/api/admin/rooms` | TheaterOwner | GetAllRoomsQuery |
| 2 | GET | `/api/admin/rooms/{id}` | TheaterOwner | GetRoomByIdQuery |
| 3 | GET | `/api/admin/rooms/{id}/layout` | TheaterOwner | GetRoomLayoutQuery |
| 4 | POST | `/api/admin/rooms` | TheaterOwner | CreateRoomCommand |
| 5 | PUT | `/api/admin/rooms/{id}` | TheaterOwner | UpdateRoomCommand |
| 6 | POST | `/api/admin/rooms/{roomId}/seats/bulk` | TheaterOwner | CreateSeatsInBulkCommand |
| 7 | PUT | `/api/admin/seats/{id}/status` | TheaterOwner | UpdateSeatStatusCommand |
| 8 | GET | `/api/admin/showings` | TheaterOwner | GetAllShowingsQuery |
| 9 | GET | `/api/admin/showings/{id}` | TheaterOwner | GetShowingDetailQuery |
| 10 | POST | `/api/admin/showings` | TheaterOwner | CreateShowingCommand |
| 11 | PUT | `/api/admin/showings/{id}` | TheaterOwner | UpdateShowingCommand |
| 12 | GET | `/api/admin/movies` | TheaterOwner | GetAllMoviesAdminQuery |
| 13 | GET | `/api/admin/movies/{id}` | TheaterOwner | GetMovieDetailAdminQuery |
| 14 | POST | `/api/admin/movies` | SystemAdmin | CreateMovieCommand |
| 15 | PUT | `/api/admin/movies/{id}` | SystemAdmin | UpdateMovieCommand |
| 16 | GET | `/api/admin/theaters` | SystemAdmin | GetAllTheatersQuery |
| 17 | GET | `/api/movies` | Anonymous | GetAllMoviesUserQuery |
| 18 | GET | `/api/movies/{id}` | Anonymous | GetMovieDetailsUserQuery |
| 19 | GET | `/api/showings` | Anonymous | GetAllShowingsUserQuery |
| 20 | GET | `/api/rooms/layout/{showingId}` | User | GetLayoutRoomUserQuery |
| 21 | POST | `/api/bookings` | User | CreateBookingCommand |
| 22 | GET | `/api/bookings/history` | User | GetBookingHistoryQuery |

---

## CHECKLIST API & HANDLER

**Admin Modules**
- [ ] AdminRoomsModule (5 endpoints)
- [ ] AdminSeatsModule (2 endpoints)
- [ ] AdminShowingsModule (4 endpoints)
- [ ] AdminMoviesModule (4 endpoints)
- [ ] AdminTheatersModule (1 endpoint)

**User Modules**
- [ ] UserMoviesModule (2 endpoints)
- [ ] UserShowingsModule (1 endpoint)
- [ ] UserRoomsModule (1 endpoint)
- [ ] UserBookingsModule (2 endpoints)

**Infra**
- [ ] Carter đăng ký trong Program.cs
- [ ] ApiResponse wrapper hoàn chỉnh
- [ ] Authorization policies cấu hình ("TheaterOwner", "SystemAdmin", "User")
- [ ] Global exception middleware
- [ ] Swagger/OpenAPI theo tag
