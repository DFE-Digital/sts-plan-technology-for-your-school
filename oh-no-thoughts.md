# Oh No Thoughts

A running log of code quality concerns spotted during the documentation update. Roughly ordered by severity within each entry.

---

## `src/Dfe.PlanTech.Core`

### Infrastructure concerns leaking into a "Core" library

`Dfe.PlanTech.Core` has a direct NuGet dependency on `contentful.csharp`. This means every project in the solution that references Core also transitively depends on the Contentful SDK — including the SQL data layer and infrastructure projects that have nothing to do with Contentful. In clean architecture, a "Core" or "Domain" library should have zero infrastructure dependencies. The Contentful entry models and query builders would be better placed in `Dfe.PlanTech.Data.Contentful`, which is already the right home for Contentful-specific concerns.

### SQL DTOs in Core

`DataTransferObjects/Sql/` lives in Core, meaning the SQL data contract is baked into the shared base layer. If the database schema changes, Core changes, and everything recompiles. These DTOs belong in `Dfe.PlanTech.Data.Sql`.

### DSI establishment type IDs as hardcoded constants

`DsiConstants` contains a large number of numeric/string IDs for DfE Sign-in organisation categories and establishment types. These are government reference data values that can and do change. Hardcoding them as `const` strings in a compiled assembly means a code change and redeployment is required every time the reference data changes. These should live in configuration or a seeded database table.

### `ReflectionHelper` used for type discovery at runtime

`ReflectionHelper.GetAllConcreteInheritingTypes<T>()` scans loaded assemblies at runtime to find subtypes. In a web application this is called per-request (or at least per-startup) and is expensive — reflection over all loaded types is slow and allocation-heavy. The Contentful content type mapping (`ContentfulContentTypeConstants`) already exists as a static dictionary; that pattern should be used consistently instead of runtime reflection.

### 30+ Contentful entry models in a single flat folder

`Contentful/Models/` contains over 30 `*Entry` classes in one directory with no further grouping. As the content model grows this becomes very hard to navigate. Grouping by concern (questionnaire, recommendations, page components, navigation) would help significantly.

---

## `src/Dfe.PlanTech.Application`

### Redundant Service + Workflow layering

There is both a `Services/` layer and a `Workflows/` layer, and the services are almost entirely thin pass-through facades over the workflows — they add a layer of indirection with no logic of their own. This doubles the number of classes and interfaces a developer must read to understand any operation (14 interfaces for what is effectively 7 things). Pick one: either services call repositories directly, or you have workflows and expose them directly to the presentation layer. Having both without clear differentiation is a common organic-growth anti-pattern that makes onboarding significantly harder.

### `Workflows/ContentfulQueries/` is compiled out of the project

The folder `Workflows/ContentfulQueries/` is explicitly excluded in the `.csproj`. This means there is dead code sitting in the repository that doesn't build and isn't tested. It should either be restored and used, or deleted. Excluded source folders are a maintenance hazard — they're invisible to the compiler but visible to confused developers.

### `ApiAuthenticationConfiguration` contains business logic

Configuration objects should be plain data carriers. `ApiAuthenticationConfiguration` has a `HasApiKey` computed property and an `ApiKeyMatches()` method. Business logic in a configuration class is a mixing of concerns — it makes the logic harder to test in isolation and breaks the single-responsibility principle.

### `GoogleTagManagerServiceServiceConfiguration` — naming error

The class is named `GoogleTagManagerServiceServiceConfiguration` (double "Service"). This is clearly a typo that has been compiled and shipped. It's a small thing but it suggests the class wasn't reviewed carefully.

### Hardcoded "(opens in new tab)" string in `HyperlinkRenderer`

The accessibility label "(opens in new tab)" is a string literal in `HyperlinkRenderer.cs`. If the content team ever needs to change this wording (e.g. for different locales or updated accessibility guidance), it requires a code change and redeployment. This string belongs in microcopy/CMS alongside the other UI text.

---

## `src/Dfe.PlanTech.Data.Sql`

### Business logic in a repository (`SubmissionRepository`)

