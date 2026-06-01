# Movie Owner – Project Structure Template (.NET 8 / Clean Architecture)

Repo: `dongnuc/movie-owner`

Tài liệu này mô tả **cấu trúc dự án** trong repository và **quy tắc tổ chức / khai báo** để bạn có thể tái sử dụng cho các dự án khác.

---

## 1) Tổng quan kiến trúc

Repository đang tổ chức theo hướng **Clean Architecture / Layered Architecture** tách thành nhiều project (mỗi layer là 1 `.csproj`) và liên kết bằng `ProjectReference`.

Các layer chính:

- **API (Host)**: entrypoint chạy ASP.NET Core, cấu hình DI, middleware, hosting.
- **Presentation**: nơi đặt các API endpoints/routes (ví dụ dùng Carter), contracts liên quan request/response ở tầng trình bày.
- **Application**: use-cases (commands/queries), validation, mapping, pipeline behaviors (MediatR).
- **Domain**: entity, domain exception, domain abstraction (pure business rules).
- **Contract**: các interface/contract dùng chung (ví dụ email abstraction), tránh dependency ngược.
- **Persistence**: EF Core DbContext, migrations, repositories triển khai.
- **Infrastructure**: các tích hợp kỹ thuật (JWT, Redis caching, external services), wiring dịch vụ hạ tầng.

Ngoài ra repo có **Dockerfile** và **docker-compose** để chạy stack (API + SQL Server + Redis + Client).

---

## 2) Cấu trúc thư mục (root)

```text
.
├─ Movie_StructureCode.sln
├─ Movie_StructrueCode.API/                 # ASP.NET Core host
├─ Movie_StructureCode.Presentation/         # Endpoints/API layer
├─ Movie_StructureCode.Application/          # Use cases, CQRS, validation, mapping
├─ Movie_StructureCode.Domain/               # Domain model
├─ Movie_StructureCode.Contract/             # Shared contracts/abstractions
├─ Movie_StructureCode.Persistence/          # EF Core, DB, migrations
├─ Movie_StructureCode.Infracstructure/      # Infrastructure integrations (Redis, JWT, ...)
├─ Dockerfile
├─ docker-compose.yml
├─ docker-compose.dev.yml
├─ .dockerignore
├─ .gitignore
└─ MovieDbOwner.bak                          # SQL backup for local/dev
```

> Lưu ý: trong tên project có một số typo như `Structrue` và `Infracstructure`. Nếu bạn dùng làm template cho dự án mới, nên chuẩn hoá thành `Structure` và `Infrastructure` để tránh gây nhầm lẫn.

---

## 3) Phân rã từng project (layer) và nhiệm vụ

### 3.1. `Movie_StructrueCode.API` (Host)
**Mục tiêu:** Là project chạy thật (`Microsoft.NET.Sdk.Web`). Nơi cấu hình pipeline và bootstrapping.

**Bên trong (theo repo):**

```text
Movie_StructrueCode.API/
├─ Program.cs
├─ DependencyInjection/
├─ Middleware/
├─ Hubs/
├─ Services/
└─ appsettings*.json
```

**Tham chiếu project:**
- `Application`
- `Domain`
- `Persistence`
- `Presentation`
- `Infracstructure`

**Quy tắc:**
- API **không chứa business logic** (không viết use-case ở đây).
- API chỉ:
  - cấu hình DI (gọi các extension `AddXyz(...)` của các layer)
  - cấu hình middleware
  - cấu hình swagger/versioning/logging
  - host endpoints từ `Presentation`

### 3.2. `Movie_StructureCode.Presentation`
**Mục tiêu:** Chứa phần "presentation" thuần: routes/endpoints, request/response DTO dành cho HTTP.

**Bên trong (theo repo):**

```text
Movie_StructureCode.Presentation/
├─ APIs/
├─ Abstractions/
└─ AssemblyReference.cs
```

**Quy tắc:**
- Presentation phụ thuộc `Application`, `Domain`, `Contract`.
- Tại đây:
  - định nghĩa endpoint (ví dụ Carter modules)
  - map request -> command/query của `Application`
  - trả về response DTO

### 3.3. `Movie_StructureCode.Application`
**Mục tiêu:** Application layer triển khai **use-cases** theo CQRS (MediatR).

**Bên trong (theo repo):**

