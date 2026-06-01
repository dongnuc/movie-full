# 🗄️ KẾ HOẠCH REPOSITORY LAYER

> **Luồng bắt buộc**: Domain Entity → Fluent Configuration → Migration → Repository Implementation → DI Registration

---

## BƯỚC 0 – DOMAIN ENTITIES & IRepository (viết trước, không implement)

### 0.1 BaseEntity

```
Domain/Entities/BaseEntity.cs
```

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    public void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
```

---

## BƯỚC 1 – ENTITY: THEATER

### 1.1 Entity

```
Domain/Entities/Theater.cs
```

```csharp
public class Theater : BaseEntity
{
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }
    public bool IsActive { get; private set; }
    public Guid OwnerId { get; private set; }      // FK → User (TheaterOwner)

    public ICollection<Room> Rooms { get; private set; } = new List<Room>();

    private Theater() { }

    public static Theater Create(string name, string address, string city, Guid ownerId)
        => new() { Name = name, Address = address, City = city, OwnerId = ownerId, IsActive = true };

    public void Update(string name, string address, string city)
    {
        Name = name; Address = address; City = city;
        SetUpdatedAt();
    }

    public void Deactivate() { IsActive = false; SetUpdatedAt(); }
    public void Activate()   { IsActive = true;  SetUpdatedAt(); }
}
```

### 1.2 IRepository

```
Domain/Repositories/ITheaterRepository.cs
```

```csharp
public interface ITheaterRepository
{
    Task<Theater?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IEnumerable<Theater> Items, int Total)> GetAllAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken ct = default);
    void Add(Theater theater);
    void Update(Theater theater);
}
```

### 1.3 Fluent API Configuration

```
Persistence/Configurations/TheaterConfiguration.cs
```

```csharp
internal sealed class TheaterConfiguration : IEntityTypeConfiguration<Theater>
{
    public void Configure(EntityTypeBuilder<Theater> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Address).IsRequired().HasMaxLength(500);
        builder.Property(t => t.City).IsRequired().HasMaxLength(100);
        builder.Property(t => t.IsActive).IsRequired().HasDefaultValue(true);
        builder.HasMany(t => t.Rooms).WithOne(r => r.Theater)
            .HasForeignKey(r => r.TheaterId).OnDelete(DeleteBehavior.Restrict);
    }
}
```

### 1.4 Repository Implementation

```
Persistence/Repositories/TheaterRepository.cs
```

```csharp
internal sealed class TheaterRepository : ITheaterRepository
{
    private readonly ApplicationDbContext _context;
    public TheaterRepository(ApplicationDbContext context) => _context = context;

    public async Task<Theater?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Theaters.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<(IEnumerable<Theater> Items, int Total)> GetAllAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _context.Theaters.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(t => t.Name.Contains(search) || t.City.Contains(search));
        if (isActive.HasValue)
            q = q.Where(t => t.IsActive == isActive.Value);

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public void Add(Theater t)    => _context.Theaters.Add(t);
    public void Update(Theater t) => _context.Theaters.Update(t);
}
```

---

## BƯỚC 2 – ENTITY: ROOM

### 2.1 Entity

```
Domain/Entities/Room.cs
```

```csharp
public class Room : BaseEntity
{
    public string Name { get; private set; }
    public int TotalRows { get; private set; }
    public int TotalColumns { get; private set; }
    public bool IsActive { get; private set; }
    public Guid TheaterId { get; private set; }

    public Theater Theater { get; private set; }
    public ICollection<Seat> Seats { get; private set; } = new List<Seat>();
    public ICollection<Showing> Showings { get; private set; } = new List<Showing>();

    private Room() { }

    public static Room Create(string name, Guid theaterId, int rows, int cols)
        => new() { Name = name, TheaterId = theaterId, TotalRows = rows, TotalColumns = cols, IsActive = true };

    public void Update(string name, int rows, int cols)
    {
        Name = name; TotalRows = rows; TotalColumns = cols;
        SetUpdatedAt();
    }

    public void Deactivate() { IsActive = false; SetUpdatedAt(); }
    public void Activate()   { IsActive = true;  SetUpdatedAt(); }
}
```

### 2.2 IRepository

```
Domain/Repositories/IRoomRepository.cs
```

```csharp
public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Room?> GetByIdWithSeatsAsync(Guid id, CancellationToken ct = default);  // cho GetLayout
    Task<(IEnumerable<Room> Items, int Total)> GetAllAsync(
        string? search, bool? isActive, Guid? theaterId,
        int page, int pageSize, CancellationToken ct = default);
    void Add(Room room);
    void Update(Room room);
}
```

### 2.3 Fluent Config + Repository Implementation

> Tương tự mẫu Theater ở trên. `GetByIdWithSeatsAsync` dùng `.Include(r => r.Seats)`.

---

## BƯỚC 3 – ENTITY: SEAT

### 3.1 Entity

```
Domain/Entities/Seat.cs
```

```csharp
public enum SeatType { Regular, VIP, Couple }
public enum SeatStatus { Active, Inactive, Maintenance }