`ConfirmCheckAnswersAndUpdateRecommendationsAsync` in `SubmissionRepository` is a complex multi-step orchestration: it compares previous and new recommendation statuses, upserts recommendation records, creates history entries, marks the submission as reviewed, and marks older reviewed submissions as inaccessible. That is business logic and belongs in the application/workflow layer, not in a repository. Repositories should provide simple data access operations; the "what to do with the data" decision should live higher up.

### Data Protection keys stored in the application database

`DataProtectionDbContext` persists ASP.NET Core Data Protection keys in the same SQL Server database as the application data. This means a database breach potentially exposes both the application data and the keys used to protect it. Data Protection keys should be stored in a dedicated, separately secured store — Azure Key Vault is the standard choice for an Azure-hosted application.

### `SignInEntity.EstablishmentId` defaults to magic value `1`

The `EstablishmentId` foreign key on `SignInEntity` is nullable in intent (sign-ins without an establishment are a valid scenario) but is configured with a default value of `1`. This is a magic number — if establishment ID 1 ever doesn't exist or changes meaning, this silently produces bad data. The column should be nullable with no default.

### Repository methods expose `Expression<Func<T, bool>>` predicates

`GetEstablishmentsByAsync` and `GetUserByAsync` take `Expression<Func<T, bool>>` parameters — raw LINQ expression trees — as arguments. This leaks the EF Core abstraction through the repository interface: callers must know they are dealing with an ORM, and switching data access technology would require changing every call site. Repositories should expose domain-meaningful query methods rather than ORM primitives.

### Database triggers for business logic

Four entities (`EstablishmentEntity`, `UserEntity`, `SubmissionEntity`, `ResponseEntity`) have associated database triggers. Triggers make data flow opaque — side effects happen invisibly, are skipped by bulk operations, are very hard to unit test, and are difficult to observe in production. Business rules implemented in triggers should be moved to the application layer.

### Manual `AsDto()` mapping on every entity

Each of the 15 entities has a hand-written `AsDto()` method mapping it to its corresponding DTO. This is ~15 classes of boilerplate that must be kept in sync manually. A mapping library (Mapster, AutoMapper) or source-generated mappings would eliminate the maintenance burden and reduce the risk of a field being silently dropped.

---

## `src/Dfe.PlanTech.Data.Contentful`

### Contentful errors are silently swallowed

`ContentfulRepository.ProcessContentfulErrors` logs errors from `ContentfulCollection.Errors` but never throws or signals failure to the caller. When Contentful returns partial results (e.g. an unresolvable reference), the application silently receives incomplete content and continues. This can cause subtle, hard-to-diagnose issues — pages rendering with missing sections, recommendations not showing — with no visible error. At minimum, callers should be able to opt into strict mode.

### Duplicated logic between static and explicit interface implementation in `ContentfulRepository`

`GetEntryByIdOptions` appears twice: once as a `public static` method and once as an explicit `IContentfulRepository` interface implementation (marked `[ExcludeFromCodeCoverage]`). The two implementations contain identical logic rather than the interface method delegating to the static one. This is dead duplication that will eventually diverge.

### `EntryResolver` has two overlapping type-resolution mechanisms

`EntryResolver` first checks `ContentfulContentTypeConstants.ContentTypeToEntryClassTypeMap` (the explicit, maintained dictionary in Core), then falls back to a reflection-built dictionary of all `ContentfulEntry` subtypes. If the explicit map is authoritative, the reflection fallback is unnecessary noise. If reflection is needed, the explicit map is redundant. The two approaches should be unified — whichever is chosen, the other should be removed.

### Cache key stability is not guaranteed for filtered queries

The cache key for `GetEntriesAsync<TEntry>(options)` is `{contentTypeName}{options.SerializeToRedisFormat()}`. If the serialisation of `GetEntriesOptions` is not deterministic (e.g., if it serialises a dictionary or collection whose ordering is not guaranteed), two logically identical queries could produce different cache keys and both miss the cache. The serialisation format should be verified to be stable and order-independent.

---

## `src/Dfe.PlanTech.DatabaseUpgrader`

### Target framework is .NET 8.0 while the rest of the solution is .NET 9.0

