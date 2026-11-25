# AGENTS.md

## 1. Project Overview & Spec-Anchored Workflow
`UniTask` is a modular monolith built with .NET 10 and Domain-Driven Design (DDD), organized into Bounded Contexts (`Domain`, `Application`, `Infrastructure`, and `Presentation`).

- **Spec-Anchored Development:** Every business or contract change starts by updating the source-of-truth specifications in Git, followed by tests, then code[cite: 13].
- **Hierarchy Rule (Closest Wins):** A local `src/Services/<Context>/AGENTS.md` specializes or overrides rules in this root file for that context.
- **Source-of-Truth Documents (Always consult before coding):**
  - `docs/architecture/aggregates-and-invariants.md` — Aggregates, Value Objects, Ubiquitous Language, and invariants.
  - `docs/architecture/context-map.md` — Subdomain types (Core / Supporting / Generic), Context Map, and ACL boundaries[cite: 4, 8, 15].
  - `docs/architecture/stateDiagram-applicationLifecycle.md` — Valid lifecycle states and transitions.
  - `docs/architecture/resilience-policies.md` — Polly v8 timeouts, retry budgets, circuit breaker, and bulkhead limits per integration.
  - `docs/architecture/adr/` — Architectural Decision Records[cite: 13, 14].
  - `docs/contracts/openapi-*.yaml` — Contract-First OpenAPI 3.1 specifications[cite: 13, 19, 20].

## 2. Repository Structure
```text
├── docs/
│   ├── architecture/
│   │   ├── adr/
│   │   ├── aggregates-and-invariants.md
│   │   ├── context-map.md
│   │   ├── resilience-policies.md
│   │   └── stateDiagram-applicationLifecycle.md
│   └── contracts/
│       └── openapi-*.yaml
├── src/
│   └── Services/
│       ├── ApplicationFulfillment/   # Bounded Context 1
│       ├── TaskManagement/           # Bounded Context 2
│       └── UserIdentity/             # Bounded Context 3
├── tests/
│   ├── ApplicationFulfillment/       # Mirrors src/Services/ApplicationFulfillment
│   ├── TaskManagement/               # Mirrors src/Services/TaskManagement
│   └── UserIdentity/                 # Mirrors src/Services/UserIdentity
├── .gitignore
└── AGENTS.md
```

## 3. Build, Test, and Run Commands
Run these commands to verify changes before finishing any task:
- **Restore & Build:** `dotnet build --configuration Release`
- **Run Tests:** `dotnet test UniTask.slnx --configuration Release --no-build`
- **Format Code:** `dotnet format --verify-no-changes`
- **EF Core Migrations:** Not yet configured. Once persistence is added, document a context-specific migration command; never modify committed migrations—create a new migration instead.

A task is complete ONLY when `dotnet test` reports `Failed: 0`[cite: 14]. Never remove or weaken existing failing tests to make a build pass.

## 4. Architecture & Domain Boundaries
- **No Cross-Context References:** Modules MUST NOT reference another module's `Domain`, `Application`, or `Infrastructure` projects[cite: 12, 14]. Communicate only via public contracts (`Open Host Service` + `Published Language` / `OpenAPI 3.1`) or domain events[cite: 8, 16, 19].
- **Database Isolation (`FF-05`):** Each Bounded Context owns its database schema, `DbContext`, and `__EFMigrationsHistory` table[cite: 16]. Cross-schema `FOREIGN KEY` constraints, SQL `JOIN`s, and direct `SELECT`/`UPDATE` on foreign tables are strictly prohibited[cite: 16, 17]. Reference external aggregates solely by strongly-typed ID and store immutable data snapshots where needed[cite: 17].
- **Module Microarchitecture:**
  - **Core Subdomains:** Rich Domain Model (Aggregates, Value Objects, Domain Events) + Clean/Hexagonal Architecture + CQRS[cite: 4, 5].
  - **Supporting / Generic Subdomains:** Keep simple (Transaction Script / Active Record / Vertical Slice); do not overengineer with unnecessary aggregates[cite: 4, 5].
- **Shared Kernel:** Only small, immutable, ultra-stable Value Objects (e.g., `Money`, `Currency`) belong in `SharedKernel`[cite: 8]. Never add entities, aggregates, shared contract DTOs, or external packages there[cite: 8, 22].

