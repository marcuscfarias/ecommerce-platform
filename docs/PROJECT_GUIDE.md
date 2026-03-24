# E-commerce Platform v1.0.0 — Project Guide

---

## Architecture

**Modular Monolith** — Independent modules inside a single deployable unit. Each module follows Clean Architecture layers (Domain, Application, Infrastructure). Modules are decoupled and communicate through shared interfaces.

### Evolution Roadmap

| Phase | Description |
|---|---|
| **1. Modular Monolith** | Logical module boundaries, separate DbContexts/schemas, synchronous cross-module communication via shared interfaces. *(current)* |
| **2. DDD** | Tactical patterns within modules — aggregates, value objects, domain events, bounded contexts formalized. Well-defined boundaries set the stage for async communication. |
| **3. Event-Driven** | Message broker (RabbitMQ + MassTransit) for async inter-module communication via integration events, replacing synchronous cross-module calls. Eventual consistency between modules. |
| **4. Microservice Extraction** | Extract the **Payment** module into an independent deployable. Own database, CI pipeline, and API. Communicates with the monolith exclusively through integration events. |

---

## Modules

| Module | Description |
|---|---|
| **Auth** | User registration, login, and role management. |
| **Catalog** | Product and category management. |
| **Orders** | Shopping cart, checkout, and order history. |
| **Shipping** | Shipping calculation via Correios API. |
| **Payment** | Payment processing via external gateway. |
| **Notifications** | Email and push notifications triggered by domain events. |

---

## Technologies

| Technology | Purpose |
|---|---|
| **.NET 10 / ASP.NET Core 10 / C#** | API development |
| **PostgreSQL 17** | Relational database |
| **Entity Framework Core** | ORM, migrations, and data access |
| **ASP.NET Identity + JWT Bearer** | Authentication and authorization |
| **Minimal APIs** | HTTP endpoint style |
| **MediatR** | CQRS — command/query dispatch |
| **Docker + Docker Compose** | Containerization |
| **Scalar UI** | Interactive API documentation |
| **GitHub Actions** | CI/CD pipelines |
| **Azure** | Cloud deployment |

---

## Testing

| Type | Scope |
|---|---|
| **Unit** | Domain logic, handlers, validators |
| **Integration** | API endpoints, database queries (WebApplicationFactory + Testcontainers) |

| Tool | Purpose |
|---|---|
| **xUnit** | Test framework |
| **NSubstitute** | Mocking |
| **Verify** | Snapshot testing |
| **Bogus** | Test data generation |
| **Shouldly** | Assertions |
| **Testcontainers** | PostgreSQL container for integration tests |

---

## Cross-Cutting Concepts

| Concept | Description |
|---|---|
| **Result Pattern** | Custom `Result<T>` type. Handlers return success/error without throwing exceptions. |
| **Global Exception Middleware** | Catches unhandled exceptions and returns standardized `ProblemDetails` (RFC 7807). |
| **Validation via FluentValidation** | Declarative request validation integrated as a MediatR pipeline behavior. Runs automatically before every handler. |
| **Base Entity** | Abstract `Entity` class with `Id (Guid)`. All domain entities inherit from it. |
| **DbContext per Module** | Each module has its own DbContext with a dedicated PostgreSQL schema. No cross-module table access. |
| **Module DI Registration** | Each module has an Api project that exposes `Add{ModuleName}Module(IServiceCollection, IConfiguration)` and `Map{ModuleName}Endpoints(IEndpointRouteBuilder)` extension methods. The host calls these in `Program.cs`. |

---

## Cross-Module Communication

Modules never reference each other directly. Shared interfaces (e.g., `ICatalogQuery`) are defined in the `Shared` project. The owning module implements it, the consuming module depends only on the interface.

---

## Folder Structure

```
src/
├── Ecommerce.AppHost/                                # Host — composition root
│   ├── Program.cs                                    # Registers modules, middleware, pipeline
│   └── Middleware/                                    # Global exception handling
│
├── Ecommerce.Shared/                                 # Cross-cutting code shared by all modules
│   ├── Domain/                                       # Entity base, Result<T>
│   ├── Application/                                  # MediatR pipeline behaviors
│   ├── Infrastructure/                               # Shared infrastructure utilities
│   └── Contracts/                                    # Cross-module interfaces
│
├── {ModuleName}/
│   ├── Ecommerce.{ModuleName}.Api/                   # Endpoints, module DI registration, route paths
│   ├── Ecommerce.{ModuleName}.Domain/                # Entities, value objects, enums
│   ├── Ecommerce.{ModuleName}.Application/           # Commands, queries, handlers, DTOs, validators
│   ├── Ecommerce.{ModuleName}.Infrastructure/        # DbContext, repositories, external API clients
│   ├── Ecommerce.{ModuleName}.UnitTests/             # Layer subfolders: Domain/, Application/
│   └── Ecommerce.{ModuleName}.IntegrationTests/      # Layer subfolders: Infrastructure/
│
├── Ecommerce.slnx                                    # Solution file
└── compose.yaml                                      # Docker Compose: PostgreSQL + API
```