```text
Movie_StructureCode.Application/
├─ Abstractions/                 # interfaces cho service/repository/caching...
├─ Behaviors/                    # MediatR pipeline behaviors
├─ Common/                       # helpers, base result, constants...
├─ DependencyInjection/          # extension methods AddApplication(...)
├─ Exceptions/
├─ Features/                     # UseCases theo module
├─ Mappers/                      # AutoMapper profiles
└─ Services/                     # application services (orchestrations)
```

**Quy tắc:**
- Application chỉ phụ thuộc: `Domain` + `Contract` (không phụ thuộc Persistence/Infrastructure/API).
- Nên tổ chức **Feature-first**:
  - `Features/<Module>/Commands/...`
  - `Features/<Module>/Queries/...`
- Use-case nên dùng MediatR:
  - `Command : IRequest<Result<T>>`
  - `CommandHandler : IRequestHandler<Command, Result<T>>`

### 3.4. `Movie_StructureCode.Domain`
**Mục tiêu:** Domain là lõi nghiệp vụ, càng ít phụ thuộc bên ngoài càng tốt.

**Bên trong (theo repo):**

```text
Movie_StructureCode.Domain/
├─ Entities/
├─ Exceptions/
├─ Repositories/
└─ Abstractions/
```

**Quy tắc:**
- Domain không phụ thuộc Application/Infrastructure/Persistence.
- Chứa:
  - Entities, ValueObjects (nếu có)
  - Domain events (nếu có)
  - Domain exceptions
  - Domain interfaces (ví dụ repository abstractions) nếu bạn chọn để ở domain

### 3.5. `Movie_StructureCode.Contract`
**Mục tiêu:** Tập hợp các contract dùng chung giữa các layer (đặc biệt khi muốn tránh reference vòng).

**Bên trong (theo repo):**

```text
Movie_StructureCode.Contract/
├─ Abstractions/
├─ Emails/
└─ AssemblyReference.cs
```

**Quy tắc:**
- Contract chứa interface/DTO mang tính "shared".
- Các layer có thể phụ thuộc vào Contract mà không tạo coupling mạnh.

### 3.6. `Movie_StructureCode.Persistence`
**Mục tiêu:** Data access (EF Core). Đây là nơi "implement" repository + DbContext.

**Bên trong (theo repo):**

```text
Movie_StructureCode.Persistence/
├─ Context/
├─ Configurations/
├─ DependencyInjection/
├─ Migrations/
├─ Repositories/
├─ SeedData/
└─ AssemblyReference.cs
```

**Quy tắc:**
- Persistence phụ thuộc `Domain`.
- Chứa:
  - `DbContext`
  - EF Core configuration (fluent config)
  - migrations
  - repository implementations
- Đăng ký DI thông qua `DependencyInjection` (extension method).

### 3.7. `Movie_StructureCode.Infracstructure` (Infrastructure)
**Mục tiêu:** Các tích hợp kỹ thuật / external services: JWT auth, Redis cache, email provider, ...

**Bên trong (theo repo):**

```text
Movie_StructureCode.Infracstructure/
├─ Caching/
├─ Services/
├─ DependencyInjection/
└─ AssemblyReference.cs
```

**Quy tắc:**
- Infrastructure có thể phụ thuộc `Application` (để implement các abstraction) và `Persistence` (nếu cần).
- Infrastructure **không được kéo Presentation/API**.

---

## 4) Quy tắc dependency (rất quan trọng để tái sử dụng)

Khuyến nghị dependency theo hướng **inward** (từ ngoài vào trong):

```text
API (Host)
  -> Presentation
  -> Infrastructure
  -> Persistence
  -> Application
  -> Domain
  -> Contract
```

Quy tắc cứng nên áp dụng:

1. **Domain** không tham chiếu project nào khác (hoặc chỉ tham chiếu `Contract` nếu cần chia sẻ abstractions).
2. **Application** chỉ tham chiếu `Domain` + `Contract`.
3. **Persistence** tham chiếu `Domain` (để map entity) và implement repository.
4. **Infrastructure** implement các abstraction do `Application`/`Contract` định nghĩa.
5. **Presentation** gọi `Application` (MediatR) để thực thi use-case.
6. **API** là composition root, tham chiếu tất cả layer để wire DI.

---

