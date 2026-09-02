<h1 align="center">Ecommerce</h1>
<p align="center">
  <a href="https://github.com/marcuscfarias/ecommerce-platform/issues"><img alt="number of issues for the repository" src="https://img.shields.io/github/issues/marcuscfarias/ecommerce-platform?color=red&label=Issues&style=for-the-badge" target="_blank" /></a>
  <a href="https://github.com/marcuscfarias/ecommerce-platform"><img alt="the size of the repo in kb" src="https://img.shields.io/github/repo-size/marcuscfarias/ecommerce-platform?color=orange&label=Repo-Size&style=for-the-badge" target="_blank" /></a>
  <a href="https://opensource.org/licenses/MIT"><img alt="licence the repo is published under" src="https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge" target="_blank" /></a>
 <a href="https://github.com/marcuscfarias/ecommerce-platform/graphs/contributors"><img alt="the number of contributors on the repo" src="https://img.shields.io/github/contributors/marcuscfarias/ecommerce-platform?color=brightgreen&label=Contributors&style=for-the-badge" target="_blank" /></a>
  <a href="https://github.com/marcuscfarias/ecommerce-platform/network/members"><img alt="total number of times the repo has been forked" src="https://img.shields.io/github/forks/marcuscfarias/ecommerce-platform?color=blue&label=Forks&style=for-the-badge" target="_blank" /></a>
  <a href="https://github.com/marcuscfarias/ecommerce-platform/stargazers"><img alt="total number of times the repo has been starred" src="https://img.shields.io/github/stars/marcuscfarias/ecommerce-platform?color=blueviolet&label=Stars&style=for-the-badge" target="_blank" /></a>
</p>

## Summary

<!-- TOC -->