The upgrader targets `net8.0` while every other project in the solution targets `net9.0`. This means it has a separate support lifecycle and will need its own upgrade pass. It should be brought in line with the rest of the solution.

### Connection string passed as a plain CLI argument

The database connection string is passed via `-c` on the command line. Command-line arguments are visible in the OS process list (`ps aux`, Task Manager, deployment logs) and persist in shell history. Credentials should be passed via environment variable or a secrets manager (Azure Key Vault, Key Vault references in App Service config), not as a process argument.

### Retry policy catches all exceptions indiscriminately

`Policy.Handle<Exception>()` in `DatabaseExecutor` retries on any exception — including `NullReferenceException`, `ArgumentException`, and other programmer errors that will never succeed on retry. The policy should be scoped to transient failures (e.g. `SqlException` with specific error codes for timeouts and connection failures) so that genuine bugs fail fast rather than wasting 6 minutes retrying.

### `FormattedSqlParameters` silently truncates values containing `=`

`Options.FormattedSqlParameters` splits each `KEY=VALUE` string with `.Split('=')` and takes `[0]` and `[1]`. If a value legitimately contains `=` (base64 strings, connection string fragments, signed URLs), everything from the second `=` onwards is silently discarded. The split should use `Split('=', 2)` to limit to two parts.

### `Azure.Identity` and `System.IdentityModel.Tokens.Jwt` referenced but not used

Both packages appear in the `.csproj` but there is no code in the project that uses them. Dead dependencies add to the supply chain attack surface, slow down builds, and create false signals for security scanning tools. They should be removed, or the intended usage documented if this is planned future work.

### A solution file (`.sln`) lives inside the project folder

`Dfe.PlanTech.DatabaseUpgrader.sln` sits inside `src/Dfe.PlanTech.DatabaseUpgrader/` alongside the `.csproj`. Solution files belong at the repository root (where `plan-technology-for-your-school.sln` already lives). This stray file will confuse IDEs and CI tooling that discover solution files by scanning.

---

## `src/Dfe.PlanTech.Infrastructure.Redis`

### `RedisConnectionManager` is almost certainly registered as scoped, not singleton

`ConnectionMultiplexer` is StackExchange.Redis's own recommendation to keep as a long-lived singleton shared across all requests — the [official docs](https://stackexchange.github.io/StackExchange.Redis/Basics) are emphatic about this. If `RedisConnectionManager` is registered as scoped, a new `ConnectionMultiplexer` is created on every request, which is extremely expensive (each connection involves a TCP handshake, AUTH, and SELECT). Even with the lazy `??=` initialisation, scoped lifetime means the field is discarded at the end of each request scope. This needs to be verified and almost certainly fixed to singleton.

### `LockAndRun` and `LockAndGet` silently swallow exceptions from the locked operation

Both methods catch all exceptions, log them, and return `default`. The caller receives `null` with no indication that the operation failed. Any business logic relying on the return value of `LockAndGet` will silently produce wrong results if an exception is thrown inside the lock. These methods should either re-throw or use a result wrapper type so callers can handle failure explicitly.

### Lock acquisition uses a polling spin-wait rather than Pub/Sub

`WaitForLockAsync` loops with `Task.Delay(random 50–600ms)` until the lock is acquired or the timeout is reached. Under contention, multiple instances are each polling Redis independently. A more efficient approach is to subscribe to a Redis Pub/Sub channel and receive a notification when the lock is released, eliminating the polling overhead entirely.

### `RedisDependencyManager` reflects over content properties on every cache write

`GetContentDependenciesAsync` calls `GetType().GetProperties()` and recursively reflects over all nested `ContentfulEntry` properties to discover dependencies — at cache-write time, per entry. This reflection is unbounded in depth, uncached, and runs on the hot path (albeit queued to the background). The type structure of `ContentfulEntry` subclasses is fixed at compile time; the dependency graph should be computed once at startup and cached in a static dictionary.

### Magic value `databaseId = -1` used as a convention throughout `RedisLockProvider`

`-1` is used as a sentinel meaning "use the default database" in all lock methods. This is not documented anywhere except through familiarity with StackExchange.Redis internals. A named constant (`RedisDb.Default`) or simply defaulting to `RedisDb.General` would make intent explicit.

