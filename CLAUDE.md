# F.O.G.O. Talos — Ferramenta Onisciente de Gestão e Operações

## Communication rules
- Talk to the developer in **Brazilian Portuguese**.
- All **code** is in **English**: classes, methods, variables, files, folders, routes, database objects, commit message suggestions.
- All **user-facing UI text** (labels, buttons, messages, alerts) is in **Brazilian Portuguese**.
- Work in **phases** (see "Delivery phases"). At the end of each phase: run `dotnet build` and `dotnet test`, summarize what was done, explain key decisions briefly, and **stop and wait for confirmation** before starting the next phase.
- When something depends on the developer's machine (installs, ports, Docker, credentials), explain it step by step and ask before running anything that changes the system.
- Never invent requirements outside this document without asking first.
- **Git is controlled exclusively by the developer.** Never run `git add`, `git commit`, `git push`, `git reset`, `git checkout`, `git merge`, `git rebase`, `git stash` or `git tag`. Read-only commands (`git status`, `git diff`, `git log`) are allowed. At the end of each phase, list the changed files and **suggest** a Conventional Commit message for the developer to use.
- `README.md` (pt-BR) already exists. At the end of each phase, check the corresponding Roadmap item and keep commands, ports and service names in sync with the real implementation. Never describe a feature as done before it works.

## Product identity
- Product name: **Talos** — shown in the UI as "F.O.G.O. Talos". Code namespace root: `Talos`.
- Concept: in Greek mythology, Talos was the bronze giant that patrolled Crete three times a day, guarding it without rest. Here, Talos is the tireless guardian of PROMETHEUS's city: every IoT device is part of its patrol.
- Tagline (pt-BR, for UI header, README and video): "Talos — o guardião que nunca para de vigiar a cidade."
- Docker service and container names: `talos-api`, `talos-web`, `talos-postgres`, `talos-redis`. Database name: `talos`.

## Project goal
Urban mobility and safety management platform for a city, built for the DSIN "Desafio F.O.G.O." (deadline: 2026-10-27 23:59). Three parts:
1. **Smart Inventory** — register IoT traffic/safety equipment and field agents.
2. **Operations Center** — real-time map dashboard with KPIs, filters, alerts and regional overview.
3. **Predictive City** — risk index, reinforcement recommendations, anomaly detection and availability trends, always explaining *what* is predicted, *which data* was used and *how it helps the decision*.

Evaluation values: solving the problem, creativity, effectiveness, performance, technical quality, user experience, and **running on the first try**. Prefer simple and polished over complex and fragile.

## Tech stack
- .NET 10 (LTS), C# latest
- ASP.NET Core **Minimal API** + OpenAPI + **Scalar** UI
- **Blazor Web App** — Interactive Server render mode — **MudBlazor**
- **Leaflet + OpenStreetMap** via JS interop (+ `Leaflet.markercluster`, `Leaflet.heat`)
- **SignalR** for real-time updates
- **PostgreSQL 17** + EF Core (Npgsql) + `EFCore.NamingConventions` (snake_case)
- **Redis 7** + StackExchange.Redis (cache, GEO index, pub/sub)
- FluentValidation, Bogus (pt_BR locale) for seed data
- xUnit + FluentAssertions-style assertions (use Shouldly or plain Assert if licensing is a concern)
- Docker + docker-compose
- **Do NOT use MediatR or AutoMapper** (licensing + unnecessary). Use plain handlers and explicit mapping methods.

## Solution structure
```
Talos.sln
├── Directory.Build.props          # Nullable, ImplicitUsings, LangVersion, analyzers
├── Directory.Packages.props       # Central package management
├── .editorconfig                  # Naming + style rules (enforced)
├── docker-compose.yml
├── README.md
├── docs/AI_USAGE.md               # Declares AI tools used (Claude / Claude Code) and how
├── src/
│   ├── Talos.Domain/
│   ├── Talos.Contracts/            # Input/Output records shared by Application, Api and Web
│   ├── Talos.Application/
│   ├── Talos.Infrastructure/
│   ├── Talos.Api/
│   └── Talos.Web/
└── tests/
    ├── Talos.Domain.Tests/
    └── Talos.Application.Tests/
```

### Dependency rules (Clean Architecture)
- `Domain` → no project references, no infrastructure packages.
- `Contracts` → no project references (pure records/enums for transport).
- `Application` → Domain, Contracts.
- `Infrastructure` → Application, Domain.
- `Api` → Application, Infrastructure, Contracts.
- `Web` → Contracts only (talks to the Api over HTTP + SignalR).

