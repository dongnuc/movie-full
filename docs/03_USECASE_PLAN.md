# ⚙️ KẾ HOẠCH USECASE (Application Layer)

> **Tiên quyết**: Repository layer phải hoàn thành trước khi bắt đầu UseCase.
> **Luồng**: Command/Query → Handler → Validator → Mapper → Response DTO

---

## PHẦN A – ADMIN: ROOM USECASES

### A1. GetAllRoomsAdmin (Query)

**Mục đích**: Lấy danh sách phòng có phân trang, filter search + isActive  
**Auth**: TheaterOwner

```
Application/Features/Admin/Rooms/Queries/GetAllRooms/
├── GetAllRoomsQuery.cs
└── GetAllRoomsQueryHandler.cs
```

```csharp
// GetAllRoomsQuery.cs
public sealed record GetAllRoomsQuery(
    string? Search,
    bool? IsActive,
    int Page = 1,
    int PageSize = 10
) : IQuery<PagedResult<RoomResponse>>;

// GetAllRoomsQueryHandler.cs
internal sealed class GetAllRoomsQueryHandler
    : IQueryHandler<GetAllRoomsQuery, PagedResult<RoomResponse>>
{
    private readonly IRoomRepository _repo;

    public GetAllRoomsQueryHandler(IRoomRepository repo) => _repo = repo;

    public async Task<Result<PagedResult<RoomResponse>>> Handle(
        GetAllRoomsQuery request, CancellationToken ct)
    {
        var (items, total) = await _repo.GetAllAsync(
            request.Search, request.IsActive, null,
            request.Page, request.PageSize, ct);

        var response = new PagedResult<RoomResponse>(
            items.Select(RoomMapper.ToResponse),
            total, request.Page, request.PageSize);

        return Result.Success(response);
    }
}
```

**Response DTO**:
```csharp
public sealed record RoomResponse(
    Guid Id,
    string Name,
    int TotalRows,
    int TotalColumns,
    bool IsActive,
    Guid TheaterId,
    string TheaterName,
    DateTime CreatedAt);
```

---

### A2. GetRoomByIdAdmin (Query)

```
Application/Features/Admin/Rooms/Queries/GetRoomById/
├── GetRoomByIdQuery.cs
└── GetRoomByIdQueryHandler.cs
```

```csharp
public sealed record GetRoomByIdQuery(Guid Id) : IQuery<RoomDetailResponse>;

internal sealed class GetRoomByIdQueryHandler : IQueryHandler<GetRoomByIdQuery, RoomDetailResponse>
{
    private readonly IRoomRepository _repo;

    public async Task<Result<RoomDetailResponse>> Handle(GetRoomByIdQuery request, CancellationToken ct)
    {
        var room = await _repo.GetByIdAsync(request.Id, ct);
        if (room is null) return Result.Failure<RoomDetailResponse>(RoomErrors.NotFound(request.Id));
        return Result.Success(RoomMapper.ToDetailResponse(room));
    }
}
```

**Lưu ý**: `RoomErrors` đặt trong `Application/Features/Admin/Rooms/`:
```csharp
public static class RoomErrors
{
    public static Error NotFound(Guid id) => new("Room.NotFound", $"Room {id} not found.");
    public static Error AlreadyExists => new("Room.AlreadyExists", "Room name already exists.");
}
```

---

### A3. GetLayoutRoom (Query – Admin)

```
Application/Features/Admin/Rooms/Queries/GetRoomLayout/
├── GetRoomLayoutQuery.cs
└── GetRoomLayoutQueryHandler.cs
```

```csharp
public sealed record GetRoomLayoutQuery(Guid RoomId) : IQuery<RoomLayoutResponse>;

internal sealed class GetRoomLayoutQueryHandler : IQueryHandler<GetRoomLayoutQuery, RoomLayoutResponse>
{
    private readonly IRoomRepository _repo;
    private readonly ICacheService _cache;

    public async Task<Result<RoomLayoutResponse>> Handle(GetRoomLayoutQuery request, CancellationToken ct)
    {
        // Cache key theo roomId + version tag
        var cacheKey = $"room:layout:{request.RoomId}";
        var cached = await _cache.GetAsync<RoomLayoutResponse>(cacheKey, ct);
        if (cached is not null) return Result.Success(cached);

        var room = await _repo.GetByIdWithSeatsAsync(request.RoomId, ct);
        if (room is null) return Result.Failure<RoomLayoutResponse>(RoomErrors.NotFound(request.RoomId));

        var response = RoomMapper.ToLayoutResponse(room);
        await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(24), ct);
        return Result.Success(response);
    }
}
```