## 5. Layer Structure & Coding Conventions (.NET 10 / C#)
- **General C#:** Target `.NET 10` with `<Nullable>enable</Nullable>`, `file-scoped namespaces`, primary constructors where appropriate, and `sealed` by default for classes and records. Never invent names for domain classes, exceptions, or Value Objects already defined in `docs/`.
- **Clean Architecture & CQRS Layers per Bounded Context:**
  - `<System>.<Context>.Domain` — Pure business core. ZERO external NuGet packages (no EF Core, JSON, etc.) and zero references to other layers[cite: 3, 12].
  - `<System>.<Context>.Application` — CQRS Commands/Queries (`Features/`), repository interfaces (`I*Repository`), and outbound port interfaces (`Ports/I*Port`). Depends exclusively on `.Domain`.
  - `<System>.<Context>.Infrastructure` — `DbContext`, `IEntityTypeConfiguration<T>`, `Sql*Repository`, typed `IHttpClientFactory` + Polly v8 pipelines, and `Acl/`. Mark all implementation classes (except `DependencyInjection`) `internal sealed`[cite: 6, 9, 18, 40].
  - `<System>.<Context>.Presentation` — Thin Web API controllers delegating to CQRS handlers, global `IExceptionHandler` (RFC 9457), idempotency filters, HTTP request contracts, and `Program.cs`[cite: 29, 31, 33].

## 6. Domain Model Rules (`.Domain`)
- **Encapsulation (`FF-02`):** NEVER use `public set` on entities, aggregates, or value objects; use `private set` or `{ get; }`[cite: 12, 15].
- **Collections & Constructors:** Store collections in `private readonly List<T> _items = new();` exposed as `IReadOnlyCollection<T>`. Include a parameterless `private` constructor for ORM materialization.
- **Persistence Ignorance (`FF-01`):** NEVER use EF Core or serialization attributes (`[Key]`, `[Table]`, `[Column]`, `[ForeignKey]`) in `.Domain`[cite: 3, 12].
- **Types & Invariants:** Prefer strongly-typed IDs and Value Objects over raw primitives in Core domains[cite: 17, 21]. Enforce invariants inside Aggregate methods and signal violations via dedicated exceptions inheriting from `DomainException` with a unique `snake_case` `Code` (e.g., `cart.value_too_low`)[cite: 31, 33].

## 7. Persistence & Anti-Corruption Layer (`.Infrastructure`)
- **CQRS Read/Write Separation:**
  - **Write Path (`GetByIdAsync` for mutation):** Load the full aggregate graph with change tracking enabled (NEVER `.AsNoTracking()`) and `.Include(...)` child collections[cite: 6].
  - **Read Path (Query handlers):** Bypass domain aggregates completely, use `.AsNoTracking()`, and project directly in SQL (`.Select(x => new ...Dto(...))`) to flat read DTOs[cite: 6].
- **Anti-Corruption Layer (`FF-04`):** Place port interfaces (`I*Port`) in `.Domain/Ports/` or `.Application/Ports/` per `context-map.md`, using only our domain types in signatures[cite: 9]. Trap foreign DTOs (`*_DTO`) and error codes inside `.Infrastructure/Acl/` using 3 `internal sealed` classes[cite: 9]:
  - `*Translator`: pure mapping using `CultureInfo.InvariantCulture` (zero I/O)[cite: 9].
  - `*Adapter`: I/O communication via `IHttpClientFactory`[cite: 9, 18].
  - `*Acl`: facade implementing `I*Port`[cite: 9].
- **Tolerant Reader (Consumer Side):** Never share contract `.dll` assemblies across contexts[cite: 22]. Deserialize responses into minimal `internal sealed record` DTOs, ignore unknown JSON fields, and map foreign status strings defensively with a fallback (never throw `JsonException` on unknown enum values)[cite: 22].

## 8. Contract-First REST API & Error Handling (`.Presentation` & `docs/contracts/`)
- **Contract-First OpenAPI 3.1:** Author `docs/contracts/openapi-<service>.yaml` before writing controllers or HTTP clients[cite: 19]. Encode Value Object rules directly in JSON Schema Draft 2020-12 (`format: uuid`, `pattern`, `minimum`, `maximum`, `enum`) and set `additionalProperties: false` on all Request schemas[cite: 20, 21, 22].
- **Versioning & Compatibility:** Use URI path versioning (`/api/v1/...`)[cite: 23]. Never rename/remove response fields, change types, add new `required` request fields, or tighten validation inside `/v1/`[cite: 22]. Retire old endpoints via `Deprecation: true` and `Sunset` headers (RFC 8594)[cite: 23].
- **Business Intent Resources (RMM Level 2+):** Never expose anemic `PUT` endpoints that let clients overwrite aggregate state directly[cite: 26, 27]. Model state transitions as noun sub-resources matching Ubiquitous Language commands (e.g., `POST /api/v1/applications/{id}/submission`, `POST /api/v1/orders/{id}/cancellation`)[cite: 27].
- **Idempotency (RFC 9110):** Require the `Idempotency-Key` header (`format: uuid`) on state-mutating `POST` commands[cite: 20, 28, 29]. Persist `(Key, RequestHash, StatusCode, ResponseBody)` in the same SQL transaction as the domain mutation[cite: 29]. Return cached responses on identical retries; reject payload mismatches with `409`/`422` (`IdempotencyKeyConflictException`)[cite: 29].
- **RFC 9457 Problem Details & Tracing:**
  - Never return `200 OK` for failures, custom ad-hoc error payloads, or raw stack traces[cite: 30].
  - Controllers and handlers MUST NOT use `try/catch` for HTTP mapping[cite: 33]. Handle exceptions centrally via `IExceptionHandler` and `IProblemDetailsService` returning `application/problem+json` with `type`, `title`, `status`, `detail`, `instance`, `errorCode` (`DomainException.Code`), and W3C `traceId` (`Activity.Current?.TraceId`), propagating `traceparent` across `IHttpClientFactory`[cite: 31, 33].
  - **Status Code Mapping:** `400` (malformed JSON / invalid Value Object), `401`/`403` (auth/permissions), `404` (`*.not_found`), `409` (`*.state_conflict` / optimistic concurrency `ETag`), `422` (default `DomainException` invariant violation or `Idempotency-Key` payload conflict), `503`/`504` (downstream dependency failure / open circuit)[cite: 29, 32, 33].