## Naming and code style
- Types, methods, properties, records, enums: **PascalCase**.
- Local variables and parameters: **camelCase**.
- Private fields: **_camelCase**.
- Interfaces: `I` prefix.
- Async methods end with `Async` and accept `CancellationToken cancellationToken`.
- JSON payloads: **camelCase**. Routes: lowercase, kebab-case (`/api/equipment/{equipmentId}/report-failure`).
- Database: **snake_case** tables and columns.
- JavaScript: camelCase.
- **No abbreviations.** Names must be self-explanatory: `equipmentRepository` (not `repo`), `cancellationToken` (not `ct`), `serialNumber` (not `sn`), `maintenanceDate` (not `mDate`), `index` (not `i`) except trivial lambdas like `equipment => equipment.Id`.
- File-scoped namespaces, one public type per file, `sealed` by default, records for Inputs/Outputs and Value Objects.
- Methods short and single-purpose (SOLID, Clean Code). No business rules in endpoints or Blazor components.
- Comments only when explaining *why*, never *what*.

## Input / Output pattern (mandatory)
Every use case has exactly one Input, one Output and one UseCase class.

```
Talos.Contracts/Equipment/CreateEquipment/CreateEquipmentInput.cs
Talos.Contracts/Equipment/CreateEquipment/CreateEquipmentOutput.cs
Talos.Application/Equipment/CreateEquipment/CreateEquipmentUseCase.cs
Talos.Application/Equipment/CreateEquipment/CreateEquipmentInputValidator.cs
```

```csharp
public interface IUseCase<TInput, TOutput>
{
    Task<Result<TOutput>> ExecuteAsync(TInput input, CancellationToken cancellationToken);
}
```

- Queries also follow the pattern: `ListEquipmentInput` (filters + paging) → `ListEquipmentOutput`.
- Use cases with no input use an empty record (e.g. `GetDashboardSummaryInput`).
- `Result<T>` with `Error(Code, Message, ErrorType)` where `ErrorType` = Validation, NotFound, Conflict, BusinessRule.
- Api maps `Result` to `TypedResults` / `ProblemDetails` through a single extension (`ResultExtensions.ToHttpResult()`).
- Mapping Domain ↔ Output is explicit (`EquipmentOutputMapper.ToOutput(equipment)`), no reflection mappers.
- Use cases are registered in DI by convention (assembly scan) in `Application/DependencyInjection.cs`.

## Domain model

### Common
`Entity<TId>`, `AggregateRoot<TId>` (holds domain events), `ValueObject` (records), `IDomainEvent`, `DomainException`.

### Equipment aggregate
- `EquipmentId`, `SerialNumber` (unique), `Brand`, `Model`, `EquipmentType`, `ResponsibleAgency`
- `InstallationLocation` (Value Object): `ZipCode`, `Street`, `Reference` (free text: "número 181", "KM 203", "S/N", "em frente ao Mercado do Tio"), `Neighborhood`, `City`, `State`, `Region`, `GeoCoordinate` (Latitude, Longitude with range validation)
- `EquipmentStatus`: NotInstalled, Operational, Malfunctioning, UnderMaintenance, Deactivated, Stolen
- `MaintenancePlan` (Value Object): `MaintenanceType` (None, Immediate, Scheduled, Periodic), `NextMaintenanceDate`, `PeriodicityInDays`
- `Specifications`: type-specific attributes stored as **JSONB** (`Dictionary<string, JsonElement>` or equivalent), validated against `EquipmentSpecificationCatalog`
- `LastTelemetryReceivedAt`
- Behaviors (rules live here): `Install()`, `ReportMalfunction(reason)`, `StartMaintenance()`, `CompleteMaintenance()`, `ReportStolen()`, `Deactivate()`, `ScheduleMaintenance(date)`, `RegisterTelemetry(receivedAt)`, `IsMaintenanceDueWithin(days)`.
- Invalid status transitions throw `DomainException` (e.g. a Stolen equipment cannot become Operational without being reinstalled).
- Domain events: `EquipmentBecameUnavailable`, `EquipmentReportedStolen`, `EquipmentMaintenanceCompleted`.

### EquipmentType (enum) and specification catalog
SpeedCamera, RedLightCamera, ProhibitedTurnCamera, SurveillanceCamera, TruckScale, AutonomousParkingEnforcementVehicle, AutomatedGate, Taxi, MotorcycleTaxi, SchoolTransport, ParkingKiosk, ElectricScooter, TrafficAgentMotorcycle, MunicipalGuardVehicle, MilitaryPoliceVehicle, Other.