**Response DTO**:
```csharp
public sealed record RoomLayoutResponse(
    Guid RoomId,
    string RoomName,
    int TotalRows,
    int TotalColumns,
    IReadOnlyList<SeatLayoutItem> Seats);

public sealed record SeatLayoutItem(
    Guid SeatId,
    string Row,
    int Column,
    SeatType Type,
    SeatStatus Status,
    decimal Price);
```

---

### A4. CreateRoom (Command)

```
Application/Features/Admin/Rooms/Commands/CreateRoom/
├── CreateRoomCommand.cs
├── CreateRoomCommandHandler.cs
└── CreateRoomCommandValidator.cs
```

```csharp
public sealed record CreateRoomCommand(
    string Name,
    Guid TheaterId,
    int TotalRows,
    int TotalColumns
) : ICommand<Guid>;

internal sealed class CreateRoomCommandHandler : ICommandHandler<CreateRoomCommand, Guid>
{
    private readonly IRoomRepository _roomRepo;
    private readonly ITheaterRepository _theaterRepo;
    private readonly IUnitOfWork _uow;

    public async Task<Result<Guid>> Handle(CreateRoomCommand request, CancellationToken ct)
    {
        // Validate theater exists
        var theater = await _theaterRepo.GetByIdAsync(request.TheaterId, ct);
        if (theater is null)
            return Result.Failure<Guid>(TheaterErrors.NotFound(request.TheaterId));

        var room = Room.Create(request.Name, request.TheaterId, request.TotalRows, request.TotalColumns);
        _roomRepo.Add(room);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(room.Id);
    }
}

public sealed class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TheaterId).NotEmpty();
        RuleFor(x => x.TotalRows).InclusiveBetween(1, 30);
        RuleFor(x => x.TotalColumns).InclusiveBetween(1, 30);
    }
}
```

---

### A5. UpdateRoom (Command)

```csharp
public sealed record UpdateRoomCommand(
    Guid Id, string Name, int TotalRows, int TotalColumns, bool IsActive
) : ICommand;

internal sealed class UpdateRoomCommandHandler : ICommandHandler<UpdateRoomCommand>
{
    private readonly IRoomRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public async Task<Result> Handle(UpdateRoomCommand request, CancellationToken ct)
    {
        var room = await _repo.GetByIdAsync(request.Id, ct);
        if (room is null) return Result.Failure(RoomErrors.NotFound(request.Id));

        room.Update(request.Name, request.TotalRows, request.TotalColumns);
        if (request.IsActive) room.Activate(); else room.Deactivate();

        _repo.Update(room);
        await _uow.SaveChangesAsync(ct);

        // Invalidate cache
        await _cache.RemoveAsync($"room:layout:{request.Id}", ct);

        return Result.Success();
    }
}
```

---

## PHẦN B – ADMIN: SEAT USECASES

### B1. CreateSeatsInBulk (Command)

**Mục đích**: Tạo hàng loạt ghế khi setup phòng lần đầu.

```
Application/Features/Admin/Seats/Commands/CreateSeatsInBulk/
├── CreateSeatsInBulkCommand.cs
├── CreateSeatsInBulkCommandHandler.cs
└── CreateSeatsInBulkCommandValidator.cs
```