## 9. Cross-Context Resilience & Fault Tolerance (`.Infrastructure`)
- **HTTP Clients:** Always use `IHttpClientFactory` with Docker/Kubernetes service DNS names (never `new HttpClient()`, `localhost`, or the default 100s timeout)[cite: 18, 34, 35].
- **Polly v8 Pipeline Order (Outer -> Inner):** Configure pipelines per `docs/architecture/resilience-policies.md` in strict order[cite: 39, 40]:
  1. **Fallback / Mapping:** Catches `BrokenCircuitException` or final timeout for graceful degradation (Stale Cache, Store & Forward background queue, or `503` + `Retry-After`)[cite: 38, 39].
  2. **Total Request Timeout:** Caps total budget across all attempts (`TotalTimeout > (MaxRetryAttempts + 1) * AttemptTimeout + Backoff`)[cite: 36, 39, 40].
  3. **Retry (Exponential Backoff + Decorrelated Jitter):** Retries ONLY transient errors (`503`, `504`, `408`, `429`, network failures) on idempotent calls (or `POST` with `Idempotency-Key`)[cite: 36, 39]. NEVER retry `400`, `401`, `403`, `404`, `409`, or `422`[cite: 32, 36, 40].
  4. **Circuit Breaker + Bulkhead:** Isolates concurrency/connection pools per dependency and fails fast (`0 ms`) when failure thresholds are breached[cite: 37, 38, 39].
  5. **Attempt Timeout:** Cuts individual hanging attempts early (e.g., `2.0 s`–`3.0 s`) so Retry can trigger[cite: 39, 40].

## 10. Agent Workflow & Testing Conventions (`xUnit` Only)
- **Iteration Protocol (Plan -> Red -> Green):**
  1. **Plan & Contract:** Update `docs/contracts/openapi-*.yaml` and propose ports (`.Application/Ports` or `.Domain/Ports`), CQRS contracts, and `DomainException` types[cite: 15, 19].
  2. **RED Phase:** Write `xUnit` tests first from Given-When-Then scenarios, OpenAPI schemas, and ACL tables (no NUnit/MSTest). Do not write tautological mock-mirroring tests; every domain invariant test must fail if its `throw` or `private set` guard is mutated[cite: 15].
  3. **GREEN Phase:** Write minimal production code to pass tests[cite: 15].
  4. **Verification:** Run `CleanArchitectureGuardTests` (`NetArchTest.Rules`) and all `xUnit` tests[cite: 15].
- **Test Naming & Isolation:** Mirror `src/Services/<Context>/...` under `tests/<Context>/...` and name every test `Check_<SubjectOrAction>_<ConditionOrInput>_Should<ExpectedResult>`. Domain tests must be purely in-memory with zero database or network dependencies.
- **Required Fitness Functions (`FF-01..FF-06`):**
  - `FF-01`: `.Domain` has zero ORM/infrastructure/external dependencies[cite: 3, 12].
  - `FF-02`: No `public set` on domain entities, aggregates, or Value Objects[cite: 12, 15].
  - `FF-03` / `FF-04`: Layer dependency rules and ACL isolation (`*_DTO` types are `internal` to `.Infrastructure/Acl`)[cite: 9, 15].
  - `FF-05`: No cross-context references or cross-schema foreign keys[cite: 14, 17].
  - `FF-06`: Consumer-Driven Contract (CDC) tests verified against `openapi-*.yaml`[cite: 24].
- **Deterministic Fault-Injection Tests:** Every resilient HTTP adapter must include `xUnit` tests using a scripted `HttpMessageHandler` verifying retry counts, `Idempotency-Key` preservation, non-retry on `400`/`422`, and circuit-breaker fail-fast behavior[cite: 40].