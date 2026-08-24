# VeloCarbon Parallel Delivery Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver the VeloCarbon MVP through parallel backend and frontend work that integrates cleanly through stable, tested contracts.

**Architecture:** A four-layer WPF solution separates UI, application use cases, domain rules, and EF Core infrastructure. The architect lands contracts first; backend implements those contracts while frontend binds against mock implementations, then swaps to real services through dependency injection during integration.

**Tech Stack:** .NET 8, C# 12, WPF, CommunityToolkit.Mvvm, EF Core 8, Npgsql Entity Framework Core PostgreSQL provider, PostgreSQL, xUnit.

**Spec:** `docs/PRD-VeloCarbon.md`

## Global Constraints

- Target Windows desktop with .NET 8 and PostgreSQL local.
- The core calculation and scenario workflow must work without network access.
- Only mobility darat and electricity pribadi/kos belong to the MVP.
- Store every emission factor with unit, source, version, effective date, and active status.
- WPF Views and ViewModels must not access `DbContext` or external HTTP clients directly.
- Every change is developed in its role branch and merged through a reviewed Pull Request.

---

## Branch and Integration Sequence

| Sequence | Owner branch | Deliverable | Merge condition |
| --- | --- | --- | --- |
| 1 | `539398` | Solution structure, contracts, entities, ERD, seed specification | Compiles and contract tests pass. |
| 2A | `534432` | EF Core repositories, calculation service, scenario service, tests | Implements the contracts from `main`; tests pass. |
| 2B | `24542344TK60216` | Views, ViewModels, charts, mock services, ViewModel tests | Compiles against the same contracts and mock tests pass. |
| 3 | `539398` | DI wiring, real-service integration, end-to-end smoke tests | Full input-to-dashboard-to-scenario path passes. |
| 4 | all branches via reviewed fixes | Demo data, errors, README run instructions | Clean build and repeatable local demo. |

### Task 1: Architecture Baseline and Contracts

**Owner:** Software Architect (`539398`)

**Files:**
- Create: `src/VeloCarbon.Domain/VeloCarbon.Domain.csproj`
- Create: `src/VeloCarbon.Application/VeloCarbon.Application.csproj`
- Create: `src/VeloCarbon.Application/Contracts/ActivityInput.cs`
- Create: `src/VeloCarbon.Application/Contracts/EmissionSummary.cs`
- Create: `src/VeloCarbon.Application/Contracts/ScenarioInput.cs`
- Create: `src/VeloCarbon.Application/Interfaces/IEmissionCalculationService.cs`
- Create: `src/VeloCarbon.Application/Interfaces/IScenarioSimulationService.cs`
- Create: `docs/architecture/ERD.md`

**Produces:** contracts consumed by both `534432` and `24542344TK60216`.

- [ ] Define immutable record contracts with unambiguous units.

```csharp
public sealed record ActivityInput(
    DateOnly Date,
    string Category,
    decimal ActivityValue,
    string Unit,
    Guid EmissionFactorId);

public sealed record EmissionSummary(
    decimal TotalKgCo2e,
    IReadOnlyDictionary<string, decimal> KgCo2eByCategory);
```

- [ ] Define service interfaces before either implementation begins.

```csharp
public interface IEmissionCalculationService
{
    Task<decimal> CalculateKgCo2eAsync(ActivityInput input, CancellationToken cancellationToken);
    Task<EmissionSummary> GetSummaryAsync(DateOnly start, DateOnly end, CancellationToken cancellationToken);
}

public interface IScenarioSimulationService
{
    Task<ScenarioResult> SimulateAsync(ScenarioInput input, CancellationToken cancellationToken);
}
```

- [ ] Create the ERD including `UserProfile`, `Vehicle`, `ActivityRecord`, `ElectricityUsage`, `EmissionFactor`, `EmissionCalculation`, `Scenario`, `ScenarioChange`, and `ScenarioResult`.
- [ ] Build the solution and commit with `docs: add VeloCarbon architecture contracts`.
- [ ] Open a Pull Request from `539398` to `main`; backend and frontend branch only after this is merged.

