# E-commerce Platform — Project Guide

---

## Architecture

**Modular Monolith** — Independent modules inside a single deployable unit. Each module follows Clean Architecture
layers (Domain, Application, Infrastructure, Api). Modules are decoupled and communicate through shared contracts.
`Ecommerce.AppHost` is the composition root: it owns cross-cutting configuration (authentication, authorization map,
CORS, rate limiting, Scalar, Key Vault) and registers every module uniformly.

### Evolution Roadmap

| Phase | Description |
| --- | --- |
| **1. Modular Monolith** | Logical module boundaries, separate DbContexts/schemas, synchronous cross-module communication via shared contracts. *(current)* |
| **2. DDD** | Tactical patterns within modules — aggregates, value objects, domain events, bounded contexts formalized. Well-defined boundaries set the stage for async communication. |
| **3. Event-Driven** | Message broker (RabbitMQ + MassTransit) for async inter-module communication via integration events, replacing synchronous cross-module calls. Eventual consistency between modules. |
| **4. Microservice Extraction** | Extract the **Payment** module into an independent deployable. Own database, CI pipeline, and API. Communicates with the monolith exclusively through integration events. |

---

## Modules

| Module | Description | Status |
| --- | --- | --- |
| **Auth** | User management, login, sessions, roles and account lifecycle. | Implemented |
| **Catalog** | Product and category management, product images, storefront listing. | Implemented |
| **Orders** | Shopping cart, checkout, and order history. | Planned |
| **Shipping** | Shipping calculation via Correios API. | Planned |
| **Payment** | Payment processing via external gateway. | Planned |
| **Notifications** | Email and push notifications triggered by domain events. | Planned |

---

## Technologies

| Technology | Purpose |
| --- | --- |
| **.NET 10 / ASP.NET Core 10 / C#** | API development |
| **PostgreSQL 18** | Relational database |
| **Entity Framework Core** | ORM, migrations, and data access |
| **Controllers + vertical-slice folders** | HTTP endpoint style |
| **JWT Bearer + BCrypt.Net** | Authentication and password hashing (no ASP.NET Identity) |
| **MediatR** | CQRS — command/query dispatch |
| **FluentValidation** | Declarative request validation |
| **Azure Blob Storage** (Azurite locally) | Product image storage |
| **Microsoft.Extensions.Logging** (`[LoggerMessage]`) | Structured logging to stdout, shipped to Log Analytics |
| **Blazor WebAssembly + MudBlazor** | Admin SPA (`Ecommerce.Admin.Web`) |
| **Docker + Docker Compose** | Containerization |
| **Scalar UI** (over OpenAPI) | Interactive API documentation |
| **GitHub Actions** | CI/CD pipelines |
| **Azure** | Container Apps, Static Web Apps, Azure SQL, Blob Storage, Key Vault |

---

## Testing

| Type | Scope |
| --- | --- |
| **Unit** | Domain rules, handlers, validators, request mappers |
| **Integration** | API endpoints, database queries, blob storage (WebApplicationFactory + Testcontainers) |

| Tool | Purpose |
| --- | --- |
| **xUnit** | Test framework |
| **NSubstitute** | Mocking |
| **Bogus** | Test data generation |
| **Shouldly** | Assertions |
| **Testcontainers** | PostgreSQL + Azurite containers for integration tests |
| **Respawn** | Database reset between integration tests |

---

## Cross-Cutting Concepts

