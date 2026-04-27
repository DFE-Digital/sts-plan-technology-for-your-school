# Technical Debt Backlog

Generated from code review during documentation update sprint (March 2026). Items are grouped by theme and ordered roughly by risk within each group. Architectural items are listed last as they require design discussion before work begins.

## Summary

| # | Title | Theme | Estimate |
|---|-------|-------|----------|
| 1 | Contentful errors silently swallowed | Bug | 1 day |
| 2 | Service Bus native retry & dead-letter | Bug | 2 days |
| 3 | `LockAndRun`/`LockAndGet` exception handling | Bug | 1 day |
| 4 | `SignInEntity.EstablishmentId` magic default | Bug | 1 day |
| 5 | `DatabaseUpgrader` retry catches all exceptions | Bug | 0.5 days |
| 6 | Data Protection keys to Azure Key Vault | Security | 2 days |
| 7 | DatabaseUpgrader connection string in CLI args | Security | 1 day |
| 8 | `PageModelAuthorisationPolicy` per-request CMS call | Performance | 3 days |
| 9 | `ComponentViewsFactory` reflection at render time | Performance | 1 day |
| 10 | `HttpContext.Items` coupling in auth policy/binder | Design | 0.5 days |
| 11 | Migrate ServiceBus to `System.Text.Json` | Design | 1 day |
| 12 | `ApiAuthenticationConfiguration` business logic | Design | 0.5 days |
| 13 | DSI establishment IDs to configuration | Design | 1 day |
| 14 | Replace manual `AsDto()` mappings | Design | 2 days |
| 15 | Business logic out of `SubmissionRepository` | Design | 2 days |
| 16 | Replace database triggers with app-layer logic | Design | 3 days |
| 17 | Eliminate Service + Workflow double-layer | Architecture | 5 days |
| 18 | Decompose `CurrentUser` god class | Architecture | 4 days |
| 19 | Remove Contentful dependency from `Core` | Architecture | 5 days |
| 20 | Confirmation prompt for `delete-all-content.js` | Tooling | 0.5 days |
| 21 | Run or remove `remove-underlining` script | Tooling | 0.5 days |
| 22 | Unify ES modules in `content-management` | Tooling | 1 day |
| 23 | Fix `export-processor` 9 GB heap | Tooling | 3 days |
| | **Total** | | **~41 days** |

## Bugs & Correctness

### 1. Contentful errors silently swallowed in `ContentfulRepository`

**Objective**
Ensure the application surfaces Contentful partial-failure conditions to callers rather than silently continuing with incomplete data.

**Description**
`ContentfulRepository.ProcessContentfulErrors` logs errors from `ContentfulCollection.Errors` but never throws or signals failure. When Contentful returns partial results (e.g. an unresolvable linked entry), the application receives incomplete content and continues rendering. This can produce pages with missing sections or recommendations that don't appear, with no visible error and no way for callers to detect or recover.

**Acceptance Criteria**

- [ ] `ProcessContentfulErrors` accepts a caller-controlled flag (or a separate strict-mode overload) that throws a typed exception when errors are present
- [ ] All existing call sites default to the current lenient behaviour so nothing breaks
- [ ] At least one call site (e.g. the recommendation page query) opts into strict mode, so a partial CMS failure produces a 500 rather than a silently broken page
- [ ] Unit tests cover: no errors → no exception; errors + lenient → logs only; errors + strict → throws

**Estimate:** 1 day

### 2. Azure Service Bus message handling should use native retry and dead-letter mechanisms

**Objective**
Replace the hand-rolled retry implementation with Azure Service Bus's built-in delivery count and dead-letter support, and ensure transient errors trigger a retry rather than an immediate dead-letter.

**Description**
Two separate issues in `Dfe.PlanTech.Infrastructure.ServiceBus`:

1. `MessageRetryHandler` completes the original message and re-enqueues a brand-new `ServiceBusMessage` with a custom `DeliveryAttempts` property. This loses the original `MessageId`, `EnqueuedTime`, and all Service Bus metadata. Azure Service Bus provides `AbandonMessageAsync` for re-queue-with-retry and `DeferMessageAsync` for delayed retry; the `MaxDeliveryCount` queue setting handles dead-lettering automatically.