`EquipmentSpecificationCatalog` defines, per type, a list of `SpecificationField(Key, DisplayLabelPtBr, DataType, IsRequired, AllowedValues?)`. Examples:
- SpeedCamera: `maximumSpeedKmh` (int, required), `lanesMonitored` (int)
- SurveillanceCamera: `hasNightVision` (bool), `resolution` (options: HD, FullHD, 4K), `isColor` (bool), `hasPanTiltZoom` (bool)
- TruckScale: `maximumWeightTons` (decimal)
- ElectricScooter: `batteryCapacityWh` (int), `isShared` (bool)
- Vehicles: `licensePlate` (string, required), `capacity` (int)

The catalog is exposed by the Api (`GET /api/equipment-types`) so the Web renders the **dynamic form** per type.

### Agent aggregate
- `AgentId`, `FullName`, `RegistrationNumber` (unique), `AgentRole` (TrafficAgent, MunicipalGuard, PoliceOfficer), `ResponsibleAgency`, `AgentStatus` (OnDuty, Responding, OnBreak, OffDuty), `CurrentPosition` (GeoCoordinate), `LastPositionUpdatedAt`, `AssignedRegion`
- Behaviors: `StartShift()`, `EndShift()`, `UpdatePosition(coordinate, timestamp)`, `RespondTo(occurrenceId)`.

### Supporting entities
- `Occurrence`: `OccurrenceType` (TrafficAccident, Congestion, EquipmentFailure, Vandalism, Theft, IllegalParking, PublicDisorder), `Severity` (Low, Medium, High, Critical), `OccurredAt`, `GeoCoordinate`, `Neighborhood`, `Region`. Feeds the predictive module.
- `Alert`: `AlertSeverity` (Information, Warning, Critical), `AlertSource` (Equipment, Agent, Prediction), `SourceId`, `Message` (pt-BR), `CreatedAt`, `AcknowledgedAt`.
- `ResponsibleAgency` (enum): TrafficDepartment, MunicipalGuard, MilitaryPolice, PublicTransportAuthority, PrivateConcessionaire.

## Application layer
Organized by feature folder: `Equipment/`, `Agents/`, `Operations/`, `Alerts/`, `Predictions/`.

Abstractions (implemented in Infrastructure):
`IEquipmentRepository`, `IAgentRepository`, `IOccurrenceRepository`, `IAlertRepository`, `IUnitOfWork`, `ICacheService`, `IAgentLocationIndex` (Redis GEO), `IRealtimeNotifier`, `IDateTimeProvider`.

Domain events are dispatched after `SaveChangesAsync` to `IDomainEventHandler<TEvent>` implementations (e.g. create an `Alert` and notify in real time).

## Infrastructure layer
- `Persistence/`: `TalosDbContext`, entity configurations (Value Objects as owned types, Specifications as `jsonb`), migrations, repositories, `UnitOfWork` with domain event dispatching.
- `Caching/`: `RedisCacheService` (dashboard summary snapshot, short TTL ~10s, invalidated on writes), `RedisAgentLocationIndex` (`GEOADD` / `GEOSEARCH` → nearest agents to a coordinate).
- `Simulation/`: `TelemetrySimulatorBackgroundService` — every few seconds moves on-duty agents, registers telemetry, and occasionally changes equipment status (malfunction, stolen, maintenance completed). Configurable via `appsettings` (`Simulation:Enabled`, `Simulation:IntervalInSeconds`).
- `Seeding/`: `DatabaseSeeder` — runs on startup when the database is empty.

### Seed data (must produce meaningful patterns)
- City: **Marília-SP**. 8–10 neighborhoods with approximate real coordinates, grouped into regions (Centro, Norte, Sul, Leste, Oeste).
- ~120 equipment items of all types, realistic status mix (~80% operational), some with maintenance overdue or due within 7 days.
- ~30 agents across the three roles and agencies.
- **90 days of occurrences** with deliberate patterns: rush hours (07–09h, 17–19h), Friday/Saturday nights, 2–3 "hot" neighborhoods, accidents concentrated near speed cameras that went offline. These patterns are what the predictive module must discover.
- Daily equipment availability history (for trend analysis).

## Api
- Endpoints grouped per feature as extension methods: `app.MapEquipmentEndpoints()`, `MapAgentEndpoints()`, `MapOperationsEndpoints()`, `MapAlertEndpoints()`, `MapPredictionEndpoints()`.
- Endpoints are thin: bind Input → call UseCase → `ToHttpResult()`.
- `OperationsHub` (SignalR) at `/hubs/operations` broadcasting: `EquipmentStatusChanged`, `AgentPositionUpdated`, `AlertRaised`, `DashboardSummaryUpdated`.
- Global exception handler → ProblemDetails. Health checks at `/health` (Postgres + Redis). OpenAPI + Scalar at `/scalar`.
- On startup: apply migrations, then seed.