---

## `src/Dfe.PlanTech.Infrastructure.ServiceBus`

### `CmsWebHookPayload.ContentType` likely returns the wrong value

`CmsWebHookPayload.ContentType` is defined as `Sys.Type`, which in the Contentful webhook payload is always the system type — `"Entry"` or `"Asset"` — not the content type ID (e.g. `"page"`, `"category"`). The actual content type ID lives at `sys.contentType.sys.id`. If `ICmsCache.InvalidateCacheAsync` uses the content type to target which cache keys to invalidate, it would always receive `"Entry"` or `"Asset"` and never correctly scope the invalidation to the right content type. This is worth verifying urgently as it could mean cache invalidation is silently broken.

### Retry is implemented by creating a new message rather than using Service Bus's native mechanisms

`MessageRetryHandler` completes the original message and enqueues a brand-new `ServiceBusMessage` with an incremented `DeliveryAttempts` custom property. Azure Service Bus has first-class support for this via `AbandonMessageAsync` (which increments the built-in `DeliveryCount` and re-queues automatically) and `DeferMessageAsync` (for delayed retry). The hand-rolled approach loses the original `MessageId`, `EnqueuedTime`, and other metadata, and duplicates functionality the platform provides for free. The `MaxDeliveryCount` setting on the queue itself would handle dead-lettering without custom logic.

### Unhandled exceptions in `ContentfulServiceBusProcessor` dead-letter immediately with no retry

If an exception escapes the `MessageHandler` method (i.e. is not caught by `CmsWebHookMessageProcessor`), the message is immediately dead-lettered. This means a single transient error — a momentary Redis unavailability, a network timeout — permanently removes a cache invalidation event from the queue. The handler should call `AbandonMessageAsync` for unexpected exceptions so Service Bus's built-in retry policy applies.

### `Newtonsoft.Json` used here while the rest of the solution uses `System.Text.Json`

This is the only project in the solution that references `Newtonsoft.Json`. Everything else, including the Redis serialisation layer, uses `System.Text.Json`. Two JSON libraries in the same application is unnecessary complexity — the serialisation behaviour is subtly different between them and it doubles the attack surface for deserialization vulnerabilities.

### `ServiceBusAction` enum is defined but never used

`ServiceBusAction` (`NotSet`, `DeadLetter`, `Complete`) exists in the codebase but is not referenced anywhere. It is dead code and should be removed.

---

## `src/Dfe.PlanTech.Infrastructure.SignIn`

### `services.BuildServiceProvider()` called during service registration

`ServiceCollectionExtensions.ConfigureOpenIdConnect` calls `services.BuildServiceProvider()` to resolve an `ILogger` instance. This is a well-known anti-pattern explicitly warned against in the ASP.NET Core docs — it creates a second root `IServiceProvider`, causing singleton services to be instantiated twice and scoped services to resolve incorrectly. The logger should instead be resolved lazily inside the event handler via `context.HttpContext.RequestServices`.

### `GetDsiReference` uses `.Single()` with no error handling

`UserClaimsExtensions.GetDsiReference` calls `.Single()` on the claims enumerable filtered by `nameidentifier`. If no matching claim exists, or if there are multiple, this throws an `InvalidOperationException` during the authentication callback — giving the user a generic error with no actionable message and no logged context about which user was affected. At minimum this should use `.SingleOrDefault()` with a guard that calls `context.Fail()` with a descriptive message, consistent with how the rest of `OnUserInformationReceivedEvent` handles failures.

### `UserClaimsExtensions` entirely excluded from code coverage

`[ExcludeFromCodeCoverage]` is applied to the whole class. This is the code that parses the `nameidentifier` and `organisation` claims and hands the user their database IDs. If this parsing is wrong, every user in the system gets the wrong identity. These are arguably the most important functions to have tests for.

### The `organisation` claim is a JSON blob with no schema validation

`GetOrganisation` deserialises the `organisation` claim string directly to `EstablishmentModel` with no validation beyond checking that the resulting `Id` is not `Guid.Empty`. If DfE Sign-in changes the shape of this JSON, the deserialisation will silently succeed but return a partially-null object, potentially writing corrupt establishment records to the database. The deserialised model should be validated before use.