2. If an unhandled exception escapes `MessageHandler` in `ContentfulServiceBusProcessor`, the message is immediately dead-lettered. A single Redis timeout or network blip permanently removes a cache-invalidation event from the queue.

**Acceptance Criteria**

- [ ] `MessageRetryHandler` is removed; retry is handled by calling `AbandonMessageAsync` so the Service Bus runtime increments the native `DeliveryCount`
- [ ] `MaxDeliveryCount` on the cache-invalidation queue is documented in Terraform and set to an appropriate value (suggest 5)
- [ ] Unhandled exceptions in `ContentfulServiceBusProcessor.MessageHandler` call `AbandonMessageAsync` rather than allowing dead-lettering
- [ ] The custom `DeliveryAttempts` property is no longer written or read
- [ ] Integration test (or manual test plan) confirms a simulated failure re-queues rather than dead-letters

**Estimate:** 2 days

### 3. `LockAndRun`/`LockAndGet` swallow exceptions from the locked operation

**Objective**
Ensure callers of `RedisLockProvider.LockAndRun` and `LockAndGet` can observe and handle failures in the locked operation.

**Description**
Both methods catch all exceptions from the delegate, log them, and return `default`. The lock is released (correctly, in `finally`), but the caller has no way to know the operation failed. Any business logic relying on the return value of `LockAndGet` will silently produce wrong results. The simplest fix — re-throwing after logging — was explored but reverted; this ticket captures the need to decide and implement the correct approach for this codebase, which may be re-throw, a `Result<T>` wrapper, or a cancellation-based pattern.

**Acceptance Criteria**

- [ ] A decision is made and documented (ADR or inline comment) on whether to re-throw, return a result wrapper, or use another pattern
- [ ] Callers that currently rely on swallowed exceptions are identified and updated
- [ ] The lock is still released via `finally` regardless of outcome
- [ ] Unit tests cover the failure path for both methods

**Estimate:** 1 day

### 4. `SignInEntity.EstablishmentId` defaults to magic value `1`

**Objective**
Remove the magic default value from a foreign key column to prevent silent data corruption.

**Description**
`SignInEntity.EstablishmentId` is a nullable foreign key but is configured with a default value of `1`. Sign-ins without an establishment are a valid scenario, but a missing establishment currently writes `1` rather than `NULL`. If establishment ID 1 ever changes meaning, this produces silently wrong data with no error.

**Acceptance Criteria**

- [ ] The default value of `1` is removed from the column configuration
- [ ] The column is properly nullable with no default
- [ ] A database migration script removes the default constraint
- [ ] Existing rows with `EstablishmentId = 1` that were set by the default (rather than a genuine establishment) are identified and handled (null-out or leave, with a documented decision)
- [ ] Code that reads `EstablishmentId` handles `null` correctly

**Estimate:** 1 day

### 5. `DatabaseUpgrader` retry policy retries on all exceptions

**Objective**
Scope the retry policy to transient failures so genuine bugs fail fast.

**Description**
`Policy.Handle<Exception>()` in `DatabaseExecutor` retries on any exception, including `NullReferenceException` and `ArgumentException`, which will never succeed on retry. This wastes up to 6 minutes on genuine bugs and makes deployment failures very slow to surface.

**Acceptance Criteria**

- [ ] Retry is scoped to `SqlException` with transient error codes (timeout, connection failure, deadlock)
- [ ] Non-transient exceptions fail immediately without retrying
- [ ] The retry count and backoff are documented

**Estimate:** 0.5 days

## Security

### 6. Move Data Protection keys out of the application database

**Objective**
Ensure ASP.NET Core Data Protection keys are not stored in the same database as the application data they protect.

**Description**
`DataProtectionDbContext` persists Data Protection keys in the application's SQL Server database. A database breach therefore exposes both the application data and the keys used to protect it (session cookies, anti-forgery tokens, etc.). Azure Key Vault is the standard storage location for an Azure-hosted application and is already used elsewhere in the project.

**Acceptance Criteria**