public class Seat : BaseEntity
{
    public string Row { get; private set; }          // "A", "B", "C"...
    public int Column { get; private set; }          // 1, 2, 3...
    public SeatType Type { get; private set; }
    public SeatStatus Status { get; private set; }
    public decimal Price { get; private set; }
    public Guid RoomId { get; private set; }

    public Room Room { get; private set; }

    private Seat() { }

    public static Seat Create(string row, int column, Guid roomId,
        SeatType type = SeatType.Regular, decimal price = 0)
        => new() { Row = row, Column = column, RoomId = roomId,
                   Type = type, Price = price, Status = SeatStatus.Active };

    public void UpdateStatus(SeatStatus status) { Status = status; SetUpdatedAt(); }
    public void UpdatePrice(decimal price)       { Price = price;   SetUpdatedAt(); }
}
```

### 3.2 IRepository

```
Domain/Repositories/ISeatRepository.cs
```

```csharp
public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Seat>> GetByRoomIdAsync(Guid roomId, CancellationToken ct = default);
    Task<IEnumerable<Seat>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    void AddRange(IEnumerable<Seat> seats);
    void Update(Seat seat);
}
```

---

## BƯỚC 4 – ENTITY: CATEGORY + MOVIE

### 4.1 Category Entity

```
Domain/Entities/Category.cs
```

```csharp
public class Category : BaseEntity
{
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<Movie> Movies { get; private set; } = new List<Movie>();

    private Category() { }
    public static Category Create(string name) => new() { Name = name, IsActive = true };
    public void Update(string name) { Name = name; SetUpdatedAt(); }
}
```

### 4.2 Movie Entity

```
Domain/Entities/Movie.cs
```

```csharp
public class Movie : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int DurationMinutes { get; private set; }
    public string PosterUrl { get; private set; }
    public string TrailerUrl { get; private set; }
    public DateTime ReleaseDate { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CategoryId { get; private set; }

    public Category Category { get; private set; }
    public ICollection<Showing> Showings { get; private set; } = new List<Showing>();

    private Movie() { }

    public static Movie Create(string title, string description, int duration,
        string poster, string trailer, DateTime releaseDate, Guid categoryId)
        => new() { Title = title, Description = description,
                   DurationMinutes = duration, PosterUrl = poster,
                   TrailerUrl = trailer, ReleaseDate = releaseDate,
                   CategoryId = categoryId, IsActive = true };

    public void Update(string title, string description, int duration,
        string poster, string trailer, DateTime releaseDate)
    {
        Title = title; Description = description; DurationMinutes = duration;
        PosterUrl = poster; TrailerUrl = trailer; ReleaseDate = releaseDate;
        SetUpdatedAt();
    }

    public void Deactivate() { IsActive = false; SetUpdatedAt(); }
}
```

### 4.3 IMovieRepository

```
Domain/Repositories/IMovieRepository.cs
```

```csharp
public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Movie?> GetByIdWithShowingsAsync(Guid id, CancellationToken ct = default);
    Task<(IEnumerable<Movie> Items, int Total)> GetAllAsync(
        string? search, Guid? categoryId, bool? isActive,
        int page, int pageSize, CancellationToken ct = default);
    void Add(Movie movie);
    void Update(Movie movie);
}
```

---

## BƯỚC 5 – ENTITY: SHOWING

### 5.1 Entity

```
Domain/Entities/Showing.cs
```

```csharp
public enum ShowingStatus { Scheduled, Ongoing, Finished, Cancelled }

public class Showing : BaseEntity
{
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public ShowingStatus Status { get; private set; }
    public bool IsActive { get; private set; }
    public Guid MovieId { get; private set; }
    public Guid RoomId { get; private set; }

    public Movie Movie { get; private set; }
    public Room Room { get; private set; }
    public ICollection<BookingDetail> BookingDetails { get; private set; } = new List<BookingDetail>();

    private Showing() { }

    public static Showing Create(Guid movieId, Guid roomId, DateTime startTime, DateTime endTime)
        => new() { MovieId = movieId, RoomId = roomId,
                   StartTime = startTime, EndTime = endTime,
                   Status = ShowingStatus.Scheduled, IsActive = true };

    public void Update(DateTime startTime, Guid roomId)
    {
        StartTime = startTime; RoomId = roomId;
        SetUpdatedAt();
    }