Main endpoints:
- `GET/POST /api/equipment`, `GET/PUT /api/equipment/{equipmentId}`, `POST /api/equipment/{equipmentId}/report-malfunction|report-stolen|start-maintenance|complete-maintenance|deactivate`
- `GET /api/equipment-types` (catalog)
- `GET/POST /api/agents`, `GET/PUT /api/agents/{agentId}`, `GET /api/agents/nearest?latitude=&longitude=&count=`
- `GET /api/operations/summary` (KPIs), `GET /api/operations/map-items` (filters: type, status, neighborhood, region, agency), `GET /api/operations/regions` (health per region)
- `GET /api/alerts`, `POST /api/alerts/{alertId}/acknowledge`
- `GET /api/predictions/risk-index`, `/reinforcement-recommendations`, `/anomalies`, `/availability-trend`

## Web (Blazor, UI in pt-BR)
- Typed `HttpClient` api clients (`EquipmentApiClient`, ...) using Contracts records.
- `OperationsHubClient` wrapping the SignalR connection.
- `wwwroot/js/operationsMap.js` with small functions (`initializeMap`, `upsertMarker`, `removeMarker`, `setHeatmap`, `focusOn`) called through one `OperationsMap.razor` component.
- Pages:
  - `/` **Central de Operações**: map + KPI cards + filters + alert panel + regional status.
  - `/equipamentos` list with filters, `/equipamentos/novo` and `/equipamentos/{id}` with **dynamic specification form**.
  - `/agentes` list and form.
  - `/previsoes` risk heatmap (neighborhood × hour), reinforcement recommendations, anomalies, availability trend chart. Each card explains: **O que é previsto / Dados utilizados / Como ajuda na decisão**.
- Distinct icon per equipment category and per agent role; color by status (green operational, amber maintenance/due, red malfunction/stolen, gray deactivated/not installed).

## Predictive module (explainable first)
1. **RiskIndexCalculator** — for each neighborhood × day of week × hour: weighted occurrence frequency (severity weights, recency decay favoring last 30 days), normalized 0–100 → RiskLevel (Low, Moderate, High, Critical).
2. **ReinforcementRecommendationService** — expected occurrences in the next hours per region vs. agents on duty there → deficit and suggested number of agents to move, plus nearest available agents (Redis GEO).
3. **AnomalyDetectionService** — equipment silent beyond expected telemetry interval; occurrence counts deviating > 2 standard deviations from the historical baseline for that slot; optional ML.NET spike detection if time allows.
4. **AvailabilityTrendService** — daily availability % over 30 days + linear regression slope → projected availability and alert if trending down.

Every prediction Output includes an `Explanation` object: `WhatIsPredicted`, `DataUsed`, `HowItHelps` (pt-BR text).

## Infrastructure / running
- `docker-compose.yml` with `postgres`, `redis`, `api`, `web`, healthchecks and `depends_on: condition: service_healthy`. Single command: `docker compose up --build`.
- Also must work with `dotnet run` (infra only from compose: `docker compose up postgres redis`).
- Connection strings via environment variables / appsettings, no secrets committed.
- Ports: Web 8080, Api 8081, Postgres 5432, Redis 6379.

## Quality
- `Directory.Build.props`: `Nullable=enable`, `ImplicitUsings=enable`, analyzers on.
- `.editorconfig` enforcing the naming rules above.
- Unit tests at least for: equipment status transitions, maintenance due logic, specification validation, risk index calculation.
- Commit messages follow Conventional Commits (`feat:`, `fix:`, `chore:`, `test:`, `docs:`), suggested by Claude Code and executed only by the developer.

## Delivery phases (stop after each one)
0. **Scaffolding** — `global.json` pinning SDK `10.0.400` with `rollForward: latestFeature` (the machine also has SDK 8), `dotnet new gitignore`, solution, projects, references, Directory.Build.props, Directory.Packages.props, .editorconfig, docker-compose (postgres + redis only), empty Program.cs files building.
1. **Domain** — entities, value objects, enums, catalog, domain events, rules + domain tests.
2. **Persistence + Seed** — DbContext, configurations, migrations, repositories, UnitOfWork, seeder with patterns.
3. **Application + Api (Part 1)** — Contracts, use cases, validators, Result, endpoints, Scalar.
4. **Web (Part 1)** — layout, MudBlazor, equipment and agent CRUD with dynamic form.
5. **Real time (Part 2 backend)** — Redis cache/GEO, simulator, SignalR hub, operations endpoints, alerts.
6. **Operations Center (Part 2 frontend)** — map, markers, filters, KPIs, alerts, regional status.
7. **Predictions (Part 3)** — services, endpoints, `/previsoes` page with heatmap and explanations.
8. **Delivery** — full docker-compose, README (how to run, architecture, decisions, screenshots), docs/AI_USAGE.md, cleanup.