- [ ] Data Protection keys are stored in Azure Key Vault (using `ProtectKeysWithAzureKeyVault`)
- [ ] `DataProtectionDbContext` and its migration are removed
- [ ] Key rotation policy is documented
- [ ] Existing keys are migrated or a re-authentication plan is in place for active sessions
- [ ] Terraform provisions the Key Vault key and grants the app identity appropriate access

**Estimate:** 2 days

### 7. `DatabaseUpgrader` connection string passed as a CLI argument

**Objective**
Stop passing database credentials in a way that is visible in process lists, shell history, and deployment logs.

**Description**
The connection string is passed via the `-c` flag on the command line. Command-line arguments are visible in `ps aux`, Windows Task Manager, Azure deployment logs, and shell history. Credentials should be passed via environment variable or Azure Key Vault reference.

**Acceptance Criteria**

- [ ] The `-c` / `connectionstring` CLI argument is replaced with an environment variable or Key Vault reference
- [ ] The Terraform and GitHub Actions workflows are updated to pass credentials via the new mechanism
- [ ] The README is updated
- [ ] No connection string appears in any process argument, log line, or deployment output

**Estimate:** 1 day

## Performance

### 8. `PageModelAuthorisationPolicy` makes a CMS/Redis call on every page request

**Objective**
Remove the per-request remote call from the authorisation path for page authentication status.

**Description**
`PageModelAuthorisationPolicy` calls `IContentfulService.GetPageBySlugAsync` (a Redis-backed CMS query) on every incoming request to the `PagesController`, before the request reaches a controller. Even with Redis caching, this adds a network round-trip to every page authorisation check. The `RequiresAuthorisation` flag is stable CMS data that could be held in an in-memory lookup populated at startup.

This requires:

- A startup service that fetches all page slugs and their `RequiresAuthorisation` flags from Redis/Contentful and populates an `IMemoryCache` or `ConcurrentDictionary`
- The Service Bus cache-invalidation path being extended to refresh the in-memory lookup when a `page` entry is invalidated
- A fallback to the existing Redis path if the in-memory lookup has no entry (for newly published pages before next restart)

**Acceptance Criteria**

- [ ] Auth policy consults an in-memory lookup for `slug → requiresAuthorisation` before falling back to Redis
- [ ] The lookup is populated at application startup
- [ ] Cache invalidation (via Service Bus) triggers a refresh of the in-memory lookup for the affected page
- [ ] A slug not in the lookup falls back to the existing Redis query (handles newly published pages)
- [ ] Benchmark or log comparison shows reduced Redis calls on a production-like load

**Estimate:** 3 days

### 9. `ComponentViewsFactory` uses reflection to resolve component view paths

**Objective**
Replace the runtime reflection scan in `ComponentViewsFactory` with a static, startup-time mapping.

**Description**
`ComponentViewsFactory.TryGetViewForType` performs a linear scan over compiled assembly types at render time to find the Razor view path for a component model type. This scan is unindexed, runs on the rendering hot path, and fails silently at render time if a naming convention isn't followed exactly. A static `Dictionary<Type, string>` registered at startup would be faster, more reliable, and immediately visible to developers.

**Acceptance Criteria**

- [ ] `ComponentViewsFactory` uses a pre-built `Dictionary<Type, string>` populated at startup
- [ ] The dictionary is built once (e.g. via `IHostedService` or static initialiser) and reused across requests
- [ ] A missing mapping throws at startup rather than failing silently at render time
- [ ] All existing component types are covered
- [ ] Unit tests verify correct view path is returned for known types and that unknown types fail appropriately

**Estimate:** 1 day

## Design & Technical Debt

### 10. Decouple `PageModelAuthorisationPolicy` and `PageModelBinder` from `HttpContext.Items`

**Objective**
Replace the invisible, stringly-typed contract between the auth policy and the model binder with an explicit mechanism.

**Description**
`PageModelAuthorisationPolicy` stores the fetched `PageEntry` in `HttpContext.Items["PageEntry"]` and `PageModelBinder` retrieves it using the same string key. If the key changes in one place, the other silently receives `null`. This is an invisible coupling between two components that have no formal relationship. An endpoint filter, a typed `HttpContext` extension method, or a scoped service would make the contract explicit.

**Acceptance Criteria**