    public void Cancel() { Status = ShowingStatus.Cancelled; IsActive = false; SetUpdatedAt(); }
}
```

### 5.2 IShowingRepository

```
Domain/Repositories/IShowingRepository.cs
```

```csharp
public interface IShowingRepository
{
    Task<Showing?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Showing?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default); // include Movie, Room, BookingDetails
    Task<(IEnumerable<Showing> Items, int Total)> GetAllAdminAsync(
        DateTime? dateStart, TimeSpan? hour, Guid? roomId, bool? isActive,
        int page, int pageSize, CancellationToken ct = default);
    Task<IEnumerable<Showing>> GetByMovieAndTheaterAsync(
        Guid movieId, Guid theaterId, CancellationToken ct = default);
    Task<IEnumerable<Guid>> GetBookedSeatIdsAsync(Guid showingId, CancellationToken ct = default);
    void Add(Showing showing);
    void Update(Showing showing);
}
```

---

## BƯỚC 6 – ENTITY: BOOKING + BOOKINGDETAIL

### 6.1 Entities

```
Domain/Entities/Booking.cs
Domain/Entities/BookingDetail.cs
```

```csharp
public enum BookingStatus { Pending, Paying, Confirmed, Cancelled, Expired }

public class Booking : BaseEntity
{
    public Guid UserId { get; private set; }
    public BookingStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? SessionId { get; private set; }   // Redis session key
    public DateTime? PaidAt { get; private set; }

    public ICollection<BookingDetail> Details { get; private set; } = new List<BookingDetail>();

    private Booking() { }

    public static Booking Create(Guid userId, string sessionId)
        => new() { UserId = userId, Status = BookingStatus.Pending, SessionId = sessionId };

    public void SetPaying(int extendedTtlSeconds)
    {
        Status = BookingStatus.Paying;
        SetUpdatedAt();
        // Signal để extend Redis TTL
    }

    public void Confirm(decimal totalAmount)
    {
        Status = BookingStatus.Confirmed;
        TotalAmount = totalAmount;
        PaidAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Cancel() { Status = BookingStatus.Cancelled; SetUpdatedAt(); }
    public void Expire()  { Status = BookingStatus.Expired;   SetUpdatedAt(); }
}

public class BookingDetail : BaseEntity
{
    public Guid BookingId { get; private set; }
    public Guid ShowingId { get; private set; }
    public Guid SeatId { get; private set; }
    public decimal Price { get; private set; }

    public Booking Booking { get; private set; }
    public Showing Showing { get; private set; }
    public Seat Seat { get; private set; }

    private BookingDetail() { }

    public static BookingDetail Create(Guid bookingId, Guid showingId, Guid seatId, decimal price)
        => new() { BookingId = bookingId, ShowingId = showingId, SeatId = seatId, Price = price };
}
```

### 6.2 IBookingRepository

```
Domain/Repositories/IBookingRepository.cs
```

```csharp
public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Booking?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> IsSeatBookedAsync(Guid showingId, Guid seatId, CancellationToken ct = default);
    void Add(Booking booking);
    void Update(Booking booking);
}
```

---

## BƯỚC 7 – UNIT OF WORK

```
Domain/Repositories/IUnitOfWork.cs
Persistence/UnitOfWork.cs
```

```csharp
// Domain
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// Persistence
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    public UnitOfWork(ApplicationDbContext context) => _context = context;
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
```

---

## BƯỚC 8 – MIGRATION

```bash
# Chạy lần lượt sau khi hoàn thành tất cả Entity + Configuration
dotnet ef migrations add InitialCreate --project Persistence --startup-project API
dotnet ef database update --project Persistence --startup-project API
```

**Thứ tự migration phải đảm bảo FK**:
1. Category
2. Movie (FK → Category)
3. Theater
4. Room (FK → Theater)
5. Seat (FK → Room)
6. Showing (FK → Movie, Room)
7. Booking (FK → User)
8. BookingDetail (FK → Booking, Showing, Seat)

---

## BƯỚC 9 – SEED DATA

```
Persistence/SeedData/DatabaseSeeder.cs
```

Dữ liệu seed tối thiểu:
- 3 Categories (Hành động, Tình cảm, Hoạt hình)
- 2 Theaters với địa chỉ thực
- Mỗi Theater có 2 Rooms
- Mỗi Room có layout 8×10 ghế (80 ghế)
- 5 Movies (2 đang chiếu, 3 sắp chiếu)
- Showing cho 7 ngày tới

---

## CHECKLIST REPOSITORY

- [ ] BaseEntity tạo xong
- [ ] Tất cả Entity viết xong (7 entity)
- [ ] Tất cả IRepository khai báo trong Domain (7 interface)
- [ ] Tất cả Fluent Configuration viết xong (7 file)
- [ ] Migration chạy thành công
- [ ] Tất cả Repository Implementation viết xong (7 implement)
- [ ] IUnitOfWork + UnitOfWork implement
- [ ] DI đăng ký đầy đủ trong `Persistence/DependencyInjection.cs`
- [ ] Seed data chạy thành công
