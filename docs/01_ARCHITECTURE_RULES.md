# 📐 QUY TẮC KIẾN TRÚC & CHUẨN CODE

> Tất cả thành viên / lượt code PHẢI tuân thủ file này. Đây là nguồn sự thật duy nhất về cấu trúc project.

---

## 1. NGUYÊN TẮC TỔNG QUÁT

- **Dependency Rule**: Layer trong KHÔNG được phụ thuộc layer ngoài.
  - Domain ← Application ← Infrastructure/Persistence ← Presentation ← API
- **Interface được khai báo ở Domain hoặc Application**, implementation ở Infrastructure/Persistence.
- **Không inject DbContext trực tiếp** vào Application hay Presentation. Phải đi qua Repository.
- **Không dùng `static` class** cho business logic.
- **Mỗi file chỉ chứa 1 class/interface**.

---

## 2. DOMAIN LAYER (`Domain/`)

### 2.1 Cấu trúc thư mục

```
Domain/
├── Entities/
│   ├── Theater.cs
│   ├── Room.cs
│   ├── Seat.cs
│   ├── Movie.cs
│   ├── Category.cs
│   ├── Showing.cs
│   ├── Booking.cs
│   ├── BookingDetail.cs
│   └── User.cs
├── Repositories/
│   ├── ITheaterRepository.cs
│   ├── IRoomRepository.cs
│   ├── ISeatRepository.cs
│   ├── IMovieRepository.cs
│   ├── ICategoryRepository.cs
│   ├── IShowingRepository.cs
│   ├── IBookingRepository.cs
│   └── IUnitOfWork.cs
└── Exceptions/
    ├── TheaterNotFoundException.cs
    ├── RoomNotFoundException.cs
    ├── ShowingNotFoundException.cs
    └── SeatAlreadyHoldException.cs
```

### 2.2 Quy tắc Entity

```csharp
// ✅ ĐÚNG - Entity kế thừa base class
public class Room : BaseEntity  // BaseEntity chứa Id (Guid), CreatedAt, UpdatedAt
{
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public Guid TheaterId { get; private set; }

    // Navigation properties
    public Theater Theater { get; private set; }
    public ICollection<Seat> Seats { get; private set; } = new List<Seat>();
    public ICollection<Showing> Showings { get; private set; } = new List<Showing>();

    // Constructor private/protected - dùng factory method
    private Room() { }

    public static Room Create(string name, Guid theaterId)
    {
        return new Room { Name = name, TheaterId = theaterId, IsActive = true };
    }

    public void Update(string name) => Name = name;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
```

### 2.3 Quy tắc IRepository

```csharp
// Domain/Repositories/IRoomRepository.cs
public interface IRoomRepository
{
    // Query methods
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Room> Items, int TotalCount)> GetAllAsync(
        string? search,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Room?> GetLayoutAsync(Guid id, CancellationToken cancellationToken = default); // include Seats

    // Command methods
    void Add(Room room);
    void Update(Room room);
    void Delete(Room room);
}

// Domain/Repositories/IUnitOfWork.cs
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### 2.4 Quy tắc Domain Exception

```csharp
// Domain/Exceptions/RoomNotFoundException.cs
public sealed class RoomNotFoundException : NotFoundException
{
    public RoomNotFoundException(Guid id)
        : base($"Room with id '{id}' was not found.") { }
}
```

---

## 3. CONTRACT LAYER (`Contract/`)

### 3.1 Cấu trúc thư mục

```
Contract/
├── Command/
│   ├── RedisSettings.cs
│   ├── EmailSettings.cs
│   └── JwtSettings.cs
├── Messages/
│   ├── ICommand.cs
│   ├── ICommandHandler.cs
│   ├── IQuery.cs
│   └── IQueryHandler.cs
└── Shared/
    ├── Error.cs
    ├── Result.cs
    ├── ResultT.cs
    ├── IValidationResult.cs
    ├── ValidationResult.cs
    └── ValidationResultT.cs
```

### 3.2 Message Interfaces

```csharp
// Contract/Messages/ICommand.cs
public interface ICommand : IRequest<Result> { }
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

// Contract/Messages/IQuery.cs
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }

// Contract/Messages/ICommandHandler.cs
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand { }

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse> { }

// Contract/Messages/IQueryHandler.cs
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
```

### 3.3 Result Pattern

```csharp
// Contract/Shared/Result.cs
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error) { ... }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

