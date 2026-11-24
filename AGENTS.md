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