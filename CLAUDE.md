# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Quick commands

### Backend (.NET)

Run API:
```bash
# from repo root

dotnet run --project RestaurantWithAi.Api
```

Build:
```bash
dotnet build RestaurantWithAi.sln
```

Run all tests:
```bash
dotnet test RestaurantWithAi.sln
```

Run a single test project:
```bash
dotnet test RestaurantWithAi.Tests/RestaurantWithAi.Tests.csproj
```

Run a single test (filter by fully-qualified name or substring):
```bash
# examples

dotnet test RestaurantWithAi.Tests/RestaurantWithAi.Tests.csproj --filter FullyQualifiedName~DishServiceTests

dotnet test RestaurantWithAi.Tests/RestaurantWithAi.Tests.csproj --filter "FullyQualifiedName=RestaurantWithAi.Tests.Core.Services.DishServiceTests.CreateDishAsync_WhenX_ShouldY"
```

### Frontend (Angular)

When implementing frontend tasks, first run the dev server so you can reproduce issues and verify UI changes:
```bash
cd frontend
npm start
# (ng serve) -> http://localhost:4200
```

Install deps:
```bash
cd frontend
npm ci
```

Dev server:
```bash
cd frontend
npm start
# (ng serve) -> http://localhost:4200
```

Build:
```bash
cd frontend
npm run build
```

Unit tests (Vitest via Angular CLI):
```bash
cd frontend
npm test
```

## Database migrations (EF Core)

Migrations live in `RestaurantWithAi.Data/Migrations/` and the DbContext is `RestaurantWithAi.Data/RestaurantDbContext.cs`.

List migrations:
```bash
dotnet ef migrations list --project RestaurantWithAi.Data --startup-project RestaurantWithAi.Api
```

Add a migration:
```bash
# name example: AddReservationIndexes

dotnet ef migrations add <MigrationName> --project RestaurantWithAi.Data --startup-project RestaurantWithAi.Api
```

Apply migrations (update database):
```bash
dotnet ef database update --project RestaurantWithAi.Data --startup-project RestaurantWithAi.Api
```

Remove last migration (only if not applied):
```bash
dotnet ef migrations remove --project RestaurantWithAi.Data --startup-project RestaurantWithAi.Api
```

## Repository structure (big picture)

This is a .NET 9 solution + an Angular frontend:

- `RestaurantWithAi.Api/` — ASP.NET Core Web API host.
  - Controllers live in `RestaurantWithAi.Api/Controllers/*Controller.cs` and are routed as `api/[controller]`.
  - Authentication is JWT Bearer against AWS Cognito (see **Auth** below).
  - DI is composed in `RestaurantWithAi.Api/Program.cs` via extension methods from Core/Data.

- `RestaurantWithAi.Core/` — application layer.
  - Defines repository contracts in `RestaurantWithAi.Core/Contracts/*`.
  - Implements business services in `RestaurantWithAi.Core/Services/*`.
  - Wiring: `RestaurantWithAi.Core/Extensions/ServiceCollectionExtensions.cs` registers services and AutoMapper profiles.

- `RestaurantWithAi.Data/` — infrastructure/persistence layer (EF Core + SQL Server).
  - `RestaurantDbContext` is in `RestaurantWithAi.Data/RestaurantDbContext.cs`.
  - Repository implementations are in `RestaurantWithAi.Data/Repositories/*`.
  - Wiring: `RestaurantWithAi.Data/Extensions/ServiceCollectionExtensions.cs` registers repositories and adds `RestaurantDbContext` using SQL Server.
  - EF migrations are in `RestaurantWithAi.Data/Migrations/`.

- `RestaurantWithAi.Shared/` — shared boundary types.
  - Request/response DTOs and service interfaces used across layers (and by controllers).
  - Also contains shared exceptions and options (e.g., `AwsCognitoOptions`).

- `RestaurantWithAi.Tests/` — xUnit test project.
  - Tests cover Core services and Data repositories.

- `frontend/` — Angular app (Angular CLI). Uses `@angular/router` and SCSS.

## Backend request flow

Typical flow for an HTTP request:

`Controller` (Api) → calls an `I*Service` (from `RestaurantWithAi.Shared`) → service (Core) uses `I*Repository` (Core contract) → repository (Data) uses EF Core `RestaurantDbContext` → SQL Server.

This separation is enforced via DI:
- `Program.cs` calls `AddCoreServices()` and `AddDataServices(configuration)`.

## Auth (AWS Cognito + roles)

- Auth endpoints are in `RestaurantWithAi.Api/Controllers/AuthenticationController.cs`:
  - `POST api/authentication/login`
  - `POST api/authentication/register`

- JWT validation is configured in `RestaurantWithAi.Api/Program.cs`:
  - Authority is `AWS:Authority`.
  - Role claim type is `cognito:groups` (so `[Authorize(Roles = "...")]` checks Cognito group membership).
  - `token_use` must be `access` and `client_id` must match `AWS:ClientId`.

- Cognito settings are bound from configuration section `AWS` (`RestaurantWithAi.Shared/Options/AwsCognitoOptions.cs`).
  - Note: `ClientSecret` is required by options validation; it is not present in `RestaurantWithAi.Api/appsettings.json` (likely supplied via User Secrets or environment variables).

## Persistence

- SQL Server connection string is pulled from:
  - `ConnectionStrings:LocalConnection` (preferred)
  - or `ConnectionStrings:DefaultConnection`

Default dev config uses LocalDB (`RestaurantWithAi.Api/appsettings.json`).