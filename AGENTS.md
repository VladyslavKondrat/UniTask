# AGENTS.md

## 1. Project Overview
`UniTask` is a modular monolith built with .NET 10 and Domain-Driven Design (DDD).
The solution is organized into Bounded Contexts, each containing four layers: `Domain`, `Application`, `Infrastructure`, and `Presentation`.

## 2. Build, Test, and Run Commands
Always run these commands to verify changes before finishing a task:
- **Restore & Build:** `dotnet build --configuration Release`
- **Run Unit Tests:** `dotnet test tests/UnitTests --no-build`
- **Run Integration Tests:** `dotnet test tests/IntegrationTests`
- **Format Code:** `dotnet format --verify-no-changes`
- **Add EF Core Migration (example for a specific context):**
  `dotnet ef migrations add <MigrationName> --project src/Modules/<Context>/<Context>.Infrastructure --startup-project src/Host`

## 3. Architecture & Domain Boundaries (Strict Rules)
- **No Cross-Context References:** Modules MUST NOT reference another module's `Domain`, `Application`, or `Infrastructure` projects directly. Communicate only via public contracts/facades or domain events.
- **Database Isolation:** Each Bounded Context uses its own database schema and `DbContext`. Cross-schema `FOREIGN KEY` constraints and SQL `JOIN`s between different context schemas are strictly prohibited.
- **Module Microarchitecture:**
  - **Core Subdomains:** Use Rich Domain Model (Aggregates, Value Objects, Domain Events) with Clean/Hexagonal Architecture. Keep domain entities free of EF Core or serialization attributes.
  - **Supporting / Generic Subdomains:** Keep simple (Transaction Script / Active Record). Do not overengineer with unnecessary aggregates or ports.
- **Anti-Corruption Layer (ACL):** External integrations with incompatible or legacy models must be isolated behind a port interface in `Application/Ports` and translated inside `Infrastructure/Acl`. Never leak external DTOs or error codes into `Domain` or `Application`.
- **Shared Kernel:** Only small, immutable Value Objects (e.g., `Money`, `Currency`) belong in `SharedKernel`. Never add entities, aggregates, or external package dependencies there.

## 4. Coding Conventions (.NET / C#)
- Target `.NET 10` with nullable reference types enabled (`<Nullable>enable</Nullable>`).
- Use `file-scoped namespaces`, primary constructors where appropriate, and `sealed` by default for classes and records.
- Prefer strongly-typed IDs and Value Objects over raw primitives (`Guid`, `string`, `decimal`) in Core domains.
- Domain invariants must be enforced inside Aggregate methods (never rely solely on controller/DTO validation).
- Throw domain-specific exceptions deriving from the context's base `DomainException` rather than generic `Exception`.

## 5. Testing Guidelines
- Follow TDD practices for Core domain logic: write/update `xUnit` unit tests covering state transitions and invariants before or alongside implementation.
- Unit tests for `Domain` MUST NOT require a database, `DbContext`, or network access.
- Name tests clearly using the convention: `MethodName_StateUnderTest_ExpectedBehavior`.

## 6. Documentation & Git Workflow
- Architectural decisions must be recorded in `docs/architecture/adr/` (see existing ADRs before proposing structural changes).
- Keep `docs/architecture/` diagrams (Mermaid) in sync if Bounded Context boundaries or integration patterns change.
- Do not modify existing EF Core migrations that have already been committed; create a new migration instead.

## 7. Project Overview & Architecture Sources
`UniTask` is a .NET 10 modular monolith designed using Domain-Driven Design (DDD).
Before implementing or modifying domain logic, aggregates, state transitions, or integrations, ALWAYS consult the source-of-truth architectural documents:
- `docs/architecture/aggregates-and-invariants.md` — Aggregates, Value Objects, Ubiquitous Language, and domain invariants.
- `docs/architecture/context-map.md` — Subdomain classification (Core / Supporting / Generic), Bounded Context Map, integration patterns, and ACL boundaries.
- `docs/architecture/stateDiagram-applicationLifecycle.md` — Valid lifecycle states and transitions for application fulfillment.

## 8. Repository Structure & Gitignore Rules
The codebase mirrors Bounded Contexts across `src/Services` and `tests` so every module has a clear 1-to-1 test suite:

