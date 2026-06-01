# Movie Owner System - API Specification

This document contains the detailed request and response formats for all backend APIs grouped by Role, Entity, and API endpoint.

## Table of Contents
- [System Admin](#system-admin)
  - [System Categories](#system-admin-system-categories)
  - [System Movies](#system-admin-system-movies)
  - [System SeatTypes](#system-admin-system-seattypes)
  - [System Theaters](#system-admin-system-theaters)
  - [Theater Rooms](#system-admin-theater-rooms)
  - [Theater Seats](#system-admin-theater-seats)
  - [Theater Showings](#system-admin-theater-showings)
- [User/Guest](#userguest)
  - [](#userguest-)
  - [Auth](#userguest-auth)
  - [Movies](#userguest-movies)
  - [Showings](#userguest-showings)

---

## <a name="system-admin"></a>🔑 System Admin APIs
### <a name="system-admin-system-categories"></a>📦 System Categories
#### `GET` /api/v1/admin/categories
**Summary:** Lấy danh sách thể loại phim (phân trang, lọc theo ID)

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | query | No | `string(uuid)` | - |
| `page` | query | No | `integer(int32)` | - |
| `pageSize` | query | No | `integer(int32)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Contract.Abstractions.Shared.[PagedResult](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Contract/Abstractions/Shared/PagedResult.cs)`1[Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common.[CategoryResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Categories/Common/CategoryResponse.cs)]
  Structure: { items: Array<Ref([CategoryResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Categories/Common/CategoryResponse.cs))>, pageIndex: integer(int32), pageSize: integer(int32), totalCount: integer(int32), hasNextPage: boolean, hasPrevious: boolean }
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `POST` /api/v1/admin/categories
**Summary:** Tạo thể loại phim mới

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.System.[CreateCategoryRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/System/CategorySystemApi.cs)
Structure: { name: string, isActive: boolean }
```

**Responses:**
- **`200`** (OK)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `GET` /api/v1/admin/categories/{id}
**Summary:** Lấy thông tin thể loại phim theo ID

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Application.Features.UseCase.Admin.Categories.Common.[CategoryResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Categories/Common/CategoryResponse.cs)
  Structure: { id: string(uuid), name: string, isActive: boolean, dateCreate: string(date-time), dateUpdate: string(date-time) }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `PUT` /api/v1/admin/categories/{id}
**Summary:** Cập nhật thông tin thể loại phim

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.System.[UpdateCategoryRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/System/CategorySystemApi.cs)
Structure: { name: string, isActive: boolean }
```

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `DELETE` /api/v1/admin/categories/{id}
**Summary:** Xóa thể loại phim theo ID

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---


### <a name="system-admin-system-movies"></a>📦 System Movies
#### `GET` /api/v1/admin/movies
**Summary:** Lấy danh sách phim (phân trang, tìm kiếm, lọc)

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `search` | query | No | `string` | - |
| `categoryId` | query | No | `string(uuid)` | - |
| `isActive` | query | No | `boolean` | - |
| `page` | query | No | `integer(int32)` | - |
| `pageSize` | query | No | `integer(int32)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Contract.Abstractions.Shared.[PagedResult](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Contract/Abstractions/Shared/PagedResult.cs)`1[Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common.[MovieResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Movies/Common/MovieResponse.cs)]
  Structure: { items: Array<Ref([MovieResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Movies/Common/MovieResponse.cs))>, pageIndex: integer(int32), pageSize: integer(int32), totalCount: integer(int32), hasNextPage: boolean, hasPrevious: boolean }
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `POST` /api/v1/admin/movies
**Summary:** Tạo phim mới

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.System.[CreateMovieRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/System/MovieSystemApi.cs)
Structure: { title: string, description: string, image: string, basePrice: number(double), duration: integer(int32), categoryId: string(uuid), isActive: boolean }
```

**Responses:**
- **`200`** (OK)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `GET` /api/v1/admin/movies/{id}
**Summary:** Lấy thông tin chi tiết phim theo ID

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Application.Features.UseCase.Admin.Movies.Common.[MovieDetailResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Movies/Common/MovieDetailResponse.cs)
  Structure: { id: string(uuid), title: string, description: string, image: string, basePrice: number(double), duration: integer(int32), categoryId: string(uuid), categoryName: string, isActive: boolean, dateCreate: string(date-time), dateUpdate: string(date-time) }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `PUT` /api/v1/admin/movies/{id}
**Summary:** Cập nhật thông tin phim

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.System.[UpdateMovieRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/System/MovieSystemApi.cs)
Structure: { title: string, description: string, image: string, basePrice: number(double), duration: integer(int32), categoryId: string(uuid), isActive: boolean }
```

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `DELETE` /api/v1/admin/movies/{id}
**Summary:** Xóa phim theo ID

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---


### <a name="system-admin-system-seattypes"></a>📦 System SeatTypes
#### `GET` /api/v1/admin/seattypes/active
**Summary:** Lấy danh sách loại ghế đang hoạt động (chỉ gồm ID, tên, phụ phí)

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: 
  Structure: Array<{ id: string(uuid), name: string, surcharge: number(double) }>
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `GET` /api/v1/admin/seattypes
**Summary:** Lấy danh sách tất cả loại ghế (phân trang, lọc theo ID)

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | query | No | `string(uuid)` | - |
| `page` | query | No | `integer(int32)` | - |
| `pageSize` | query | No | `integer(int32)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Contract.Abstractions.Shared.[PagedResult](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Contract/Abstractions/Shared/PagedResult.cs)`1[Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common.[SeatTypeResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/SeatTypes/Common/SeatTypeResponse.cs)]
  Structure: { items: Array<Ref([SeatTypeResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/SeatTypes/Common/SeatTypeResponse.cs))>, pageIndex: integer(int32), pageSize: integer(int32), totalCount: integer(int32), hasNextPage: boolean, hasPrevious: boolean }
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `POST` /api/v1/admin/seattypes
**Summary:** Tạo loại ghế mới

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.System.[CreateSeatTypeRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/System/SeatTypeSystemApi.cs)
Structure: { name: string, surcharge: number(double), isActive: boolean }
```

**Responses:**
- **`200`** (OK)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `GET` /api/v1/admin/seattypes/{id}
**Summary:** Lấy thông tin loại ghế theo ID

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Application.Features.UseCase.Admin.SeatTypes.Common.[SeatTypeResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/SeatTypes/Common/SeatTypeResponse.cs)
  Structure: { id: string(uuid), name: string, surcharge: number(double), isActive: boolean, dateCreate: string(date-time), dateUpdate: string(date-time) }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `PUT` /api/v1/admin/seattypes/{id}
**Summary:** Cập nhật thông tin loại ghế

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.System.[UpdateSeatTypeRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/System/SeatTypeSystemApi.cs)
Structure: { name: string, surcharge: number(double), isActive: boolean }
```

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `DELETE` /api/v1/admin/seattypes/{id}
**Summary:** Xóa loại ghế theo ID

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---


### <a name="system-admin-system-theaters"></a>📦 System Theaters
#### `GET` /api/v1/admin/theaters
**Summary:** Lấy danh sách rạp chiếu phim (phân trang, tìm kiếm, lọc)

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `search` | query | No | `string` | - |
| `isActive` | query | No | `boolean` | - |
| `page` | query | No | `integer(int32)` | - |
| `pageSize` | query | No | `integer(int32)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Contract.Abstractions.Shared.[PagedResult](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Contract/Abstractions/Shared/PagedResult.cs)`1[Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common.[TheaterResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Theaters/Common/TheaterResponse.cs)]
  Structure: { items: Array<Ref([TheaterResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Theaters/Common/TheaterResponse.cs))>, pageIndex: integer(int32), pageSize: integer(int32), totalCount: integer(int32), hasNextPage: boolean, hasPrevious: boolean }
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `POST` /api/v1/admin/theaters
**Summary:** Tạo rạp chiếu phim mới

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.System.[CreateTheaterRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/System/TheaterSystemApi.cs)
Structure: { name: string, location: string, isActive: boolean }
```

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: 
  Structure: string
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `GET` /api/v1/admin/theaters/{id}
**Summary:** Lấy thông tin rạp chiếu phim theo ID

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Application.Features.UseCase.Admin.Theaters.Common.[TheaterResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Theaters/Common/TheaterResponse.cs)
  Structure: { id: string(uuid), name: string, location: string, isActive: boolean, dateCreate: string(date-time), dateUpdate: string(date-time) }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `PUT` /api/v1/admin/theaters/{id}
**Summary:** Cập nhật thông tin rạp chiếu phim

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.System.[UpdateTheaterRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/System/TheaterSystemApi.cs)
Structure: { name: string, location: string, isActive: boolean }
```

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `DELETE` /api/v1/admin/theaters/{id}
**Summary:** Xóa rạp chiếu phim theo ID

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---


### <a name="system-admin-theater-rooms"></a>📦 Theater Rooms
#### `GET` /api/v1/admin/rooms
**Summary:** Lấy danh sách phòng chiếu (phân trang, lọc theo rạp)

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `theaterId` | query | Yes | `string(uuid)` | - |
| `search` | query | No | `string` | - |
| `isActive` | query | No | `boolean` | - |
| `page` | query | No | `integer(int32)` | - |
| `pageSize` | query | No | `integer(int32)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Contract.Abstractions.Shared.[PagedResult](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Contract/Abstractions/Shared/PagedResult.cs)`1[Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common.[RoomResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Rooms/Common/RoomResponse.cs)]
  Structure: { items: Array<Ref([RoomResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Rooms/Common/RoomResponse.cs))>, pageIndex: integer(int32), pageSize: integer(int32), totalCount: integer(int32), hasNextPage: boolean, hasPrevious: boolean }
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `POST` /api/v1/admin/rooms
**Summary:** Tạo phòng chiếu mới

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.[Theater](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Domain/Entities/Theater.cs).[CreateRoomRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/Theater/RoomTheaterApi.cs)
Structure: { name: string, totalRow: integer(int32), totalCol: integer(int32), theaterId: string(uuid), isActive: boolean }
```

**Responses:**
- **`200`** (OK)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `GET` /api/v1/admin/rooms/{id}
**Summary:** Lấy thông tin chi tiết phòng chiếu theo ID

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common.[RoomDetailResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Rooms/Common/RoomDetailResponse.cs)
  Structure: { id: string(uuid), name: string, totalSeat: integer(int32), totalRow: integer(int32), totalCol: integer(int32), isActive: boolean, dateCreate: string(date-time), dateUpdate: string(date-time) }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `PUT` /api/v1/admin/rooms/{id}
**Summary:** Cập nhật thông tin phòng chiếu

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.[Theater](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Domain/Entities/Theater.cs).[UpdateRoomRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/Theater/RoomTheaterApi.cs)
Structure: { name: string, totalRow: integer(int32), totalCol: integer(int32), theaterId: string(uuid), isActive: boolean }
```

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `DELETE` /api/v1/admin/rooms/{id}
**Summary:** Xóa phòng chiếu theo ID

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `GET` /api/v1/admin/rooms/{id}/layout
**Summary:** Lấy sơ đồ chỗ ngồi của phòng chiếu theo ID

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Application.Features.UseCase.Admin.Rooms.Common.[RoomLayoutResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Rooms/Common/RoomLayoutResponse.cs)
  Structure: { id: string(uuid), name: string, totalSeat: integer(int32), totalRow: integer(int32), totalCol: integer(int32), isActive: boolean, seatRows: Array<Ref([SeatRowLayoutResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Rooms/Common/RoomLayoutResponse.cs))> }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `GET` /api/v1/admin/rooms/theater/{theaterId}
**Summary:** Lấy danh sách phòng chiếu thuộc rạp (Id, Name, Matrix, isActive = true)

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `theaterId` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: 
  Structure: Array<{ id: string(uuid), name: string, matrix: string }>
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---


### <a name="system-admin-theater-seats"></a>📦 Theater Seats
#### `POST` /api/v1/admin/seats/bulk
**Summary:** Tạo sơ đồ ghế hàng loạt tự động cho phòng chiếu

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.[Theater](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Domain/Entities/Theater.cs).[CreateSeatsInBulkRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/Theater/SeatTheaterApi.cs)
Structure: { roomId: string(uuid) }
```

**Responses:**
- **`200`** (OK)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `PUT` /api/v1/admin/seats/seattype/bulk
**Summary:** Cập nhật loại ghế hàng loạt cho danh sách ghế

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.[Theater](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Domain/Entities/Theater.cs).[UpdateSeatsTypeInBulkRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/Theater/SeatTheaterApi.cs)
Structure: { seatIds: Array<string(uuid)>, seatTypeId: string(uuid) }
```

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `PUT` /api/v1/admin/seats/seattype/couple
**Summary:** Cập nhật các ghế được chọn thành ghế Couple (yêu cầu số lượng chẵn, cùng hàng và liền kề nhau)

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.[Theater](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Domain/Entities/Theater.cs).[UpdateSeatsToCoupleRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/Theater/SeatTheaterApi.cs)
Structure: { seatIds: Array<string(uuid)> }
```

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---


### <a name="system-admin-theater-showings"></a>📦 Theater Showings
#### `GET` /api/v1/admin/showings/theater/{theaterId}
**Summary:** Lấy toàn bộ lịch chiếu thuộc rạp (gom theo movie, phân trang)

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `theaterId` | path | Yes | `string(uuid)` | - |
| `dateStart` | query | No | `string` | - |
| `pageIndex` | query | No | `integer(int32)` | - |
| `pageSize` | query | No | `integer(int32)` | - |
| `isActive` | query | No | `boolean` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Contract.Abstractions.Shared.[PagedResult](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Contract/Abstractions/Shared/PagedResult.cs)`1[Movie_StructureCode.Contract.Abstractions.Shared.[ShowingGroupedByMovie](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Contract/Abstractions/Shared/ShowingGroupedByMovie.cs)]
  Structure: { items: Array<Ref([ShowingGroupedByMovie](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Contract/Abstractions/Shared/ShowingGroupedByMovie.cs))>, pageIndex: integer(int32), pageSize: integer(int32), totalCount: integer(int32), hasNextPage: boolean, hasPrevious: boolean }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `GET` /api/v1/admin/showings/{id}
**Summary:** Lấy thông tin chi tiết lịch chiếu theo ID

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Application.Features.UseCase.Admin.Showings.Common.[ShowingDetailResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/Admin/Showings/Common/ShowingDetailResponse.cs)
  Structure: { id: string(uuid), timeStart: string(date-time), price: number(double), maxTicketsPerUser: integer(int32), movieId: string(uuid), movieTitle: string, movieDuration: integer(int32), roomId: string(uuid), roomName: string, theaterId: string(uuid), isActive: boolean, dateCreate: string(date-time), dateUpdate: string(date-time) }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `PUT` /api/v1/admin/showings/{id}
**Summary:** Cập nhật thông tin lịch chiếu

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.[Theater](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Domain/Entities/Theater.cs).[UpdateShowingRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/Theater/ShowingTheaterApi.cs)
Structure: { timeStart: string(date-time), price: number(double), maxTicketsPerUser: integer(int32), roomId: string(uuid), isActive: boolean }
```

**Responses:**
- **`204`** (No Content)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `POST` /api/v1/admin/showings
**Summary:** Tạo lịch chiếu mới

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Admin.[Theater](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Domain/Entities/Theater.cs).[CreateShowingRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Admin/Theater/ShowingTheaterApi.cs)
Structure: { timeStart: string(date-time), price: number(double), maxTicketsPerUser: integer(int32), movieId: string(uuid), roomId: string(uuid), isActive: boolean }
```

**Responses:**
- **`200`** (OK)
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`403`** (Forbidden)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---


## <a name="userguest"></a>🔑 User/Guest APIs
### <a name="userguest-"></a>📦 
#### `GET` /

**Responses:**
- **`200`** (OK)

---


### <a name="userguest-auth"></a>📦 Auth
#### `POST` /api/v1/auth/register
**Summary:** Đăng ký tài khoản mới (username, email, password, re-password)

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Auth.[RegisterRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Auth/AuthUserApi.cs)
Structure: { username: string, email: string, password: string, confirmPassword: string }
```

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: 
  Structure: string
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `POST` /api/v1/auth/confirm-email
**Summary:** Xác minh email tài khoản

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `userId` | query | Yes | `string(uuid)` | - |
| `token` | query | Yes | `string` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: 
  Structure: string
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `POST` /api/v1/auth/resend-confirm-email
**Summary:** Gửi lại email xác thực

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Auth.[ResendConfirmEmailRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Auth/AuthUserApi.cs)
Structure: { email: string }
```

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: 
  Structure: string
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `POST` /api/v1/auth/login
**Summary:** Đăng nhập (username hoặc email + password)

**Request Body:**
```yaml
Type: application/json
Schema Name: Movie_StructureCode.Presentation.APIs.Auth.[LoginRequest](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Presentation/APIs/Auth/AuthUserApi.cs)
Structure: { usernameOrEmail: string, password: string }
```

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Application.Features.UseCases.Queries.Auth.[Login](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCases/Queries/Auth/Login/Login.cs).[LoginResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCases/Queries/Auth/Login/Login.cs)
  Structure: { accessToken: string, accessTokenExpiresAt: string(date-time), refreshToken: string }
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `POST` /api/v1/auth/refresh
**Summary:** Cấp access token mới từ refresh token

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Application.Features.UseCases.Commands.Auth.[RefreshAccessToken](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCases/Commands/Auth/RefreshAccessToken/RefreshAccessToken.cs).[RefreshAccessTokenResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCases/Commands/Auth/RefreshAccessToken/RefreshAccessToken.cs)
  Structure: { refreshTokenNew: string, refreshTokenExpires: string(date-time), accessToken: string, accessTokenExpiresAt: string(date-time) }
  ```
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---
#### `POST` /api/v1/auth/logout
**Summary:** Đăng xuất (xóa refresh token và session khỏi Redis)

**Responses:**
- **`204`** (No Content)
- **`401`** (Unauthorized)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```
- **`400`** (Bad Request)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---


### <a name="userguest-movies"></a>📦 Movies
#### `GET` /api/v1/movies
**Summary:** Lấy danh sách phim (chỉ phim active)

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `search` | query | No | `string` | - |
| `categoryId` | query | No | `string(uuid)` | - |
| `page` | query | No | `integer(int32)` | - |
| `pageSize` | query | No | `integer(int32)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Contract.Abstractions.Shared.[PagedResult](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Contract/Abstractions/Shared/PagedResult.cs)`1[Movie_StructureCode.Application.Features.UseCase.User.Movies.Common.[MovieUserResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/User/Movies/Common/MovieUserResponse.cs)]
  Structure: { items: Array<Ref([MovieUserResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/User/Movies/Common/MovieUserResponse.cs))>, pageIndex: integer(int32), pageSize: integer(int32), totalCount: integer(int32), hasNextPage: boolean, hasPrevious: boolean }
  ```

---
#### `GET` /api/v1/movies/{id}
**Summary:** Lấy chi tiết phim kèm danh sách rạp + suất chiếu hôm nay

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `id` | path | Yes | `string(uuid)` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: Movie_StructureCode.Application.Features.UseCase.User.Movies.Common.[MovieDetailUserResponse](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/User/Movies/Common/MovieDetailUserResponse.cs)
  Structure: { id: string(uuid), title: string, description: string, image: string, basePrice: number(double), duration: integer(int32), categoryId: string(uuid), categoryName: string, theaters: Array<Ref([TheaterShowingGroup](file:///d:/CSharp_Project/Project/Movie_Project/code/backend/StructureCode_Clean_Architecture/Movie_StructureCode.Application/Features/UseCase/User/Movies/Common/MovieDetailUserResponse.cs))> }
  ```
- **`404`** (Not Found)
  ```yaml
  Type: application/problem+json
  Schema Name: Microsoft.AspNetCore.Mvc.ProblemDetails
  Structure: { type: string, title: string, status: integer(int32), detail: string, instance: string }
  ```

---


### <a name="userguest-showings"></a>📦 Showings
#### `GET` /api/v1/showings
**Summary:** Lấy danh sách suất chiếu theo phim, rạp và ngày

**Request Parameters:**
| Name | In | Required | Type | Description |
| --- | --- | --- | --- | --- |
| `movieId` | query | Yes | `string(uuid)` | - |
| `theaterId` | query | Yes | `string(uuid)` | - |
| `date` | query | Yes | `string` | - |

**Responses:**
- **`200`** (OK)
  ```yaml
  Type: application/json
  Schema Name: 
  Structure: Array<{ id: string(uuid), timeStart: string(date-time), price: number(double), roomId: string(uuid) }>
  ```

---