- [ ] The string key `"PageEntry"` is replaced with a typed accessor (e.g. a static extension method `httpContext.GetPageEntry()` / `httpContext.SetPageEntry(page)` that encapsulates the key)
- [ ] Both the auth policy and the binder use the same typed accessor
- [ ] A missing page entry in the binder logs a structured warning with the slug rather than a generic null-reference path
- [ ] Unit tests cover both the set and get paths

**Estimate:** 0.5 days

### 11. Migrate `Newtonsoft.Json` to `System.Text.Json` in `Dfe.PlanTech.Infrastructure.ServiceBus`

**Objective**
Eliminate the second JSON library from the solution.

**Description**
`Dfe.PlanTech.Infrastructure.ServiceBus` is the only project in the solution using `Newtonsoft.Json`. All other projects, including the Redis serialisation layer, use `System.Text.Json`. Two JSON libraries in the same process have subtly different deserialisation behaviour and doubles the dependency surface for security vulnerabilities.

**Acceptance Criteria**

- [ ] `Newtonsoft.Json` is removed from `Dfe.PlanTech.Infrastructure.ServiceBus.csproj`
- [ ] All serialisation/deserialisation in that project uses `System.Text.Json`
- [ ] `CmsWebHookPayload` and related models deserialise correctly from the Contentful webhook JSON format under `System.Text.Json` (camelCase, `[JsonPropertyName]` attributes as needed)
- [ ] Existing unit tests pass; new tests cover the JSON mapping for the webhook payload models

**Estimate:** 1 day

### 12. Move `ApiAuthenticationConfiguration` business logic to a service

**Objective**
Restore `ApiAuthenticationConfiguration` to a plain data-carrier with no methods.

**Description**
`ApiAuthenticationConfiguration` has a `HasApiKey` computed property and an `ApiKeyMatches()` method. Configuration objects should carry data only; logic that operates on that data is harder to test in isolation and breaks the single-responsibility principle.

**Acceptance Criteria**

- [ ] `HasApiKey` and `ApiKeyMatches()` are removed from `ApiAuthenticationConfiguration`
- [ ] The logic is moved to the class that currently calls these methods
- [ ] Existing tests are updated; the logic is directly testable without constructing a configuration object

**Estimate:** 0.5 days

### 13. Move hardcoded DSI establishment type IDs to configuration

**Objective**
Allow DfE Sign-in reference data to be updated without a code change and redeployment.

**Description**
`DsiConstants` contains a large set of numeric/string IDs for organisation categories and establishment types. These are government reference data values that can change. As `const` strings in a compiled assembly, any change requires a code change, PR, and full deployment.

**Acceptance Criteria**

- [ ] The values in `DsiConstants` are loaded from application configuration (e.g. `appsettings.json` + Key Vault override) rather than compiled as constants
- [ ] Default values matching the current constants are provided in `appsettings.json`
- [ ] No behaviour change in existing functionality
- [ ] The README or a comment documents where to update these values

**Estimate:** 1 day

### 14. Replace manual `AsDto()` entity mappings with a mapping library

**Objective**
Eliminate ~15 classes of hand-written boilerplate mapping that must be kept in sync manually.

**Description**
Each of the 15 SQL entities has a hand-written `AsDto()` method. When the entity schema changes, every mapping must be updated manually, and a silently dropped field produces no compile-time error. A mapping library (Mapster is recommended as it supports source generation, reducing runtime overhead) would eliminate the boilerplate and make schema drift a compile-time failure.

**Acceptance Criteria**

- [ ] A mapping library is selected and added (Mapster recommended)
- [ ] All 15 `AsDto()` methods are replaced with generated mappings
- [ ] Any non-trivial custom field mappings are explicitly configured and documented
- [ ] Unit tests verify DTO output for each entity type
- [ ] `AsDto()` methods are deleted

**Estimate:** 2 days

### 15. Move business logic out of `SubmissionRepository`

**Objective**
Restore `SubmissionRepository` to a data-access class and move orchestration logic to the application layer.

**Description**
`ConfirmCheckAnswersAndUpdateRecommendationsAsync` in `SubmissionRepository` performs multi-step orchestration: comparing previous and new recommendation statuses, upserting recommendation records, creating history entries, marking submissions as reviewed, and marking older submissions as inaccessible. This is business logic and belongs in the workflow or service layer, not in a repository.