```csharp
public sealed record SeatItem(string Row, int Column, SeatType Type, decimal Price);

public sealed record CreateSeatsInBulkCommand(
    Guid RoomId,
    IReadOnlyList<SeatItem> Seats
) : ICommand<int>;    // returns số ghế đã tạo

internal sealed class CreateSeatsInBulkCommandHandler : ICommandHandler<CreateSeatsInBulkCommand, int>
{
    private readonly ISeatRepository _seatRepo;
    private readonly IRoomRepository _roomRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public async Task<Result<int>> Handle(CreateSeatsInBulkCommand request, CancellationToken ct)
    {
        var room = await _roomRepo.GetByIdAsync(request.RoomId, ct);
        if (room is null) return Result.Failure<int>(RoomErrors.NotFound(request.RoomId));

        var seats = request.Seats
            .Select(s => Seat.Create(s.Row, s.Column, request.RoomId, s.Type, s.Price))
            .ToList();

        _seatRepo.AddRange(seats);
        await _uow.SaveChangesAsync(ct);

        // Invalidate layout cache
        await _cache.RemoveAsync($"room:layout:{request.RoomId}", ct);

        return Result.Success(seats.Count);
    }
}
```

---

### B2. UpdateSeatStatus (Command)

```csharp
public sealed record UpdateSeatStatusCommand(Guid SeatId, SeatStatus Status) : ICommand;

internal sealed class UpdateSeatStatusCommandHandler : ICommandHandler<UpdateSeatStatusCommand>
{
    private readonly ISeatRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public async Task<Result> Handle(UpdateSeatStatusCommand request, CancellationToken ct)
    {
        var seat = await _repo.GetByIdAsync(request.SeatId, ct);
        if (seat is null) return Result.Failure(SeatErrors.NotFound(request.SeatId));

        seat.UpdateStatus(request.Status);
        _repo.Update(seat);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync($"room:layout:{seat.RoomId}", ct);
        return Result.Success();
    }
}
```

---

## PHẦN C – ADMIN: SHOWING USECASES

### C1. GetAllShowingAdmin (Query)

```
Application/Features/Admin/Showings/Queries/GetAllShowings/
├── GetAllShowingsQuery.cs
└── GetAllShowingsQueryHandler.cs
```

```csharp
public sealed record GetAllShowingsQuery(
    DateTime? DateStart,
    TimeSpan? Hour,
    Guid? RoomId,
    bool? IsActive,
    int Page = 1,
    int PageSize = 10
) : IQuery<PagedResult<ShowingResponse>>;
```

---

### C2. CreateShowing (Command)

```csharp
public sealed record CreateShowingCommand(
    Guid MovieId,
    Guid RoomId,
    DateTime StartTime
) : ICommand<Guid>;

internal sealed class CreateShowingCommandHandler : ICommandHandler<CreateShowingCommand, Guid>
{
    // Validate không trùng lịch phòng
    // EndTime = StartTime + Movie.DurationMinutes
    // Add showing, SaveChanges
}

public sealed class CreateShowingCommandValidator : AbstractValidator<CreateShowingCommand>
{
    public CreateShowingCommandValidator()
    {
        RuleFor(x => x.MovieId).NotEmpty();
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.StartTime).GreaterThan(DateTime.UtcNow);
    }
}
```

---

### C3. GetShowingDetail (Query)

**Mục đích**: Phim đang chiếu + phòng nào + số vé đã bán + có thể list ghế đã bán.

```csharp
public sealed record GetShowingDetailQuery(Guid ShowingId) : IQuery<ShowingDetailResponse>;

public sealed record ShowingDetailResponse(
    Guid Id,
    string MovieTitle,
    string RoomName,
    DateTime StartTime,
    DateTime EndTime,
    ShowingStatus Status,
    int TotalSeats,
    int BookedSeats,
    int AvailableSeats,
    IReadOnlyList<Guid> BookedSeatIds);
```

---

### C4. UpdateShowing (Command)

```csharp
public sealed record UpdateShowingCommand(
    Guid Id,
    DateTime StartTime,
    Guid RoomId
) : ICommand;
```

---

## PHẦN D – ADMIN: MOVIE USECASES

### D1. GetAllMovieAdmin (Query)

```csharp
public sealed record GetAllMoviesAdminQuery(
    string? Search,
    Guid? CategoryId,
    bool? IsActive,
    int Page = 1,
    int PageSize = 10
) : IQuery<PagedResult<MovieResponse>>;
```

### D2. GetMovieDetailAdmin (Query)

