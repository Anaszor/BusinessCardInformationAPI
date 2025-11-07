# BusinessCardInformationAPI

A layered ASP.NET Core Web API for importing, exporting and managing business card data, plus a JavaScript/TypeScript frontend located in `/Web`. The backend follows a clean Onion architecture with Domain, Application and Infrastructure layers and uses SOLID principles (controllers orchestrate, parsing/QR logic moved to services).

## Repository layout
- `BusinessCardInformationAPI/` — ASP.NET Core Web API project (controllers, DI).
- `Application/` — application services, DTOs, interfaces (e.g. `IFileImportService`, `IQrProcessor`).
- `Domain/` — domain entities and repository interfaces.
- `Infrastructure/` — persistence, EF Core configurations and data seeding.
- `Tests/` — unit tests (xUnit + Moq).
- `Web/` — frontend (JavaScript/TypeScript). Check `Web/package.json` for actual scripts.

## Key implementation notes
- Controller is an orchestration layer only — file import and QR parsing are implemented in `Application.Services` (`FileImportService`, `QrProcessor`) and injected via abstractions (`IFileImportService`, `IQrProcessor`) to respect Single Responsibility and Dependency Inversion.
- DTOs live in `Application/DTOs`. Domain entities are in `Domain/Entities`.
- `Infrastructure` contains repository implementations and EF Core configurations (check `BusinessCardConfiguration` and `DataSeeder`).
- Unit tests mock `IBusinessCardService` and validate controller behavior using xUnit and Moq.

## Tech stack
- .NET 8 / C# 
- ASP.NET Core Web API
- xUnit, Moq for tests
- CSV / XML parsing (CSV parsing is robust against headers and invalid rows)
- ZXing for QR processing
- Frontend: Angular/TypeScript (see `Web` folder)

## Prerequisites
- .NET 8 SDK
- Node.js (LTS) and npm or yarn (for the frontend)
- Optional: Visual Studio 2022 or VS Code

## Quick setup — Backend
1. Clone the repository:
   git clone https://github.com/Anaszor/BusinessCardInformationAPI.git
2. From solution root, restore and build:
   dotnet restore
   dotnet build
3.  Configure your DB connection in `appsettings.json` or environment variables if using EF Core.
4. Run the API:
   dotnet run --project BusinessCardInformationAPI

If using Visual Studio: open the solution, __Restore__ NuGet packages, then __Build Solution__ and run via IIS Express or the project run button.

## Database migrations (EF Core)
(Assumes migrations will be created in the `Infrastructure` project and the API is the startup project.)

1. Install EF CLI (optional):
   dotnet tool install --global dotnet-ef

2. Ensure design package is referenced in `Infrastructure/*.csproj`:
   <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.*" />

3. Create a migration:
   dotnet ef migrations add InitialCreate -p Infrastructure/Infrastructure.csproj -s BusinessCardInformationAPI/BusinessCardInformationAPI.csproj -o Migrations

4. Apply migrations to the database:
   dotnet ef database update -p Infrastructure/Infrastructure.csproj -s BusinessCardInformationAPI/BusinessCardInformationAPI.csproj

Notes:
- The application also attempts to apply pending migrations at startup via `DataSeeder` which calls `_context.Database.MigrateAsync()`. If you'd like the app to apply migrations automatically, ensure the connection string is correct before running the API.

## Run the API
- From solution root:
  dotnet run --project BusinessCardInformationAPI/BusinessCardInformationAPI.csproj

- In __Visual Studio__: open solution, __Restore__, __Build Solution__, then run.

## Frontend
1. cd Web
2. npm install
3. npm start (or check `Web/package.json` for the correct script)

## Tests
- dotnet test
- In __Visual Studio__: open __Test Explorer__. If test discovery fails, ensure `Microsoft.NET.Test.Sdk` and runner packages are present in the test project and TFM matches (.NET 8).

## Quick setup — Frontend
1. Change to the frontend folder:
   cd Web
2. Install dependencies and start:
   npm install
   npm start
   (If `npm start` is not present, check `Web/package.json` for the correct script such as `dev` or `serve`: `npm run <script>`.)

## Running tests
- From the solution root:
  dotnet test

## Development & contribution
- Follow SOLID practices: keep controllers thin and push business logic into services.
- Add unit tests for services and controllers; mock dependencies via Moq.
- Use feature branches and descriptive commits. Open a PR for review.

<img width="1665" height="855" alt="image" src="https://github.com/user-attachments/assets/bb5e08b8-272b-4e60-bad6-d9437e3e8412" />
<img width="1502" height="940" alt="image" src="https://github.com/user-attachments/assets/6245acd0-360d-4096-831d-e8bc6545af75" />
<img width="1505" height="830" alt="image" src="https://github.com/user-attachments/assets/ba87a068-7318-410a-959f-bfd16739981d" />

