# AGENTS Guide for RestaurantWithAi

## Scope and source of truth
- Convention-file glob search currently matches only this file (`AGENTS.md`); no additional agent-instruction files (`AGENT.md`, `.github/copilot-instructions.md`, `.cursorrules`, etc.) are present.
- Use this file plus code examples as the working conventions for AI changes.

## Architecture map (clean-ish layering)
- `RestaurantWithAi.Api`: HTTP layer (`Controllers/*`) + app bootstrapping in `RestaurantWithAi.Api/Program.cs`.
- `RestaurantWithAi.Core`: business services (`Services/*`), repository contracts (`Contracts/*`), mapping profiles (`Mappings/*`), domain entities (`Entities/*`).
- `RestaurantWithAi.Data`: EF Core persistence (`RestaurantDbContext.cs`, `Repositories/*`, DI in `Extensions/ServiceCollectionExtensions.cs`).
- `RestaurantWithAi.Shared`: DTOs, service interfaces, options, and custom exceptions shared across layers.
- `RestaurantWithAi.Tests`: xUnit tests for service behavior, mapping, EF repositories, and data-annotation validation.

## Request/data flow pattern (apply this when adding features)
- Controller -> Shared service interface -> Core service -> Core repository contract -> Data repository -> `RestaurantDbContext`.
- Example chain: `RestaurantsController` -> `IRestaurantsService` -> `RestaurantService` -> `IRestaurantRepository` -> `RestaurantRepository`.
- Controllers translate exceptions to status codes (for example `KeyNotFoundException` -> 404 in `RestaurantsController`, `DishesController`, `TablesController`).
- Core services are thin orchestrators: null-check input, map DTOs/entities with AutoMapper, call repository/AWS operations.

## Auth and authorization specifics
- JWT auth is configured in `RestaurantWithAi.Api/Program.cs` against AWS Cognito authority.
- Token validation enforces `token_use == access` and `client_id`/`aud` matches configured `AWS:ClientId`.
- Role checks rely on Cognito groups claim (`RoleClaimType = "cognito:groups"`).
- Endpoints use role gates heavily: `Admin` for writes, `Waiter,Admin` for dish availability patch, anonymous for catalog reads.

## Persistence and domain conventions
- EF model lives in `RestaurantWithAi.Data/RestaurantDbContext.cs` (including many-to-many `DishAvailability`).
- Read queries usually use `AsNoTracking()` and include navigation collections where needed.
- `Table` uses composite key `{ RestaurantId, TableNumber }`; `Waiter` key is string `UserId`.
- `RestaurantRepository.GetAllRestaurantsAsync` has provider-aware city filtering (`EF.Functions.Collate` on SQL Server, ordinal-ignore-case fallback otherwise).

## Mapping and DTO rules
- Mapping profiles in `RestaurantWithAi.Core/Mappings/*` intentionally ignore identity/navigation fields on create/update DTO maps.
- Keep validation at DTO level using data annotations in `RestaurantWithAi.Shared/*Request.cs`.
- Waiter mapping is Cognito-user based (`WaiterMappingProfile` maps `UserType` and reads `custom:restaurantId`).

## Local workflows (verified)
- Build/test whole solution:
  - `dotnet test C:\Users\Roman\source\RestaurantWithAi\RestaurantWithAi.sln -v minimal`
- Run API project:
  - `dotnet run --project C:\Users\Roman\source\RestaurantWithAi\RestaurantWithAi.Api\RestaurantWithAi.Api.csproj`
- Default dev URLs come from `RestaurantWithAi.Api/Properties/launchSettings.json` (`https://localhost:7153`, `http://localhost:5239`).

## Swagger docs locations
- Swagger is configured in `RestaurantWithAi.Api/Program.cs` (`AddSwaggerGen`, `UseSwagger`, `UseSwaggerUI`), and UI/JSON endpoints are enabled in Development.
- When running locally with the default launch profile, use `https://localhost:7153/swagger` (UI) and `https://localhost:7153/swagger/v1/swagger.json` (OpenAPI JSON).
- Generated/openapi artifact is checked in at `RestaurantWithAi.Api/openapi/restaurantwithai.v1.json`.
- Authorization behavior for secured operations in Swagger is customized in `RestaurantWithAi.Api/Swagger/AuthorizeOperationFilter.cs`.

## Configuration and integration points
- Required options section: `AWS` (`Region`, `UserPoolId`, `ClientId`, `Authority`, `ClientSecret`) in `AwsCognitoOptions`.
- `ClientSecret` is required by `CognitoAuthService.ComputeSecretHash`; missing value fails login/register paths.
- SQL Server connection string resolution in DI is `ConnectionStrings:LocalConnection` first, then `ConnectionStrings:DefaultConnection` fallback (`RestaurantWithAi.Data/Extensions/ServiceCollectionExtensions.cs`).
- `appsettings.json` currently includes `ConnectionStrings:LocalConnection` and AWS values, but still omits `AWS:ClientSecret`; expect user-secrets/environment overrides for secret values.

## Change checklist for agents
- Preserve layer boundaries: no EF or AWS calls directly in controllers.
- If a new endpoint is added, update Shared interface + Core service + contract/repository + mapping profile + tests together.
- Follow existing exception-to-HTTP translation style in controllers instead of introducing global exception middleware patterns ad hoc.
- Extend tests in `RestaurantWithAi.Tests` near the touched component (service tests use Moq; repository tests use EF InMemory contexts).