```csharp
public sealed record GetMovieDetailAdminQuery(Guid Id) : IQuery<MovieDetailResponse>;
```

### D3. CreateMovie (Command)

```csharp
public sealed record CreateMovieCommand(
    string Title,
    string Description,
    int DurationMinutes,
    string PosterUrl,
    string TrailerUrl,
    DateTime ReleaseDate,
    Guid CategoryId
) : ICommand<Guid>;

public sealed class CreateMovieCommandValidator : AbstractValidator<CreateMovieCommand>
{
    public CreateMovieCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DurationMinutes).InclusiveBetween(30, 300);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.ReleaseDate).NotEmpty();
    }
}
```

### D4. UpdateMovie (Command)

```csharp
public sealed record UpdateMovieCommand(
    Guid Id, string Title, string Description, int DurationMinutes,
    string PosterUrl, string TrailerUrl, DateTime ReleaseDate
) : ICommand;
```

---

## PHẦN E – ADMIN: THEATER USECASES (System Admin)

### E1. GetAllTheater (Query)

```csharp
public sealed record GetAllTheatersQuery(
    string? Search,
    bool? IsActive,
    int Page = 1,
    int PageSize = 10
) : IQuery<PagedResult<TheaterResponse>>;
```

---

## PHẦN F – USER: MOVIE USECASES

### F1. GetAllMovieUser (Query)

```csharp
public sealed record GetAllMoviesUserQuery(
    string? Search,
    Guid? CategoryId,
    int Page = 1,
    int PageSize = 12
) : IQuery<PagedResult<MovieUserResponse>>;

// Chỉ trả về IsActive = true
// Dùng cache: "movies:homepage:default" cho query mặc định (search=null, categoryId=null)
```

### F2. GetMovieDetailsUser (Query)

**Mục đích**: Chi tiết phim + danh sách rạp có suất chiếu + đếm số suất chiếu.

```csharp
public sealed record GetMovieDetailsUserQuery(Guid Id) : IQuery<MovieDetailsUserResponse>;

public sealed record MovieDetailsUserResponse(
    Guid Id,
    string Title,
    string Description,
    int DurationMinutes,
    string PosterUrl,
    string TrailerUrl,
    DateTime ReleaseDate,
    string CategoryName,
    IReadOnlyList<TheaterWithShowingCount> Theaters);

public sealed record TheaterWithShowingCount(
    Guid TheaterId,
    string TheaterName,
    string City,
    int ShowingCount);
```

---

## PHẦN G – USER: SHOWING USECASES

### G1. GetAllShowingUser (Query)

**Mục đích**: Hiển thị các suất chiếu còn trống theo movieId + theaterId.

```csharp
public sealed record GetAllShowingsUserQuery(
    Guid MovieId,
    Guid TheaterId
) : IQuery<IReadOnlyList<ShowingUserResponse>>;

public sealed record ShowingUserResponse(
    Guid Id,
    DateTime StartTime,
    DateTime EndTime,
    string RoomName,
    int TotalSeats,
    int AvailableSeats,   // TotalSeats - Booked - Held (từ Redis)
    ShowingStatus Status);
```

---

## PHẦN H – USER: ROOM USECASE

### H1. GetLayoutRoomUser (Query)

**Mục đích**: Layout phòng theo suất chiếu cụ thể, kèm ghế đã booked + đang hold.

```csharp
public sealed record GetLayoutRoomUserQuery(Guid ShowingId) : IQuery<RoomLayoutUserResponse>;

public sealed record RoomLayoutUserResponse(
    Guid ShowingId,
    Guid RoomId,
    string RoomName,
    IReadOnlyList<SeatLayoutUserItem> Seats);

public enum SeatDisplayStatus { Available, Booked, Held, Maintenance }

public sealed record SeatLayoutUserItem(
    Guid SeatId,
    string Row,
    int Column,
    SeatType Type,
    decimal Price,
    SeatDisplayStatus DisplayStatus);   // composite từ DB + Redis
```