## 5) Quy tắc đặt tên (nên chuẩn hoá khi dùng lại)

### 5.1. Solution & projects
- Solution: `<Product>.sln`
- Projects:
  - `<Product>.API`
  - `<Product>.Presentation`
  - `<Product>.Application`
  - `<Product>.Domain`
  - `<Product>.Contracts` (hoặc `Contract`)
  - `<Product>.Persistence`
  - `<Product>.Infrastructure`

### 5.2. Namespaces
- Namespace root trùng với tên project.
- Ví dụ:
  - `Movie.StructureCode.Application.Features.Movies.Commands.CreateMovie`

### 5.3. Folder conventions
- `DependencyInjection/` trong mỗi layer để chứa `ServiceCollectionExtensions`.
- `AssemblyReference.cs` để expose assembly cho scan (AutoMapper, MediatR, Carter, FluentValidation...).

---

## 6) Quy tắc khai báo DI (pattern tái sử dụng)

Mỗi layer nên có 1 entrypoint DI rõ ràng:

- `Movie_StructureCode.Application.DependencyInjection`:
  - `public static IServiceCollection AddApplication(this IServiceCollection services)`
  - đăng ký MediatR, FluentValidation, AutoMapper, pipeline behaviors.

- `Movie_StructureCode.Persistence.DependencyInjection`:
  - `AddPersistence(...)` đăng ký DbContext, repositories.

- `Movie_StructureCode.Infracstructure.DependencyInjection`:
  - `AddInfrastructure(...)` đăng ký Redis cache, JWT, external services.

- `Movie_StructureCode.Presentation`:
  - `AddPresentation(...)` đăng ký Carter modules / API versioning ở mức presentation nếu cần.

**Tại API (Program.cs)** chỉ gọi các hàm `AddXxx(...)` này.

---

## 7) Quy tắc cho CQRS (Application layer)

Khuyến nghị chuẩn hoá cấu trúc mỗi use-case:

```text
Features/
  <Module>/
    Commands/
      <Action>/
        <Action>Command.cs
        <Action>CommandHandler.cs
        <Action>Validator.cs
    Queries/
      <Action>/
        <Action>Query.cs
        <Action>QueryHandler.cs
```

- Command/Query chỉ mang dữ liệu.
- Handler phụ trách orchestration (gọi repository/service).
- Validator dùng FluentValidation.
- Mapping dùng AutoMapper profile trong `Mappers/`.

---

## 8) Docker & môi trường chạy

Repo có:

- `Dockerfile`: build/publish .NET 8 và chạy expose `8080`.
- `docker-compose.yml`: chạy image đã build/push sẵn cho `movie-api`, đồng thời chạy `movie-sql` và `movie-redis`.
- `docker-compose.dev.yml`: build local `movie-api` từ `Dockerfile`.

Quy tắc khuyến nghị khi tái sử dụng:
- `ASPNETCORE_ENVIRONMENT` cho profile config (`appsettings.Docker.json`, `appsettings.Development.json`).
- Dùng `docker-compose.dev.yml` cho dev (build local), `docker-compose.yml` cho deploy/test (pull image).

---

## 9) Checklist để dùng cấu trúc này cho dự án khác

1. Đổi tên solution + project theo sản phẩm mới.
2. Chuẩn hoá lại các typo: `Structrue` -> `Structure`, `Infracstructure` -> `Infrastructure`.
3. Đảm bảo dependency đúng (mục 4).
4. Mỗi layer có `DependencyInjection` và chỉ expose 1–2 extension method.
5. Feature-first trong Application để dễ scale.
6. Không để business logic ở API/Presentation.

---

## 10) Gợi ý file placement (khi bạn phát triển thêm)

- Thêm endpoint HTTP: `Presentation/APIs/...`
- Thêm use-case mới: `Application/Features/...`
- Thêm entity/domain rule: `Domain/Entities` hoặc `Domain/Abstractions`
- Thêm repository EF: `Persistence/Repositories`
- Thêm integration (Redis/JWT/Email): `Infrastructure/Services` hoặc `Infrastructure/Caching`

---

> Nếu bạn muốn, mình có thể cập nhật trực tiếp README.md của repo để tài liệu này hiển thị ngay tại root (hoặc tạo file `docs/PROJECT_STRUCTURE.md`).