### Task 2: Backend Persistence and Factor Data

**Owner:** Backend Developer (`534432`)

**Files:**
- Create: `src/VeloCarbon.Infrastructure/VeloCarbon.Infrastructure.csproj`
- Create: `src/VeloCarbon.Infrastructure/Data/VeloCarbonDbContext.cs`
- Create: `src/VeloCarbon.Infrastructure/Data/Seed/EmissionFactorSeeder.cs`
- Create: `src/VeloCarbon.Infrastructure/Repositories/EmissionFactorRepository.cs`
- Create: `tests/VeloCarbon.Infrastructure.Tests/EmissionFactorRepositoryTests.cs`

**Consumes:** `ActivityInput` and domain entities from Task 1.

**Produces:** persisted, versioned emission factors for Task 3 and Task 6.

- [ ] Write a failing repository test that persists a factor with `Unit`, `Source`, `Version`, `EffectiveFrom`, and `IsActive`.
- [ ] Implement the entity configuration with a unique index on `(Category, Subcategory, Version, EffectiveFrom)`.
- [ ] Implement `GetActiveAsync(category, unit, date)` to return a compatible active factor or a typed validation error.
- [ ] Seed one factor for each MVP category: motorcycle/car/bus transport and electricity kWh.
- [ ] Run the infrastructure tests with PostgreSQL test configuration and commit with `feat: add emission factor persistence`.

### Task 3: Backend Calculation and Scenario Services

**Owner:** Backend Developer (`534432`)

**Files:**
- Create: `src/VeloCarbon.Application/Services/EmissionCalculationService.cs`
- Create: `src/VeloCarbon.Application/Services/ScenarioSimulationService.cs`
- Create: `tests/VeloCarbon.Application.Tests/EmissionCalculationServiceTests.cs`
- Create: `tests/VeloCarbon.Application.Tests/ScenarioSimulationServiceTests.cs`

**Consumes:** interfaces and DTOs from Task 1, factor repository from Task 2.

**Produces:** concrete services registered by Task 6.

- [ ] Write `CalculateKgCo2eAsync` tests for transport and electricity: `10 km x 0.2 kgCO2e/km` returns `2.0 kgCO2e`; a negative value fails validation.
- [ ] Implement calculation as `activityValue * factorValue`, preserving the factor ID and source in `EmissionCalculation`.
- [ ] Write scenario tests: a 10% electricity reduction from a 50 kgCO2e baseline returns 45 kgCO2e and 10% reduction.
- [ ] Return an unavailable percentage when the baseline is zero.
- [ ] Run application tests and commit with `feat: add emission calculation and scenario services`.

### Task 4: Frontend Shell and Mock Data Path

**Owner:** Frontend Developer (`24542344TK60216`)

**Files:**
- Create: `src/VeloCarbon.Desktop/VeloCarbon.Desktop.csproj`
- Create: `src/VeloCarbon.Desktop/ViewModels/DashboardViewModel.cs`
- Create: `src/VeloCarbon.Desktop/ViewModels/ActivityEntryViewModel.cs`
- Create: `src/VeloCarbon.Desktop/Services/MockEmissionCalculationService.cs`
- Create: `src/VeloCarbon.Desktop/Views/DashboardView.xaml`
- Create: `src/VeloCarbon.Desktop/Views/ActivityEntryView.xaml`
- Create: `tests/VeloCarbon.Desktop.Tests/DashboardViewModelTests.cs`

**Consumes:** `IEmissionCalculationService`, `IScenarioSimulationService`, and DTOs from Task 1.

**Produces:** a navigable UI that works before backend merge.