* [Summary](#summary)
* [1. About this project](#1-about-this-project)
* [2. Screenshots or Demo](#2-screenshots-or-demo)
* [3. Getting started](#3-getting-started)
* [4. Functionalities](#4-functionalities)
* [5. Implementation details](#5-implementation-details)
* [6. Contributing](#6-contributing)
* [7. License](#7-license)

<!-- TOC -->

## 1. About this project

Ecommerce is a personal portfolio project I use to practice modern full-stack development on .NET (focused on backend).
It's built as a
**modular monolith** in ASP.NET Core 10, with two **Blazor WebAssembly** SPAs on top — a storefront and a backoffice —
and it runs **live on Azure** (Container Apps, Static Web Apps, Blob Storage and Key Vault) over **Postgres on Neon**.

The project is **evolutionary by design**: instead of a one-shot build, it grows feature by feature — each as a vertical
slice with its own tests — so the architectural decisions stay visible in the commit history.

The first milestone, the **backoffice**, is complete: catalog management (categories and products, including image
upload and delivery) and user management (authentication, authorization, roles and account lifecycle). The
customer-facing **storefront** is now building on that backbone: anonymous browsing, search and product pages are live,
with the cart and checkout next.

## 2. Screenshots or Demo

> ⚠️⚠️⚠️ **Cold start.** To keep hosting costs near zero, the API scales to zero when idle. The first time you open the site
> after an idle period, it takes around **1m30s** to spin everything back up — after that it responds normally. Both SPAs
> watch for a request hanging past three seconds and explain the wait on screen, so it doesn't read as a broken site. ⚠️⚠️⚠️

The project is live:
* **Storefront** — [ecommerce.marcuscfarias.com](https://ecommerce.marcuscfarias.com)
* **Admin SPA** — [admin-ecommerce.marcuscfarias.com](https://admin-ecommerce.marcuscfarias.com)
* **API docs (Scalar)** — [api-ecommerce.marcuscfarias.com/scalar](https://api-ecommerce.marcuscfarias.com/scalar)

### Demo logins

Sign in to the Admin SPA with one of the seeded profiles to explore how the UI adapts to each role's permissions:

| Role        | Email                   | Password      | Can do                         |
|:------------|:------------------------|:--------------|:-------------------------------|
| **Owner**   | `owner@ecommerce.com`   | `Owner@123`   | View users, manage the catalog |
| **Manager** | `manager@ecommerce.com` | `Manager@123` | Manage the catalog             |

A YouTube walkthrough is planned to make it easier to get familiar with the project. _(Coming soon.)_

## 3. Getting started

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Docker](https://www.docker.com/) with Docker Compose

> The local dev stack is Windows-oriented — it mounts the dev HTTPS certificate from `%USERPROFILE%`.

### 1. Clone the repository

```bash
git clone https://github.com/marcuscfarias/ecommerce-platform.git
cd ecommerce-platform
```

### 2. Trust a local HTTPS certificate

The API serves over HTTPS on port `8081`, and the SPA calls it from the browser — so Kestrel needs a TLS certificate the
container can load and your browser will trust. Export the .NET dev certificate to a password-protected `.pfx` (the
compose file mounts this folder into the API container), then trust it:

```powershell
dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\ecommerce.pfx -p <cert-password>
dotnet dev-certs https --trust
```

The `-p` password must match `ASPNETCORE_Kestrel__Certificates__Default__Password` in the `.env` you create next.

### 3. Configure the environment

```bash
cd src
cp .env.example .env
```

Edit `.env`:

* `POSTGRES_PASSWORD` — a strong database password, mirrored inside `ConnectionStrings__EcommerceDb`.
* `ASPNETCORE_Kestrel__Certificates__Default__Password` — the `<cert-password>` you chose in step 2.

Email needs no setup: `compose.override.yaml` sets `Email__Provider=Console`, so transactional emails are written to
the API log instead of being sent. No provider account and no API key are required to run the project locally.

### 4. Run the backend

```bash
docker compose up --build
```

This boots three containers:

* `ecommerce-db` — PostgreSQL 18 with a persistent volume.
* `ecommerce-azurite` — local Azure Blob Storage emulator for product images.
* `ecommerce-api` — the API (`Ecommerce.AppHost`) on [`https://localhost:8081`](https://localhost:8081).

EF Core migrations are applied and the default roles, admin and demo users (see [Demo logins](#demo-logins)) are seeded
on startup, so the API is ready as soon as it answers.

Open the **Scalar UI** at [`https://localhost:8081/scalar`](https://localhost:8081/scalar) to explore the endpoints.

### 5. Run the SPAs

```bash
dotnet run --project Ecommerce.Admin.Web
dotnet run --project Ecommerce.Shop.Web
```

Both Blazor WebAssembly apps talk to the API on port `8081`. The backoffice opens at
[`https://localhost:7777`](https://localhost:7777) — sign in with any of the [Demo logins](#demo-logins) — and the
storefront at [`https://localhost:7778`](https://localhost:7778), which needs no sign-in.

### Run tests

```bash
cd src
dotnet test
```

Integration tests require Docker to be running. See [5.6 Integration testing](#56-integration-testing) for the
composition.

## 4. Functionalities

Features are grouped by delivery phase. The **backoffice** (admin-facing) milestone is shipped and the **storefront**
(customer-facing) phase is under way. **Cross-cutting** concerns span both. Each feature is expanded in
[5. Implementation details](#5-implementation-details).

### 4.1 Backoffice — ✅ shipped

The admin-facing operations that run the store.

| Module    | Feature                        | Status  |
|:----------|:-------------------------------|:-------:|
| Catalog   | Category Management            | 🟢 Done |
| Catalog   | Product Management             | 🟢 Done |
| Auth      | User Management                | 🟢 Done |
| Auth      | Authentication & Authorization | 🟢 Done |
| Admin Web | Blazor WASM SPA                | 🟢 Done |

### 4.2 Storefront — 🚧 in progress

The customer-facing shop, built on top of the backoffice backbone.

| Module        | Feature                   |  Status  |
|:--------------|:--------------------------|:--------:|
| Catalog       | Product browsing & search | 🟢 Done  |
| Shop Web      | Blazor WASM SPA           | 🟢 Done  |
| Orders        | Cart & checkout           | 🔴 To do |
| Orders        | Order management          | 🔴 To do |
| Payment       | Payment (microservice)    | 🔴 To do |
| Shipping      | Shipping & fulfillment    | 🔴 To do |
| Notifications | Customer notifications    | 🔴 To do |

### 4.3 Cross-cutting

Platform concerns not tied to a single module.

| Feature                            |  Status  |
|:-----------------------------------|:--------:|
| Request Validation                 | 🟢 Done  |
| Global Exception Handling          | 🟢 Done  |
| API Documentation (Scalar)         | 🟢 Done  |
| Rate Limiting                      | 🟢 Done  |
| Integration Tests (Testcontainers) | 🟢 Done  |
| CI/CD (GitHub Actions)             | 🟢 Done  |
| Deployment & Environments (Azure)  | 🟢 Done  |
| Logging                            | 🟢 Done  |
| Transactional Email                | 🟢 Done  |
| Observability                      | 🔴 To do |
| Infrastructure as Code (IaC)       | 🔴 To do |

## 5. Implementation details

This section explains how the more interesting parts work — the technologies, patterns and reasoning behind the
Functionalities above.

### 5.1 Tech stack

* **.NET 10 / ASP.NET Core 10 / C#** — API runtime and framework.
* **PostgreSQL** with **Entity Framework Core** (Npgsql) — relational store, migrations and data access.
* **MediatR** — CQRS dispatch for commands and queries.
* **FluentValidation** — declarative request validation.
* **JWT bearer** + **BCrypt.Net** — token authentication and password hashing.
* **Scalar** (over OpenAPI) — interactive API documentation.
* **Microsoft.Extensions.Logging** (`[LoggerMessage]` + HTTP logging) — structured logs to stdout (JSON in production) shipped to **Azure Log Analytics**.
* **Unit testing** — xUnit, NSubstitute, Bogus, Shouldly.
* **Integration testing** — Testcontainers (PostgreSQL + Azurite), Respawn, WebApplicationFactory.
* **Docker** + **Docker Compose** — containerization.
* **GitHub Actions** — CI/CD.
* **Azure** — Container Apps, Static Web Apps, Blob Storage, Key Vault.
* **Neon** — serverless Postgres hosting in production.
* **Resend** — transactional email delivery in production.
* **Blazor WebAssembly** + **MudBlazor** — storefront and backoffice SPAs.

### 5.2 Architecture

`Ecommerce.AppHost` is the composition root. Each bounded context is a **module** (Catalog, Auth) split into
Domain / Application / Infrastructure / Api layers and registered uniformly through `Add{Module}Module` /
`Use{Module}Module` via `ModulesRegistry`.

Modules never reference each other directly: cross-module calls go through `IModule` (Kernel) and per-module contracts
(e.g. `ICatalogModule` in `Ecommerce.Catalog.Application`), implemented by an internal mediator-backed adapter
(`MediatorModuleBase`) so consumers see a typed contract instead of `ISender`. Domain invariants are enforced with a
small `BusinessRule` abstraction in the Kernel.

### 5.3 Request pipeline

Every request resolves to one consistent error contract — an [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807)
`ProblemDetails` written by a single `ProblemDetailsWriter`, so the response shape never drifts:

* **Validation** — `RequestValidationFilter` (registered globally) resolves the request's `IValidator<T>` and, on
  failure, short-circuits with a `400 ValidationProblemDetails` before the controller action runs.
* **Exceptions** — handlers throw exceptions implementing `IExceptionContract` (e.g. `ResourceNotFoundException` →
  `404`, `BusinessRuleValidationException` → `409`); `GlobalExceptionHandler` maps them through the same writer.
  Anything else falls back to a generic `500` with no internal leakage.

### 5.4 Authentication, authorization & security

Auth issues **JWT access tokens** (signed inside the Auth module) delivered as **httpOnly cookies**, with
**refresh-token rotation** and logout revocation; passwords are hashed with **BCrypt**. Login is guarded by **account
lockout** and **per-endpoint rate limiting**.

Authorization is **permission-based**: each module declares its own permissions and protects endpoints with
`RequireClaim("permission", …)` (no role checks). The `RolePermissionMap` lives in the composition root, so the Auth
module never learns other modules' permissions. The API also sets **security headers** and **HSTS**, a **CORS
allowlist** for the SPA origin, and honors **forwarded headers** behind the Azure ingress.

### 5.5 Product images: storage & delivery

Product images live in **Blob Storage** — Azurite locally, a **private** Azure Storage container in production
(`PublicAccessType.None`, no anonymous access). The catalog persists only the **blob key** (`ImageKey`), never a full
URL, so the storage account or container can change without rewriting data.

Because the container is private, images are served **through the API**. The `GET {id}/image` endpoint **streams** the
blob straight to the response (`DownloadStreamingAsync` → `File(...)`) instead of buffering it in memory, and flows the
blob's **`ETag`** back as a response header. ASP.NET Core then resolves `If-None-Match` automatically, returning
**`304 Not Modified`** when the client already has the current image — so browsers re-validate cheaply while the storage
account stays locked down.

### 5.6 Integration testing

Integration tests run against a real **PostgreSQL** container (`Testcontainers.PostgreSql`); EF migrations are applied
through a `WebApplicationFactory` on host startup, and **Respawn** resets the schema between tests so each one starts
from a known state. Each module owns a thin fixture over a shared base (e.g. `CatalogIntegrationFixture`) that exposes
`Client`, `ResetDatabaseAsync()` and `SeedAsync<TDbContext>()`, keeping the wiring out of the test classes. Unit tests
use **xUnit + NSubstitute + Bogus + Shouldly**.

### 5.7 CI/CD & deployment

Six GitHub Actions workflows split shared gates from per-stack pipelines:

* **`ci.yml`** — shared gates on every push/PR: Conventional Commits, `dotnet format`, config-file lint,
  vulnerable-dependency check and secret scan (gitleaks).
* **`backend-ci.yml`** (+ reusable **`backend-test.yml`**) — restore, build in Release, unit + integration tests
  (Testcontainers) and a Docker image build validation.
* **`frontend-ci.yml`** — builds both Blazor WebAssembly SPAs.
* **`backend-cd.yml`** — publishes the API image and deploys it to **Azure Container Apps** via OIDC, gated on tests and
  a `/health` check.
* **`frontend-cd.yml`** — deploys the backoffice and the store SPA, each to its own **Azure Static Web App**, via OIDC.

Production secrets come from **Azure Key Vault** (system-assigned managed identity) and product images from **private
Blob Storage** proxied through the API. Two environments only: local (Docker Compose) and Production.

The API scales to zero between visits, so each SPA wraps its `HttpClient` in a `DelegatingHandler` that flags any
request still pending after three seconds. The notice it raises stays up until the visitor reloads — the wait it
describes ends with a reload, and a request that dies of timeout would otherwise fall back to a generic error.

### 5.8 Logging

Structured logging on the **native `Microsoft.Extensions.Logging`** stack, written through the **`[LoggerMessage]`
source generator**. Output goes to stdout (JSON in production) and is shipped by **Azure Container Apps** to **Log
Analytics**. The **HTTP logging middleware** records one metadata-only entry per request, a per-request scope adds
`RequestId`/`UserId` (and the W3C `TraceId` surfaces in the error response) for correlation, and handlers log key
domain and auth events — always **ids only, never entities, credentials or tokens**.

### 5.9 Transactional email

Modules send mail through a single port, **`IEmailSender`**, declared in `Ecommerce.Kernel.Application`. No handler
names a provider, so replacing the provider never reaches the callers. Two adapters implement it in
`Ecommerce.Kernel.Infrastructure`: **`ConsoleEmailSender`** writes the whole message to the log, and
**`ResendEmailSender`** posts it to **Resend** through a typed `HttpClient` with a 10-second timeout.

The active adapter comes from an explicit **`Email:Provider`** setting (`Console` or `Resend`), validated with
`ValidateOnStart` — an absent or misspelled value fails the boot rather than defaulting. Selecting by environment was
rejected on purpose: a wrong mode either sends real mail from a developer machine or silently sends nothing, and both
are invisible until a customer complains. In production the API key is resolved from **Key Vault** at boot; locally
nothing is configured because `Console` needs no account.

`SendAsync` **never throws**. Every failure — non-2xx, transport error, timeout, even a cancelled token — degrades to
a single `Warning` and returns normally, so a provider outage cannot break the flow that triggered the email. The
recipient address and the message body stay out of every log entry the Resend adapter writes.

## 6. Contributing

You can send how many PR's do you want, I'll be glad to analyze and accept them! And if you have any question about the
project just ask...

## 7. License

This project is licensed under the MIT License - see
the [LICENSE.md](https://github.com/MarcusCFarias/ecommerce-platform/blob/main/LICENSE) file for details