**Acceptance Criteria**

- [ ] The orchestration in `ConfirmCheckAnswersAndUpdateRecommendationsAsync` is moved to a workflow or service class
- [ ] `SubmissionRepository` exposes simple, single-responsibility data access methods (upsert, mark-reviewed, mark-inaccessible) that the workflow calls
- [ ] Existing behaviour is preserved end-to-end
- [ ] Unit tests for the orchestration logic exist at the workflow level and do not require a database

**Estimate:** 2 days

### 16. Replace database triggers with application-layer logic

**Objective**
Make data side-effects visible, testable, and observable by moving trigger logic into the application.

**Description**
Four entities (`EstablishmentEntity`, `UserEntity`, `SubmissionEntity`, `ResponseEntity`) have database triggers. Triggers are invisible to EF Core, are skipped by bulk operations (e.g. `ExecuteUpdateAsync`), cannot be unit tested, and are difficult to observe or debug in production. The business rules they enforce should be moved to the application layer or EF Core interceptors.

**Acceptance Criteria**

- [ ] Each trigger's logic is documented and understood before removal
- [ ] The equivalent logic is implemented in the application layer (service/workflow) or as an EF Core `SaveChangesInterceptor`
- [ ] Database migrations drop all four triggers
- [ ] Integration tests verify the behaviour previously enforced by each trigger
- [ ] No regression in any submission, response, user, or establishment flow

**Estimate:** 3 days

## Architecture (Design Discussion Required Before Starting)

The following items require a design conversation with the team before implementation begins. Each represents a significant structural change that will touch many files and should be broken into sub-tasks once the approach is agreed.

### 17. Eliminate the Service + Workflow double-layer in `Dfe.PlanTech.Application`

**Objective**
Reduce the number of indirection layers between the presentation layer and data access by collapsing the service/workflow duplication.

**Description**
`Dfe.PlanTech.Application` contains both a `Services/` layer and a `Workflows/` layer. The services are almost entirely thin pass-through facades over the workflows, adding a layer of indirection with no logic. This means 14 interfaces and classes for what is functionally 7 operations. The call stack to reach data is: Controller → ViewBuilder → Service → Workflow → Repository — five layers.

The recommended approach: delete the service layer and have controllers call workflows directly. The `ViewBuilder` layer (which duplicates controller responsibilities) should also be reviewed in the same pass.

**Acceptance Criteria**

- [ ] A design decision is documented (ADR) for the chosen approach
- [ ] Service interfaces and implementations that are pure pass-throughs are deleted
- [ ] Controllers (or ViewBuilders) call workflows directly
- [ ] No behaviour change in any controller action
- [ ] Test coverage is maintained or improved

**Estimate:** 5 days

### 18. Decompose the `CurrentUser` god class

**Objective**
Break `CurrentUser` into focused, single-responsibility components that can each be independently tested.

**Description**
`CurrentUser` (~374 lines) handles: DSI reference resolution, email, group school selection (cookie serialisation/deserialisation), async and sync establishment resolution, MAT detection, `IsInRole` checks, and direct `HttpContext` access. It reads and writes cookies, deserialises JSON, and performs async database calls. This makes it effectively untestable in isolation and very risky to modify.

Suggested decomposition:

- `ClaimsReader` — extracts typed values from the claims principal
- `SchoolSelectionService` — cookie-based school selection (read/write/clear)
- `EstablishmentResolver` — resolves establishment from DB given a DSI reference

**Acceptance Criteria**

- [ ] A design decision is documented for the decomposition approach
- [ ] `CurrentUser` no longer directly accesses `HttpContext` for anything other than the claims principal
- [ ] Cookie operations are in a dedicated service
- [ ] Each new component has unit tests
- [ ] No behaviour change observed end-to-end

**Estimate:** 4 days

### 19. Remove the `contentful.csharp` dependency from `Dfe.PlanTech.Core`

**Objective**
Restore `Core` to a true domain/shared layer with no infrastructure dependencies.

**Description**
`Dfe.PlanTech.Core` has a direct NuGet dependency on `contentful.csharp`. This means every project that references Core also transitively depends on the Contentful SDK — including the SQL data layer and infrastructure projects that have nothing to do with Contentful. In clean architecture, Core should have zero infrastructure dependencies.