// Contract/Shared/Error.cs
public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided.");
}
```

---

## 4. APPLICATION LAYER (`Application/`)

### 4.1 Cấu trúc thư mục

```
Application/
├── Abstractions/
│   ├── IRedisService.cs
│   ├── ISignalRService.cs
│   ├── ICacheService.cs
│   └── ICurrentUserService.cs
├── Behaviors/
│   ├── LoggingBehavior.cs
│   ├── ValidationBehavior.cs
│   └── TransactionBehavior.cs
├── Common/
│   └── PagedResult.cs
├── DependencyInjection.cs
└── Features/
    ├── Admin/
    │   ├── Rooms/
    │   │   ├── Commands/
    │   │   │   ├── CreateRoom/
    │   │   │   │   ├── CreateRoomCommand.cs
    │   │   │   │   ├── CreateRoomCommandHandler.cs
    │   │   │   │   └── CreateRoomCommandValidator.cs
    │   │   │   └── UpdateRoom/
    │   │   │       ├── UpdateRoomCommand.cs
    │   │   │       ├── UpdateRoomCommandHandler.cs
    │   │   │       └── UpdateRoomCommandValidator.cs
    │   │   ├── Queries/
    │   │   │   ├── GetAllRooms/
    │   │   │   │   ├── GetAllRoomsQuery.cs
    │   │   │   │   └── GetAllRoomsQueryHandler.cs
    │   │   │   └── GetRoomById/
    │   │   │       ├── GetRoomByIdQuery.cs
    │   │   │       └── GetRoomByIdQueryHandler.cs
    │   │   └── Mappers/
    │   │       └── RoomMapper.cs
    │   ├── Showings/
    │   ├── Seats/
    │   └── Movies/
    └── User/
        ├── Movies/
        ├── Showings/
        ├── Rooms/
        └── Bookings/
```

### 4.2 Quy tắc Command

```csharp
// Application/Features/Admin/Rooms/Commands/CreateRoom/CreateRoomCommand.cs
public sealed record CreateRoomCommand(
    string Name,
    Guid TheaterId,
    int TotalRows,
    int TotalColumns
) : ICommand<Guid>;

// Application/Features/Admin/Rooms/Commands/CreateRoom/CreateRoomCommandHandler.cs
internal sealed class CreateRoomCommandHandler : ICommandHandler<CreateRoomCommand, Guid>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoomCommandHandler(IRoomRepository roomRepository, IUnitOfWork unitOfWork)
    {
        _roomRepository = roomRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = Room.Create(request.Name, request.TheaterId);
        _roomRepository.Add(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(room.Id);
    }
}
```

### 4.3 Quy tắc Query

```csharp
// Application/Features/Admin/Rooms/Queries/GetAllRooms/GetAllRoomsQuery.cs
public sealed record GetAllRoomsQuery(
    string? Search,
    bool? IsActive,
    int Page,
    int PageSize
) : IQuery<PagedResult<RoomResponse>>;

// Application/Features/Admin/Rooms/Queries/GetAllRooms/GetAllRoomsQueryHandler.cs
internal sealed class GetAllRoomsQueryHandler : IQueryHandler<GetAllRoomsQuery, PagedResult<RoomResponse>>
{
    private readonly IRoomRepository _roomRepository;

    public async Task<Result<PagedResult<RoomResponse>>> Handle(
        GetAllRoomsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, total) = await _roomRepository.GetAllAsync(
            request.Search, request.IsActive, request.Page, request.PageSize, cancellationToken);

        var response = new PagedResult<RoomResponse>(
            items.Select(RoomMapper.ToResponse),
            total,
            request.Page,
            request.PageSize);

        return Result.Success(response);
    }
}
```

### 4.4 Quy tắc Mapper

```csharp
// Application/Features/Admin/Rooms/Mappers/RoomMapper.cs
// ✅ Manual mapper - KHÔNG dùng AutoMapper
internal static class RoomMapper
{
    public static RoomResponse ToResponse(Room room) => new(
        room.Id,
        room.Name,
        room.IsActive,
        room.TheaterId,
        room.CreatedAt);

    public static RoomDetailResponse ToDetailResponse(Room room) => new(
        room.Id,
        room.Name,
        room.IsActive,
        room.Seats.Select(SeatMapper.ToResponse).ToList());
}
```

### 4.5 Quy tắc Validator

```csharp
// Application/Features/Admin/Rooms/Commands/CreateRoom/CreateRoomCommandValidator.cs
public sealed class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Room name is required.")
            .MaximumLength(100);

        RuleFor(x => x.TheaterId)
            .NotEmpty().WithMessage("Theater ID is required.");
    }
}
```

---

## 5. PERSISTENCE LAYER (`Persistence/`)

### 5.1 Cấu trúc thư mục

```
Persistence/
├── Configurations/
│   ├── TheaterConfiguration.cs
│   ├── RoomConfiguration.cs
│   ├── SeatConfiguration.cs
│   ├── MovieConfiguration.cs
│   ├── CategoryConfiguration.cs
│   ├── ShowingConfiguration.cs
│   ├── BookingConfiguration.cs
│   └── BookingDetailConfiguration.cs
├── DependencyInjection.cs
├── Repositories/
│   ├── TheaterRepository.cs
│   ├── RoomRepository.cs
│   ├── SeatRepository.cs
│   ├── MovieRepository.cs
│   ├── CategoryRepository.cs
│   ├── ShowingRepository.cs
│   └── BookingRepository.cs
├── Migrations/
├── SeedData/
│   └── DatabaseSeeder.cs
└── ApplicationDbContext.cs
```

### 5.2 Quy tắc Configuration (Fluent API)

```csharp
// Persistence/Configurations/RoomConfiguration.cs
internal sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(r => r.Theater)
            .WithMany(t => t.Rooms)
            .HasForeignKey(r => r.TheaterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Seats)
            .WithOne(s => s.Room)
            .HasForeignKey(s => s.RoomId);
    }
}
```

### 5.3 Quy tắc Repository Implementation

```csharp
// Persistence/Repositories/RoomRepository.cs
internal sealed class RoomRepository : IRoomRepository
{
    private readonly ApplicationDbContext _context;