### The old README referenced `DfeSignInSetup.cs` which no longer exists

The file was renamed to `ServiceCollectionExtensions.cs` at some point but the README was never updated. A good example of why this documentation effort is needed.

---

## `src/Dfe.PlanTech.Web.Node`

### Source maps shipped to production

`esbuild.config.js` enables `sourcemap: true` for all JS and CSS builds with no environment check. Source maps in production expose the original source structure, variable names, and logic to anyone who opens browser dev tools or inspects network traffic. They should either be disabled in production builds or uploaded to an error tracking service (e.g. Sentry) and excluded from the public deployment.

### `BrowserHistory` uses `localStorage` — shared across tabs

`browser-history.js` stores navigation history in `localStorage` under the key `BrowserHistory`. `localStorage` is shared across all tabs and windows for the same origin. If a user has two tabs open, back button navigation in one tab will reflect URLs visited in the other. `sessionStorage` (scoped to a single tab) would be the correct choice here.

### Browser targets are from 2017

The esbuild CSS target is `['chrome58', 'firefox57', 'safari11', 'edge16']` — all from late 2017. These are now 7+ years old. Using outdated targets means esbuild emits unnecessary polyfill code and cannot apply modern CSS/JS optimisations that would reduce output size. GDS service standards require supporting the last two major versions of popular browsers; the targets should be updated to reflect current browser support requirements.

### `"gulp": "gulp"` script with no Gulp installation

`package.json` defines a `"gulp": "gulp"` npm script but Gulp is not listed as a dependency and there is no `gulpfile.js`. Running `npm run gulp` will fail. This is leftover from a previous build setup and should be removed.

---

## `src/Dfe.PlanTech.Web`

### Five layers of indirection before reaching data

The full call stack for a typical page request is: **Controller → ViewBuilder → Service → Workflow → Repository**. That is five distinct layers before touching a database or cache. The `ViewBuilder` layer is especially hard to justify — it does exactly what controllers are supposed to do in MVC (fetch data, build a view model, return a result). The existing `Service` + `Workflow` double-layer (flagged in the Application section) is compounded here by adding a third redundant layer on top. New developers spend significant time tracing through this stack before understanding where logic actually lives.

### `PageModelAuthorisationPolicy` makes a Contentful/cache lookup on every page request

To determine whether a page requires authentication, `PageModelAuthorisationPolicy` calls `IContentfulService.GetPageBySlugAsync` — a CMS/Redis query — on every incoming page request, before the request even reaches a controller. Even with Redis caching, this adds latency and a remote call dependency to the authorisation step. Page access requirements are stable CMS data that could be stored as a simple in-memory lookup populated at startup, rather than fetched per-request.

### `PageModelAuthorisationPolicy` and `PageModelBinder` are coupled through `HttpContext.Items`

The auth policy puts the resolved `PageEntry` into `HttpContext.Items` using a string key, and `PageModelBinder` reads it from there. This is an invisible, stringly-typed contract between two unrelated parts of the system. If the key changes in one place, the other silently receives null and the binder logs a warning. This coupling should be made explicit — ideally the page should be passed through a typed endpoint filter or action result rather than ambient state.

### `CurrentUser` is a 374-line god class

`CurrentUser` handles: DSI reference, email, group school selection (with cookie serialisation/deserialisation), async and sync establishment resolution, MAT detection, `IsInRole` checks, and more. It directly accesses `HttpContext`, reads and writes cookies, deserialises JSON, and performs async database calls. This is far too many responsibilities for a single class and makes it very hard to test. It should be decomposed — at minimum into a claims reader, a cookie-based school selection service, and an establishment resolver.

### Synchronous wrappers over async methods in `ICurrentUser`

`ICurrentUser` exposes both synchronous (`GetActiveEstablishmentUrn()`) and asynchronous (`GetActiveEstablishmentUrnAsync()`) versions of the same operations. Providing sync wrappers over inherently async operations (database lookups, cookie reads) in an ASP.NET Core context is an async anti-pattern that risks thread pool starvation and deadlocks, particularly if `.Result` or `.GetAwaiter().GetResult()` is used internally.

