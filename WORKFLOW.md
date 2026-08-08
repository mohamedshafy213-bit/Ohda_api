# WORKFLOW — Adding a New Entity

This guide walks you through every file you need to create or modify when adding a new entity (e.g. `Product`) from scratch. Follow the steps in order.

---

## Overview

```
1. Entities    → add EF Core model + DbSet
2. Contracts   → add DTOs + repository interface
3. Repositories→ add concrete repository + register in wrapper
4. Service_API → add controller
5. Migration   → create & apply EF Core migration
```

---

## Step 1 — Create the Entity Model

**File:** `Entities/Models/Tables/Product.cs`

```csharp
using Entities.Models.BaseTables;

namespace Entities.Models.Tables;

public class Product : BaseTable
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}
```

> `BaseTable` already provides: `Id`, `InsertUserCode`, `InsertDate`, `UpdateUserCode`, `LastUpdate`, `IsDeleted`, `DeleteUserCode`, `DeleteDate`.

---

## Step 2 — Register the DbSet

**File:** `Entities/Models/Databases/RepositoryContext.cs`

Add a `DbSet` for your new entity:

```csharp
public DbSet<Product> Products { get; set; }
```

---

## Step 3 — Create the DTOs

### 3a. Read DTO

**File:** `Contracts/DTOs/Product/ProductDto.cs`

```csharp
using Contracts.BaseDtos;

namespace Contracts.DTOs.Product;

public class ProductDto : BaseDto   // BaseDto already has Id
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}
```

### 3b. Create DTO

**File:** `Contracts/DTOs/Product/ProductCreateDto.cs`

```csharp
using Contracts.BaseDtos;

namespace Contracts.DTOs.Product;

public class ProductCreateDto : BaseCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}
```

### 3c. Update DTO

**File:** `Contracts/DTOs/Product/ProductUpdateDto.cs`

```csharp
using Contracts.BaseDtos;

namespace Contracts.DTOs.Product;

public class ProductUpdateDto : BaseUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}
```

---

## Step 4 — Create the Repository Interface

**File:** `Contracts/Interfaces/Repository/IProductRepository.cs`

```csharp
using Contracts.DTOs.Product;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;

namespace Contracts.Interfaces.Repository;

public interface IProductRepository
    : IRepositoryBase<Product, ProductDto, ProductCreateDto, ProductUpdateDto>
{
    // Add any custom query methods here
}
```

---

## Step 5 — Implement the Repository

**File:** `Repositories/Models/ProductRepository.cs`

```csharp
using Contracts.DTOs.Product;
using Contracts.Interfaces.Repository;
using Entities.Models.Databases;
using Entities.Models.Tables;
using LoggerService;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Repositories.Repositories;

namespace Repositories.Models;

public class ProductRepository
    : RepositoryBase<Product, ProductDto, ProductCreateDto, ProductUpdateDto>,
      IProductRepository
{
    public ProductRepository(
        RepositoryContext repositoryContext,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        ILoggerManager logger)
        : base(repositoryContext, httpContextAccessor, mapper, logger)
    {
    }

    // Add any custom query method implementations here
}
```

---

## Step 6 — Register the Repository in the Wrapper

### 6a. Add to the Interface

**File:** `Contracts/Interfaces/Repository/IRepositoryWrapper.cs`

```csharp
using Contracts.Interfaces.Repository;

namespace Contracts.interfaces.Repository;

public interface IRepositoryWrapper
{
    IProductRepository Products { get; }   // ← add this line
    void Save();
}
```

### 6b. Add to the Implementation

**File:** `Repositories/Repositories/RepositoryWrapper.cs`

```csharp
private IProductRepository? _products;

public IProductRepository Products
{
    get
    {
        _products ??= new ProductRepository(
            _repoContext, _httpContextAccessor, _mapper, _logger);
        return _products;
    }
}
```

---

## Step 7 — Create the Controller

**File:** `Service_API/Controllers/ProductController.cs`

```csharp
using Contracts.DTOs.Product;
using Contracts.interfaces.Repository;
using Entities.Models.Tables;
using Microsoft.AspNetCore.Mvc;

namespace Service_API.Controllers;

public class ProductController
    : BaseController<Product, ProductDto, ProductCreateDto, ProductUpdateDto>
{
    public ProductController(IRepositoryWrapper repositoryWrapper)
    {
        _repository = repositoryWrapper.Products;
    }
}
```

This gives you `GET /api/product`, `POST /api/product`, `PUT /api/product`, `DELETE /api/product/{id}` for free.

> To override or extend an endpoint, simply override the corresponding virtual method from `BaseController`.

---

## Step 8 — Create & Apply the Migration

```bash
# Run from the solution root
cd Entities
dotnet ef migrations add Add_Product --startup-project ../Service_API
dotnet ef database update          --startup-project ../Service_API
cd ..
```

---

## Step 9 — Run & Verify

```bash
dotnet run --project Service_API
```

Open Swagger at `https://localhost:8152/swagger` and confirm the `/api/product` endpoints appear and work correctly.

---

## Checklist

| #   | Task                          | File(s)                                                 |
| --- | ----------------------------- | ------------------------------------------------------- |
| 1   | Entity model                  | `Entities/Models/Tables/Product.cs`                     |
| 2   | DbSet registration            | `Entities/Models/Databases/RepositoryContext.cs`        |
| 3   | DTOs (read / create / update) | `Contracts/DTOs/Product/Product*Dto.cs`                 |
| 4   | Repository interface          | `Contracts/Interfaces/Repository/IProductRepository.cs` |
| 5   | Repository implementation     | `Repositories/Models/ProductRepository.cs`              |
| 6   | Wrapper interface + class     | `IRepositoryWrapper.cs`, `RepositoryWrapper.cs`         |
| 7   | Controller                    | `Service_API/Controllers/ProductController.cs`          |
| 8   | EF Core migration             | `dotnet ef migrations add ...`                          |
| 9   | Verify in Swagger             | —                                                       |