- [ ] Write a ViewModel test where a mock summary populates `TotalKgCo2e` and category rows.
- [ ] Implement `MockEmissionCalculationService` using fixed DTOs that satisfy the production interfaces.
- [ ] Bind `ActivityEntryViewModel` to date, category, numeric activity value, unit, and a save command.
- [ ] Reject empty category and values less than or equal to zero in the ViewModel before invoking a service.
- [ ] Build the desktop project and commit with `feat: add dashboard and activity entry UI`.

### Task 5: Frontend Scenario and Error States

**Owner:** Frontend Developer (`24542344TK60216`)

**Files:**
- Create: `src/VeloCarbon.Desktop/ViewModels/ScenarioViewModel.cs`
- Create: `src/VeloCarbon.Desktop/Views/ScenarioView.xaml`
- Modify: `src/VeloCarbon.Desktop/ViewModels/DashboardViewModel.cs`
- Create: `tests/VeloCarbon.Desktop.Tests/ScenarioViewModelTests.cs`

**Consumes:** `IScenarioSimulationService` and `ScenarioResult` from Task 1.

**Produces:** scenario comparison UI ready for real service wiring.

- [ ] Write a test that displays baseline, scenario, and reduction values returned by a mock scenario service.
- [ ] Write a test that sets a user-facing error message when the service throws a validation exception.
- [ ] Bind a percentage reduction input and show `ReductionKgCo2e` and `ReductionPercent` with units.
- [ ] Keep the empty, loading, success, and error states distinct in the ViewModel.
- [ ] Run desktop tests and commit with `feat: add scenario comparison UI`.

### Task 6: Integrate Real Services

**Owner:** Software Architect (`539398`), after Tasks 3–5 are merged to `main`

**Files:**
- Modify: `src/VeloCarbon.Desktop/App.xaml.cs`
- Create: `src/VeloCarbon.Infrastructure/DependencyInjection.cs`
- Modify: `src/VeloCarbon.Desktop/ViewModels/DashboardViewModel.cs`
- Modify: `src/VeloCarbon.Desktop/ViewModels/ScenarioViewModel.cs`
- Create: `tests/VeloCarbon.IntegrationTests/WorkflowSmokeTests.cs`

**Consumes:** concrete backend services and frontend ViewModels.

**Produces:** one executable local workflow using PostgreSQL rather than mocks.

- [ ] Register `IEmissionCalculationService` and `IScenarioSimulationService` with concrete implementations in the DI container.
- [ ] Register the mock services only for design-time data; do not register them in the production application startup path.
- [ ] Write a smoke test: create an electricity activity, calculate emissions, request a summary, run a 10% scenario, and assert non-negative results.
- [ ] Run all test projects, launch the WPF application against local seed data, and commit with `feat: integrate calculation workflow`.

### Task 7: Demo Readiness and Review

**Owner:** Software Architect with all members

**Files:**
- Modify: `README.md`
- Create: `docs/demo-checklist.md`
- Create: `docs/architecture/decision-log.md`

**Consumes:** the integrated application from Task 6.

**Produces:** repeatable documentation and a clean demonstration path.

- [ ] Document exact local prerequisites, database setup, migration command, seed command, and demo steps in `README.md`.
- [ ] Add a checklist that verifies activity entry, calculation, dashboard, scenario, validation error, and app restart persistence.
- [ ] Record architecture decisions: MVVM separation, versioned emission factors, offline-first calculation, and interface-first parallel development.
- [ ] Build and test from a fresh database, then commit with `docs: add demo and architecture guidance`.

## Plan Self-Review

- Spec coverage: Tasks 1–3 cover the domain, database, calculation, and scenario requirements; Tasks 4–5 cover UI, validation, dashboard, and error states; Task 6 covers integration; Task 7 covers demo documentation and architect evidence.
- Placeholder check: tasks name concrete files, interfaces, expected values, test behavior, and commit messages.
- Contract consistency: all frontend and backend work depends on the same `ActivityInput`, `EmissionSummary`, `ScenarioInput`, `ScenarioResult`, `IEmissionCalculationService`, and `IScenarioSimulationService` contracts from Task 1.