    public RoomRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<(IEnumerable<Room> Items, int TotalCount)> GetAllAsync(
        string? search, bool? isActive, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Rooms.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Name.Contains(search));

        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public void Add(Room room) => _context.Rooms.Add(room);
    public void Update(Room room) => _context.Rooms.Update(room);
    public void Delete(Room room) => _context.Rooms.Remove(room);
}
```

### 5.4 Quy tắc DI trong Persistence

```csharp
// Persistence/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Đăng ký từng repository
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<ITheaterRepository, TheaterRepository>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IShowingRepository, ShowingRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }
}
```

---

## 6. PRESENTATION LAYER (`Presentation/`)

### 6.1 Cấu trúc thư mục

```
Presentation/
├── Abstractions/
│   ├── ApiResponse.cs         → Wrapper response chuẩn
│   └── BaseEndpoint.cs        → Carter module base (nếu cần)
└── APIs/
    ├── Admin/
    │   ├── RoomsModule.cs
    │   ├── ShowingsModule.cs
    │   ├── SeatsModule.cs
    │   └── MoviesModule.cs
    └── User/
        ├── MoviesModule.cs
        ├── ShowingsModule.cs
        ├── RoomsModule.cs
        └── BookingsModule.cs
```

### 6.2 Quy tắc Carter Module (Handler)

```csharp
// Presentation/APIs/Admin/RoomsModule.cs
public class AdminRoomsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/rooms")
            .RequireAuthorization("TheaterOwner")  // Policy name
            .WithTags("Admin - Rooms");

        group.MapGet("/", GetAllRooms);
        group.MapGet("/{id:guid}", GetRoomById);
        group.MapGet("/{id:guid}/layout", GetRoomLayout);
        group.MapPost("/", CreateRoom);
        group.MapPut("/{id:guid}", UpdateRoom);
    }

    // ✅ Handler là static method - KHÔNG phải class riêng
    private static async Task<IResult> GetAllRooms(
        [AsParameters] GetAllRoomsRequest request,   // query params
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetAllRoomsQuery(
            request.Search,
            request.IsActive,
            request.Page,
            request.PageSize);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Success(result.Value))
            : Results.BadRequest(ApiResponse.Failure(result.Error));
    }

    private static async Task<IResult> CreateRoom(
        CreateRoomRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoomCommand(
            request.Name,
            request.TheaterId,
            request.TotalRows,
            request.TotalColumns);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/admin/rooms/{result.Value}", ApiResponse.Success(result.Value))
            : Results.BadRequest(ApiResponse.Failure(result.Error));
    }
}
```

### 6.3 Quy tắc ApiResponse Wrapper

```csharp
// Presentation/Abstractions/ApiResponse.cs
public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T data) => new() { IsSuccess = true, Data = data };
    public static ApiResponse<object> Failure(Error error) => new()
    {
        IsSuccess = false,
        ErrorCode = error.Code,
        ErrorMessage = error.Message
    };
}
```

---

## 7. INFRASTRUCTURE LAYER (`Infrastructure/`)

### 7.1 Cấu trúc thư mục

```
Infrastructure/
├── DependencyInjection.cs
├── Redis/
│   ├── RedisCacheService.cs       → implement ICacheService
│   ├── RedisSeatService.cs        → implement ISeatRedisService
│   └── LuaScripts/
│       ├── HoldSeat.lua
│       └── ReleaseSeat.lua
├── SignalR/
│   ├── SeatHub.cs
│   └── SignalRSeatService.cs
└── BackgroundJobs/
    ├── SessionCleanupJob.cs
    └── SeatSyncJob.cs
```

### 7.2 Quy tắc Interface trong Application (Abstractions)

```csharp
// Application/Abstractions/IRedisService.cs  ← KHAI BÁO Ở ĐÂY
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
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
}
```

---

## 8. CHECKLIST TRƯỚC KHI COMMIT

- [ ] Interface đặt đúng layer (Domain hoặc Application)?
- [ ] Handler trong Presentation chỉ gọi `ISender.Send()`?
- [ ] Repository không chứa business logic?
- [ ] UseCase không inject DbContext trực tiếp?
- [ ] Mỗi Command/Query có Validator tương ứng?
- [ ] Mỗi Entity có Configuration Fluent API riêng?
- [ ] DI được đăng ký trong đúng project (`Persistence/DI`, `Infrastructure/DI`)?
- [ ] Error trả về qua `Result` pattern, không throw exception trong Controller?