```text
├── docs/
│   └── architecture/
│       ├── aggregates-and-invariants.md
│       ├── context-map.md
│       └── stateDiagram-applicationLifecycle.md
├── src/
│   └── Services/
│       ├── ApplicationFulfillment/   # Bounded Context 1
│       ├── TaskManagement/           # Bounded Context 2
│       └── UserIdentity/             # Bounded Context 3
├── tests/
│   ├── ApplicationFulfillment/       # Tests mirroring src/Services/ApplicationFulfillment
│   ├── TaskManagement/               # Tests mirroring src/Services/TaskManagement
│   └── UserIdentity/                 # Tests mirroring src/Services/UserIdentity
├── .gitignore
└── AGENTS.md
```

## 9. Testing Conventions (xUnit Only)
- **Framework Rule:** All unit and integration tests MUST use **xUnit** exclusively. Do NOT introduce NUnit, MSTest, or other test frameworks.
- **Test Naming Convention:** Name every test method clearly using the format:
  `Check_<SubjectOrAction>_<ConditionOrInput>_Should<ExpectedResult>`
  (for example: `Check_Card_Digits_Amount_ShouldReturnCorrectResult`).
- **Mirror Test Structure:** Place tests under `tests/<Context>/...` to mirror `src/Services/<Context>/...`[cite: 7].
- **Isolated Domain Tests:** Keep domain unit tests fast and purely in-memory, with no database, `DbContext`, or network dependencies.

A task is considered complete ONLY when `dotnet test` reports `Failed: 0`.
If tests are failed while testing the modules, test can't be changed (removed or modified)
NEVER invent your own names for domain classes, exceptions, or Value Objects if they are defined in the files above.

## 10. Agent Workflow Protocol (Plan -> Red -> Green Loop)
Do not generate entire modules in a single step. Work in small iterations:
1. **Plan and Contract:** first propose port interfaces (`.Application/Ports`), CQRS contracts, and domain exceptions (`.Domain/Exceptions`).
2. **RED Phase (TDD):** based on Given-When-Then scenarios or the ACL table in `.md` files, generate unit tests first.
3. **GREEN Phase:** generate minimal, clean production code that makes the tests pass.
4. **Verification:** ensure `CleanArchitectureGuardTests` (`NetArchTest.Rules`) pass.

## 11. Layer Structure and Dependency Rule (Clean Architecture + CQRS)
Each Bounded Context in `src/Services/<Context>/` consists of 4 projects:
- `<System>.<Context>.Domain` – business core. ZERO external NuGet packages (no `Microsoft.EntityFrameworkCore*`, `Newtonsoft.Json`, etc.) and zero references to other layers.
- `<System>.<Context>.Application` – CQRS Commands and Queries (`Features/`), repository interfaces (`I*Repository`), and outbound port interfaces (`Ports/I*Port`). Depends exclusively on `.Domain`.
- `<System>.<Context>.Infrastructure` – technical implementations: `DbContext`, `IEntityTypeConfiguration<T>`, `Sql*Repository`, and the Anti-Corruption Layer (`Acl/`). All implementation classes in `.Infrastructure` (except `DependencyInjection`) MUST be marked `internal sealed`.
- `<System>.<Context>.Presentation` – ASP.NET Core Web API controllers delegating to CQRS handlers, HTTP request contracts, and `Program.cs`.

## 12. Domain Model Rules (.Domain)
- NEVER use public setters (`public ... { get; set; }`) in entities, aggregates, or value objects. All state properties must use `private set` or `{ get; }`.
- Store internal aggregate collections in a private field `private readonly List<T> _items = new();` and expose them externally solely as `IReadOnlyCollection<T>`.
- Add a parameterless `private` constructor to entities and aggregates for ORM materialization.
- NEVER use database attributes (`[Key]`, `[Table]`, `[Column]`, `[ForeignKey]`) inside `.Domain`.
- Signal business invariant violations by throwing a dedicated exception inheriting from `DomainException` (with a unique `Code` in `snake_case`).

## 13. Anti-Corruption Layer (ACL) and EF Core Rules (.Infrastructure)
- Place the domain port (`I*Port`) in `.Application/Ports/`. Its signature may use ONLY types from our domain model.
- Keep foreign data structures (`*_DTO` from external systems, error codes, text formats) inside `.Infrastructure/Acl/` as `internal sealed` types.
- Separate ACL responsibilities into 3 `internal sealed` classes: `*Translator` (pure transformation with `CultureInfo.InvariantCulture`, zero network I/O), `*Adapter` (I/O communication), and `*Acl` (facade implementing `I*Port`).
- In EF Core repositories, methods loading an aggregate for modification (`GetByIdAsync`) must NEVER use `.AsNoTracking()` and must include child collections (`.Include(...)`). Use `.AsNoTracking()` exclusively in read-only Query handlers.