### `ComponentViewsFactory` uses reflection to map components to view paths

`ComponentViewsFactory.TryGetViewForType` scans compiled assembly types at runtime to find the Razor view corresponding to a component model type. This is slow (unindexed linear scan), fragile (naming conventions must be exactly right), and fails silently at render time if a view doesn't match. A static dictionary of `Type → viewPath` registered at startup would be faster, more reliable, and immediately obvious to anyone reading the code.

### `ViewModels/Interfaces/` is excluded from compilation

The `.csproj` explicitly excludes `ViewModels\Interfaces\**` from compilation. There is a folder of interface files in the repository that is never built and never used. This is dead code that should either be restored (if it was excluded by mistake) or deleted.

### Bootstrap and jQuery in `wwwroot/lib`

The project ships Bootstrap and jQuery in `wwwroot/lib`. Neither is needed — the application uses GOV.UK Frontend, which has no dependency on either. These are legacy inclusions that add unnecessary weight to every page load and represent unused third-party code that needs to be kept patched for security. They should be removed.

### The existing README referenced two projects that no longer exist

`Dfe.PlanTech.Domain` and `Dfe.PlanTech.Infrastructure.Data` were listed as project references in the README but neither exists in the solution. The README also described the frontend build as using Gulp — it was replaced by esbuild some time ago. Another good example of why this documentation effort is needed.

---

## `contentful/` tooling

### `delete-all-content.js` has no confirmation prompt

`content-management/delete-all-content.js` executes destructive deletions immediately with no "are you sure?" step. The `content-migrations` library correctly requires a `Y/N` confirmation before applying changes; the management scripts have no such guard. One mistyped `ENVIRONMENT` variable and you're wiping production content silently. At minimum this should prompt for confirmation or require a `--confirm` flag.

### CommonJS and ES modules are mixed in `content-management`

`content-management` mixes CommonJS (`require()`/`module.exports`) and ES module (`import`/`export`) syntax across different files. This works in current Node.js but is fragile — `"type": "module"` in `package.json` would break all `require()` calls, and Jest's configuration is already doing non-trivial gymnastics with `--experimental-vm-modules` to handle it. The codebase should be unified on one module format.

### `export-processor` is referenced as a local file path dependency

`broken-link-checker/package.json` declares `"export-processor": "file:../export-processor"` as a dependency. Local path dependencies are brittle in CI: the relative path must be correct from wherever `npm install` is run, `npm ci` may not handle them correctly, and the dependency is not pinned to a version. This should be handled via npm workspaces or a dedicated monorepo tool (e.g. Turborepo) that understands the relationship.

### `export-recommendations-csv` requires a 9 GB heap

`export-processor` runs the recommendations CSV export with `--max-old-space-size=9000`. This strongly suggests a memory efficiency problem in the data processing code rather than a genuine requirement for 9 GB. The `DataMapper` builds an in-memory object graph of the entire Contentful export, and the recommendations CSV code then iterates over it in a memory-intensive way. A streaming or incremental approach would almost certainly reduce this by orders of magnitude.

### `DRY_RUN = true` hardcoded in the most recent change script

`content-management/changes/20260119-1250-remove-underlining-from-richtext.js` has `const DRY_RUN = true` at the top, meaning it logs what it would do but never actually makes any changes. This appears to be the most recently created script, and it has never been run in anger. Leaving a change script in dry-run mode and committing it is confusing — it looks like the change has been applied when it hasn't. The `DRY_RUN` flag should either be removed (and the script actually run) or the script should be deleted if the change is no longer needed.

### `qa-visualiser` is the only Python project in the repository

Every other tool in this repository is Node.js or .NET. `qa-visualiser` uses Python 3.12, `uv`, Pydantic, and Graphviz. This adds an entirely separate runtime, toolchain, and dependency management system that contributors must have set up. In a team that is primarily .NET and Node.js, the Python toolchain will be unknown to most developers, making this tool less likely to be maintained or extended. It would be straightforward to rewrite using a Node.js graph library (e.g. `mermaid` or `d3`) or a .NET library (`Graphviz.NET`, `QuikGraph`).
