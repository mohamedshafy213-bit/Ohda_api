# .NET 9 Clean Architecture API Template

A production-ready starter template for building enterprise ASP.NET Core REST APIs. Derived from a real SIS project, this template provides a battle-tested architectural skeleton — auth, logging, EF Core, DTO mapping, and repository pattern — so you can focus on building features instead of plumbing.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 9.0 |
| ORM | Entity Framework Core 9 |
| Database | PostgreSQL (default) or Oracle |
| DTO Mapping | Mapster 7 |
| Authentication | Keycloak (JWT Bearer / OpenID Connect) |
| Logging | Serilog with database sink |
| File Storage | MinIO (S3-compatible, optional) |
| API Docs | Swagger / OpenAPI (Swashbuckle) |

---

## Project Structure

```
YourProject/
├── Service_API/                   # ASP.NET Core Web API entry point
│   ├── Program.cs                 # App bootstrap & DI registrations
│   ├── Extensions/
│   │   └── ServiceExtensions.cs  # IServiceCollection extension methods
│   ├── BaseControllers/
│   │   └── BaseController.cs     # Generic CRUD base controller
│   ├── Attributes/
│   │   └── AuthorizeGroupAttribute.cs
│   ├── Controllers/               # [EMPTY] Add your controllers here
│   ├── Services/                  # [EMPTY] Add app-layer services here
│   └── appsettings.json           # Configuration (fill in your values)
│
├── Entities/                      # EF Core domain models & DbContext
│   └── Models/
│       ├── BaseTables/
│       │   ├── BaseTable.cs       # Common audit + soft-delete fields
│       │   └── BaseLookup.cs      # Lightweight lookup base
│       ├── Interfaces/            # ISoftDelete, ITenantEntity, ITenantService
│       ├── Audit/AuditEntry.cs    # Change tracking helper
│       ├── Services/TenantService.cs
│       ├── Databases/
│       │   ├── RepositoryContext.cs   # Abstract DbContext base
│       │   ├── PostgresDb/PostgresContext.cs
│       │   └── OracleDb/OracleContext.cs
│       └── Tables/                # [EMPTY] Add your entity models here
│           └── AuditLog.cs        # System audit log (pre-included)
│
├── Repositories/                  # Data access layer
│   ├── Repositories/
│   │   ├── Repositories.cs        # Generic RepositoryBase<T,TDto,...>
│   │   └── RepositoryWrapper.cs   # Facade (add repos here)
│   ├── MinioServ/MinioService.cs  # File upload/download via MinIO
│   └── Models/                    # [EMPTY] Add concrete repositories here
│
├── Contracts/                     # Interfaces & DTOs
│   ├── BaseDtos/                  # BaseDto, BaseCreateDto, BaseUpdateDto
│   ├── Interfaces/Repository/
│   │   ├── IRepositoryBase.cs     # Generic CRUD interface
│   │   └── IRepositoryWrapper.cs  # Facade interface (add repos here)
│   ├── Responses/                 # Standardized API response models
│   ├── Pagging/                   # Pagination helpers
│   ├── DTOs/                      # [EMPTY] Add entity DTOs here
│   └── enums/ErrorCatalog.cs
│
├── LoggerService/                 # Serilog abstraction
│   ├── ILoggerManager.cs
│   └── LoggerManager.cs           # Writes logs to DB (Oracle or Postgres)
│
├── global.json                    # .NET SDK version pin
├── NuGet.Config                   # NuGet package source
├── YourProject.slnx               # Solution file
├── README.md                      # This file
└── WORKFLOW.md                    # Step-by-step guide: adding a new entity
```

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL 14+ **or** Oracle 19c+
- [Keycloak](https://www.keycloak.org/) (identity provider)
- MinIO (optional, only needed if using file storage)
- [dotnet-ef CLI tool](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) — install via:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## Setup

### 1. Clone / copy the template

```bash
git clone <repo-url> MyNewProject
cd MyNewProject
```

### 2. Configure your settings

Edit `Service_API/appsettings.json` and replace every `<PLACEHOLDER>` with your real values:

| Placeholder | Description |
|-------------|-------------|
| `<DB_HOST>` | PostgreSQL/Oracle server hostname |
| `<DB_PORT>` | Database port (5432 for Postgres, 1521 for Oracle) |
| `<DB_NAME>` | Database / schema name |
| `<DB_USER>` | Database username |
| `<DB_PASSWORD>` | Database password |
| `<KEYCLOAK_HOST>` | Keycloak server hostname |
| `<REALM>` | Keycloak realm name |
| `<CLIENT_ID>` | Keycloak client ID |
| `<CLIENT_SECRET>` | Keycloak client secret |
| `<MINIO_HOST>` | MinIO endpoint (optional) |
| `<MINIO_ACCESS_KEY>` | MinIO access key (optional) |
| `<MINIO_SECRET_KEY>` | MinIO secret key (optional) |
| `<BUCKET_NAME>` | MinIO bucket/upload path (optional) |

To switch to Oracle, set `"DatabaseProvider": "Oracle"`.

### 3. Restore packages

```bash
dotnet restore
```

### 4. Create the initial migration

```bash
cd Entities
dotnet ef migrations add InitialCreate --startup-project ../Service_API
dotnet ef database update --startup-project ../Service_API
cd ..
```

> The first migration will only include the `AuditLog` system table.
> Add your entity models first (see WORKFLOW.md), then create the migration.

### 5. Run the API

```bash
dotnet run --project Service_API
```

Swagger UI is available at:
- Development: `https://localhost:8152/swagger`
- Production (behind proxy): `https://yourdomain.com/api/swagger`

---

## Key Patterns

### Generic Base Controller
All controllers inherit from `BaseController<TEntity, TDto, TCreateDto, TUpdateDto>` which provides `GetAll`, `Create`, `Update`, and `Delete` out of the box.

### Generic Repository
`RepositoryBase<T, TDto, TCreateDto, TUpdateDto>` handles CRUD with Mapster projection, pagination, soft delete, and audit field population.

### Repository Wrapper (Facade)
`RepositoryWrapper` aggregates all repositories and is injected via `IRepositoryWrapper`. Call `Save()` after mutations.

### Soft Delete
All entities inheriting `BaseTable` have an `IsDeleted` flag. The base repository filters these out by default. Pass `softDelete: false` to the delete endpoint to hard-delete.

### Audit Trail
`RepositoryContext.SaveChangesAsync()` automatically writes change records to the `AuditLog` table on every update.

---

## See Also

- **WORKFLOW.md** — step-by-step guide for adding a new entity from scratch