**Handler logic**:
```csharp
// 1. Lấy Showing → tìm RoomId
// 2. Lấy layout từ cache/DB theo RoomId
// 3. Lấy BookedSeatIds từ DB (IShowingRepository.GetBookedSeatIdsAsync)
// 4. Lấy HeldSeatIds từ Redis (IRedisService.GetHeldSeatsAsync)
// 5. Merge → gán DisplayStatus cho từng seat
// 6. Return
```

---

## PHẦN I – USER: BOOKING USECASES

### I1. CreateBooking (Command)

**Luồng phức tạp nhất – cần atomic qua Redis LUA**:

```csharp
public sealed record CreateBookingCommand(
    Guid ShowingId,
    IReadOnlyList<Guid> SeatIds,
    string SessionId
) : ICommand<Guid>;

internal sealed class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, Guid>
{
    // 1. Validate: seat tồn tại, chưa booked trong DB
    // 2. Hold seats trong Redis (LUA script atomic)
    //    → Nếu bất kỳ seat nào đã hold/booked → return failure
    // 3. Tạo Booking + BookingDetails trong DB với Status = Pending
    // 4. SignalR broadcast: seat status changed
    // 5. Return BookingId
}
```

### I2. ViewHistoryBooking (Query)

```csharp
public sealed record GetBookingHistoryQuery(Guid UserId) : IQuery<IReadOnlyList<BookingHistoryItem>>;

public sealed record BookingHistoryItem(
    Guid BookingId,
    string MovieTitle,
    string RoomName,
    DateTime ShowingTime,
    IReadOnlyList<string> Seats,   // "A1", "A2"...
    decimal TotalAmount,
    BookingStatus Status,
    DateTime CreatedAt);
```

---

## MAPPERS (vị trí đặt)

```
Application/Features/Admin/Rooms/Mappers/RoomMapper.cs
Application/Features/Admin/Showings/Mappers/ShowingMapper.cs
Application/Features/Admin/Movies/Mappers/MovieMapper.cs
Application/Features/User/Movies/Mappers/MovieUserMapper.cs
Application/Features/User/Bookings/Mappers/BookingMapper.cs
```

Tất cả mapper là `internal static class`, chỉ chứa `static` methods, KHÔNG dùng AutoMapper.

---

## COMMON TYPES

```
Application/Common/PagedResult.cs
```

```csharp
public sealed record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
```

---

## DEPENDENCY INJECTION – APPLICATION

```
Application/DependencyInjection.cs
```

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    var assembly = typeof(DependencyInjection).Assembly;

    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
    services.AddValidatorsFromAssembly(assembly);

    // Register Behaviors (thứ tự quan trọng)
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    return services;
}
```

---

## CHECKLIST USECASE

**Admin – Room**
- [ ] GetAllRoomsQuery + Handler
- [ ] GetRoomByIdQuery + Handler
- [ ] GetRoomLayoutQuery + Handler (với cache)
- [ ] CreateRoomCommand + Handler + Validator
- [ ] UpdateRoomCommand + Handler + Validator
- [ ] RoomMapper hoàn chỉnh
- [ ] RoomErrors khai báo

**Admin – Seat**
- [ ] CreateSeatsInBulkCommand + Handler + Validator
- [ ] UpdateSeatStatusCommand + Handler

**Admin – Showing**
- [ ] GetAllShowingsQuery + Handler
- [ ] GetShowingDetailQuery + Handler
- [ ] CreateShowingCommand + Handler + Validator
- [ ] UpdateShowingCommand + Handler

**Admin – Movie**
- [ ] GetAllMoviesAdminQuery + Handler
- [ ] GetMovieDetailAdminQuery + Handler
- [ ] CreateMovieCommand + Handler + Validator
- [ ] UpdateMovieCommand + Handler

**Admin – Theater**
- [ ] GetAllTheatersQuery + Handler

**User – Movie**
- [ ] GetAllMoviesUserQuery + Handler (với cache homepage)
- [ ] GetMovieDetailsUserQuery + Handler

**User – Showing**
- [ ] GetAllShowingsUserQuery + Handler

**User – Room**
- [ ] GetLayoutRoomUserQuery + Handler (merge DB + Redis)

**User – Booking**
- [ ] CreateBookingCommand + Handler (atomic Redis + DB)
- [ ] GetBookingHistoryQuery + Handler