| Concept | Description |
| --- | --- |
| **Single Error Contract** | Every failure resolves to an RFC 7807 `ProblemDetails` written by one `ProblemDetailsWriter`, so the response shape never drifts. |
| **Exception Contract** | Handlers throw exceptions implementing `IExceptionContract` (e.g. `ResourceNotFoundException` → `404`, `BusinessRuleValidationException` → `409`). `GlobalExceptionHandler` (`IExceptionHandler`) maps them generically — no type-specific branches — and falls back to a `500` with no internal leakage. |
| **Validation via FluentValidation** | `RequestValidationFilter`, registered globally as an action filter, resolves the request's `IValidator<T>` and short-circuits with a `400 ValidationProblemDetails` before the action runs. |
| **Business Rules** | Entity-state invariants are `IBusinessRule` implementations validated through the Kernel's `BusinessRule` abstraction in the Domain. Query-dependent rules (uniqueness, existence) live in the Application handlers. |
| **Base Types** | The Kernel provides `Entity` (`Id (int)`), `ValueObject`, `IRepository<T>` and `PagedResult<T>`. |
| **Permission-Based Authorization** | Each module declares its own permissions; endpoints use `RequireClaim("permission", …)` — never role checks. `RolePermissionMap` lives in the AppHost, so Auth never learns other modules' permissions. |
| **Security Baseline** | HSTS, security headers, CORS allowlist for the SPA origin, global and per-endpoint rate limiting, forwarded headers behind the Azure ingress. |
| **Structured Logging** | `[LoggerMessage]` source-generated logs, one metadata-only HTTP log entry per request, and a per-request scope carrying `RequestId`/`UserId`. Ids only — never entities, credentials or tokens. |
| **DbContext per Module** | Each module has its own DbContext with a dedicated PostgreSQL schema (`catalog`, `auth`). No cross-module table access. |
| **Module DI Registration** | Each module's Api project exposes `Add{ModuleName}Module(IServiceCollection, IConfiguration)` and `Use{ModuleName}Module(WebApplication, bool applyMigrations)`. `ModulesRegistry` in the AppHost is the only place that calls them. |
| **Secrets** | Production configuration comes from Azure Key Vault through a system-assigned managed identity. Two environments only: local (Docker Compose) and Production. |

---

## Cross-Module Communication

Modules never reference each other directly. The Kernel defines `IModule` (`ExecuteCommandAsync` / `ExecuteQueryAsync`),
and each module publishes a typed contract in its Application layer (`ICatalogModule`, `IAuthModule`). The owning module
implements it with an internal MediatR-backed adapter, so consumers depend on the contract instead of `ISender`.

---

## Folder Structure

```text
src/
├── Ecommerce.AppHost/                                # Host — composition root
│   ├── Program.cs                                    # Pipeline, middleware, module registration
│   ├── Authorization/                                # AuthorizationRegistry, RolePermissionMap
│   ├── Modules/                                      # ModulesRegistry
│   ├── Scalar/                                       # OpenAPI document transformers
│   └── Security/                                     # CORS policy and settings
│
├── Ecommerce.Kernel/                                 # Cross-cutting code shared by all modules
│   ├── Ecommerce.Kernel.Domain/                      # Entity, ValueObject, BusinessRule, IRepository, PagedResult
│   ├── Ecommerce.Kernel.Application/                 # IModule, ICommand/IQuery, IUserContext
│   ├── Ecommerce.Kernel.Infrastructure/              # Mediator adapter, persistence, settings
│   └── Ecommerce.Kernel.API/                         # Exception handler, validation filter, security, observability
│
├── {ModuleName}/
│   ├── Ecommerce.{ModuleName}.Domain/                # Entities, value objects, rules, repository/storage ports
│   ├── Ecommerce.{ModuleName}.Infrastructure/        # DbContext, migrations, repositories, external clients
│   ├── Ecommerce.{ModuleName}.Application/           # Commands, queries, handlers, DTOs
│   ├── Ecommerce.{ModuleName}.Api/                   # Controllers, request contracts, validators, module DI
│   ├── Ecommerce.{ModuleName}.UnitTests/             # Layer subfolders: Domain/, Application/, Api/
│   └── Ecommerce.{ModuleName}.IntegrationTests/      # Fixtures + endpoint tests per feature
│
├── Ecommerce.Admin.Web/                              # Blazor WebAssembly admin SPA (MudBlazor)
│
├── Ecommerce.slnx                                    # Solution file
└── compose.yaml                                      # Docker Compose: PostgreSQL + API (+ Azurite via override)
```

### Conventions

- Layers are built in the order **Domain → Infrastructure → Application + Api**; ports are declared in the Domain.
- Inside Application and Api, code is organized in **vertical-slice folders per endpoint**
  (`Products/CreateProduct/`), not by technical type.
- Request/response contracts belong to the Api layer; controllers dispatch through `ToCommand()` / `ToQuery()` mappers.
