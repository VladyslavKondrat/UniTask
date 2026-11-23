# AGENTS.md – Architectural Constitution and Workflow Rules for AI Agents

## 1. Map of Architectural Documentation (Single Source of Truth)
Before generating or modifying any C# code, ALWAYS check the team's design specification:
- `docs/architecture/aggregates-and-invariants.md` – Ubiquitous Language glossary, Value Objects, strongly-typed Ids, business invariants, and Given-When-Then scenarios.
- `docs/architecture/stateDiagram-*.md` – state machine of the main aggregate and operations forbidden in specific states.
- `docs/architecture/context-map.md` – Bounded Contexts division, relationship patterns, and exact port + mapping table specification for the Anti-Corruption Layer (ACL).

NEVER invent your own names for domain classes, exceptions, or Value Objects if they are defined in the files above.

## 2. Build and Verification Commands
After every code change, the agent must verify the solution using:
- Build entire solution: `dotnet build --no-restore`
- Run all unit and architecture tests: `dotnet test --no-build`
- Run architectural guard tests only: `dotnet test --filter "FullyQualifiedName~CleanArchitectureGuardTests"`

A task is considered complete ONLY when `dotnet test` reports `Failed: 0`.

## 3. Agent Workflow Protocol (Plan -> Red -> Green Loop)
Do not generate entire modules in a single step. Work in small iterations:
1. **Plan and Contract:** first propose port interfaces (`.Application/Ports`), CQRS contracts, and domain exceptions (`.Domain/Exceptions`).
2. **RED Phase (TDD):** based on Given-When-Then scenarios or the ACL table in `.md` files, generate unit tests first.
3. **GREEN Phase:** generate minimal, clean production code that makes the tests pass.
4. **Verification:** ensure `CleanArchitectureGuardTests` (`NetArchTest.Rules`) pass.

## 4. Layer Structure and Dependency Rule (Clean Architecture + CQRS)
Each Bounded Context in `src/Services/<Context>/` consists of 4 projects:
- `<System>.<Context>.Domain` – business core. ZERO external NuGet packages (no `Microsoft.EntityFrameworkCore*`, `Newtonsoft.Json`, etc.) and zero references to other layers.
- `<System>.<Context>.Application` – CQRS Commands and Queries (`Features/`), repository interfaces (`I*Repository`), and outbound port interfaces (`Ports/I*Port`). Depends exclusively on `.Domain`.
- `<System>.<Context>.Infrastructure` – technical implementations: `DbContext`, `IEntityTypeConfiguration<T>`, `Sql*Repository`, and the Anti-Corruption Layer (`Acl/`). All implementation classes in `.Infrastructure` (except `DependencyInjection`) MUST be marked `internal sealed`.
- `<System>.<Context>.Presentation` – ASP.NET Core Web API controllers delegating to CQRS handlers, HTTP request contracts, and `Program.cs`.

## 5. Domain Model Rules (.Domain)
- NEVER use public setters (`public ... { get; set; }`) in entities, aggregates, or value objects. All state properties must use `private set` or `{ get; }`.
- Store internal aggregate collections in a private field `private readonly List<T> _items = new();` and expose them externally solely as `IReadOnlyCollection<T>`.
- Add a parameterless `private` constructor to entities and aggregates for ORM materialization.
- NEVER use database attributes (`[Key]`, `[Table]`, `[Column]`, `[ForeignKey]`) inside `.Domain`.
- Signal business invariant violations by throwing a dedicated exception inheriting from `DomainException` (with a unique `Code` in `snake_case`).

## 6. Anti-Corruption Layer (ACL) and EF Core Rules (.Infrastructure)
- Place the domain port (`I*Port`) in `.Application/Ports/`. Its signature may use ONLY types from our domain model.
- Keep foreign data structures (`*_DTO` from external systems, error codes, text formats) inside `.Infrastructure/Acl/` as `internal sealed` types.
- Separate ACL responsibilities into 3 `internal sealed` classes: `*Translator` (pure transformation with `CultureInfo.InvariantCulture`, zero network I/O), `*Adapter` (I/O communication), and `*Acl` (facade implementing `I*Port`).
- In EF Core repositories, methods loading an aggregate for modification (`GetByIdAsync`) must NEVER use `.AsNoTracking()` and must include child collections (`.Include(...)`). Use `.AsNoTracking()` exclusively in read-only Query handlers.

## 7. Unit Testing Rules (xUnit)
- Follow the `Arrange – Act – Assert` (AAA) structure and clear naming `Method_WhenCondition_ShouldExpectedResult`.
- In domain and ACL translator tests, test real objects without mocking libraries (`Moq`). NEVER write tautological tests (testing a configured mock).