The Contentful entry models currently in `Core/Contentful/` would be better placed in `Dfe.PlanTech.Data.Contentful`, which is already the correct home for Contentful-specific concerns. The SQL DTOs in `Core/DataTransferObjects/Sql/` should move to `Dfe.PlanTech.Data.Sql`.

**Acceptance Criteria**

- [ ] A design decision is documented for the migration approach, including how to handle circular dependency risks
- [ ] `contentful.csharp` is removed from `Dfe.PlanTech.Core.csproj`
- [ ] Contentful entry models live in `Dfe.PlanTech.Data.Contentful`
- [ ] SQL DTOs live in `Dfe.PlanTech.Data.Sql`
- [ ] All projects compile and all tests pass
- [ ] No behavioural change

**Estimate:** 5 days

## Contentful Tooling

### 20. Add a confirmation prompt to `content-management/delete-all-content.js`

**Objective**
Prevent accidental irreversible deletion of Contentful content.

**Description**
`delete-all-content.js` executes destructive deletions immediately with no confirmation step. A mistyped `ENVIRONMENT` variable could silently wipe production content. The `content-migrations` tool in the same repository already requires a `Y/N` confirmation; this script should do the same.

**Acceptance Criteria**

- [ ] The script prompts `"This will delete ALL content from [environment]. Type YES to confirm:"` before proceeding
- [ ] Any input other than `YES` (case-sensitive) aborts with a clear message
- [ ] A `--confirm` flag can bypass the prompt for use in automated pipelines (with appropriate documentation)

**Estimate:** 0.5 days

### 21. Run the `remove-underlining-from-richtext` change script or remove it

**Objective**
Resolve the ambiguous state of the most recent Contentful change script.

**Description**
`content-management/changes/20260119-1250-remove-underlining-from-richtext.js` has `const DRY_RUN = true` and has never been run in anger. It exists in the repository looking like a completed migration when it isn't. Either the underlining problem it addresses still exists (in which case the script should be run), or it has been resolved another way (in which case the script should be deleted).

**Acceptance Criteria**

- [ ] The team confirms whether underlining in rich text fields is still a live issue
- [ ] If yes: the script is run against each environment in turn and `DRY_RUN` is removed (or the script deleted after running)
- [ ] If no: the script is deleted
- [ ] Either way, the repository no longer contains an uncommitted change script in dry-run mode

**Estimate:** 0.5 days

### 22. Unify module format in `content-management` (CommonJS → ES modules)

**Objective**
Eliminate the CommonJS/ES module mix that causes Jest configuration complexity and fragility.

**Description**
`content-management` mixes `require()`/`module.exports` (CommonJS) and `import`/`export` (ES modules) across different files. Jest is already doing non-trivial gymnastics with `--experimental-vm-modules` to handle this. The codebase should be unified on ES modules (`import`/`export`) to match the rest of the Node.js tooling in this repository.

**Acceptance Criteria**

- [ ] All files in `content-management` use ES module syntax
- [ ] `require()` and `module.exports` are eliminated
- [ ] `jest.config.js` no longer needs `--experimental-vm-modules`
- [ ] All existing tests pass

**Estimate:** 1 day

### 23. Fix `export-processor` 9 GB heap requirement

**Objective**
Reduce the memory requirements of the recommendations CSV export to a sensible level.

**Description**
`export-processor` runs the recommendations CSV export with `--max-old-space-size=9000`. This is a strong signal of a memory efficiency problem rather than a genuine data size requirement. The `DataMapper` builds a complete in-memory object graph of the entire Contentful export before processing; the recommendations export then iterates over it in a memory-intensive way. A streaming or incremental approach would reduce memory usage by orders of magnitude.

**Acceptance Criteria**

- [ ] The peak memory usage of `export-recommendations-csv` is profiled and the hotspot identified
- [ ] The processing is refactored to stream or process entries incrementally rather than building a full in-memory graph
- [ ] The script runs successfully with a heap size of 512 MB or less
- [ ] The `--max-old-space-size=9000` flag is removed
- [ ] Output CSV content is identical to the current implementation

**Estimate:** 3 days
