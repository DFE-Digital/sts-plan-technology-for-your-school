---
applyTo: 'tests/**'
description:
  'Mandatory standards for writing .NET unit and integration tests in Plan Technology for Your
  School.'
---

# .NET Testing Standards

These rules are mandatory for everyone writing tests, including AI coding agents. "Must" means
required; "prefer" means default unless there is a stated reason. If a rule cannot be followed
because of how production code is written, **stop and flag it** (or make the minimal production
change described in §10/§14). Never work around a rule.

---

## 1. Scope

Applies to all .NET projects under `tests/`:

- `Dfe.PlanTech.*.UnitTests`
- `Dfe.PlanTech.Data.Sql.IntegrationTests`
- `Dfe.PlanTech.Tests.Shared` (shared test library)
- `Dfe.PlanTech.Web.SeedTestData` (§21 only)

Not covered: Cypress/Playwright E2E suites and Jest tests.

---

## 2. Toolchain

| Purpose        | Use                                                              | Never use                                                       |
| -------------- | ---------------------------------------------------------------- | --------------------------------------------------------------- |
| Test framework | xUnit v3                                                         | MSTest, NUnit                                                   |
| Test doubles   | NSubstitute                                                      | Moq, FakeItEasy, hand-written mocks of our own interfaces       |
| Assertions     | xUnit `Assert`                                                   | FluentAssertions, Shouldly, AwesomeAssertions                   |
| Loggers        | `FakeLogger<T>` (`Microsoft.Extensions.Diagnostics.Testing`)     | `Substitute.For<ILogger<T>>()`, `NullLogger<T>`                 |
| Time           | `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`) | `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`         |
| Options        | `Options.Create(new XOptions { ... })`                           | `Substitute.For<IOptions<T>>()`                                 |
| Test data      | Builders in `Dfe.PlanTech.Tests.Shared`                          | AutoFixture, Bogus, per-file `Make*`/`Build*`/`Create*` helpers |
| Database       | Testcontainers (`DatabaseFixture`)                               | EF Core InMemory, SQLite                                        |
| Formatting     | CSharpier (`dotnet csharpier format .`)                          | —                                                               |

Package versions and common settings live in `tests/Directory.Build.props`. Test `.csproj` files
must not declare their own versions of shared packages.

---

## 3. Project and file structure

1. **One test project per production project:** `src/Dfe.PlanTech.{Name}` →
   `tests/Dfe.PlanTech.{Name}.UnitTests`. Anything needing a real database goes in
   `tests/Dfe.PlanTech.Data.Sql.IntegrationTests`.
2. **Folders mirror `src`:** `src/Dfe.PlanTech.Web/ViewBuilders/QuestionsViewBuilder.cs` →
   `tests/Dfe.PlanTech.Web.UnitTests/ViewBuilders/QuestionsViewBuilderTests.cs`
3. **File name = class name = `{ProductionClass}Tests`.** One test class per file. Namespace =
   project root namespace + folder path. Use **file-scoped namespaces**.
4. **Test one production class per test class.** Never put tests for one production class in two
   test classes (except §3.5), and never in two folders.
5. **Splitting large test classes:** if a test class would exceed **~500 lines**, replace it with a
   folder:
   ```
   ViewBuilders/QuestionsViewBuilderTests/
       QuestionsViewBuilderTestBase.cs      // abstract: fields + CreateSystemUnderTest()
       RouteByQuestionIdTests.cs             // : QuestionsViewBuilderTestBase
       SubmitAnswerAndRedirectTests.cs       // : QuestionsViewBuilderTestBase
   ```
   This is the **only** situation where test-class inheritance is allowed.
6. **Shared test library (`Dfe.PlanTech.Tests.Shared`)** is a class library (not a test project)
   containing only:
   - `Builders/` – test data builders (§17)
   - `Fakes/` – reusable fake implementations (§18)
   - `Assertions/` – reusable assertion helpers (§16, §15)

   Before writing any helper inside a test class, check the shared library. If a helper would be
   used by two or more test classes, it must go in the shared library.

---

## 4. Test class layout

Members appear in this order, with no `#region` blocks and no banner comments (`// ---- X ----`):

1. `const` values
2. Test-double fields (`private readonly`)
3. Constructor — **only** if it sets up default stubs needed by every test
4. `CreateSystemUnderTest()`
5. Tests, grouped by production member in production declaration order (constructor test first)
6. Private helpers (rare — prefer builders)

```csharp
using Dfe.PlanTech.Application.Providers;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Tests.Shared.Builders.Contentful;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Providers;

public class MicrocopyProviderTests
{
    private const string MissingKey = "missing-key";

    private readonly FakeLogger<MicrocopyProvider> _logger = new();
    private readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();

    private MicrocopyProvider CreateSystemUnderTest() => new(_logger, _contentfulService);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<MicrocopyProvider>(
            _logger,
            _contentfulService
        );
    }

    [Fact]
    public async Task GetRecordByKeyAsync_WhenKeyIsMissing_ReturnsNullAndLogsWarning()
    {
        // Arrange
        _contentfulService.GetMicrocopyEntriesAsync().Returns(new List<MicrocopyEntry>());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.GetRecordByKeyAsync(MissingKey);

        // Assert
        Assert.Null(result);
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Warning, logRecord.Level);
        Assert.Contains(MissingKey, logRecord.Message);
    }

    [Fact]
    public async Task GetRecordByKeyAsync_WhenCalledRepeatedly_FetchesEntriesOnce()
    {
        // Arrange
        var entry = new MicrocopyEntryBuilder().WithKey("key-1").Build();
        _contentfulService.GetMicrocopyEntriesAsync().Returns(new List<MicrocopyEntry> { entry });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.GetRecordByKeyAsync("key-1");
        await systemUnderTest.GetRecordByKeyAsync("key-1");

        // Assert
        await _contentfulService.Received(1).GetMicrocopyEntriesAsync();
    }
}
```

---

## 5. Naming

### 5.1 Test methods: `{Member}_{Scenario}_{ExpectedResult}`

- Exactly **three** segments separated by underscores; PascalCase **within** each segment (no
  underscores inside a segment).
- `{Member}` is the exact production member name, including any `Async` suffix. Use `Constructor`
  for constructor tests.
- `{Scenario}` starts with `When`.
- `{ExpectedResult}` starts with a verb: `Returns…`, `Throws…`, `Logs…`, `Sets…`, `Redirects…`,
  `Calls…`, `Saves…`, `DoesNot…`.

| ✅ Good                                                            | ❌ Bad                                                      |
| ------------------------------------------------------------------ | ----------------------------------------------------------- |
| `GetRecordByKeyAsync_WhenKeyIsMissing_ReturnsNull`                 | `Should_Return_Null_When_Missing`                           |
| `RouteByQuestionId_WhenPreviewIsDisabled_RedirectsToHomePage`      | `RouteByQuestionId_When_Preview_Disabled_Redirects_To_Home` |
| `Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException`  | `Ctor_NullWorkflow_Throws`                                  |
| `CloneSubmission_WhenSubmissionIsNull_ThrowsArgumentNullException` | `UserRepository_CreateUser_WhenValid_ThenCreates`           |

### 5.2 Variables and members

- **No abbreviations.** Use `systemUnderTest`, `httpContext`, `repository`, `question`, `request` —
  never `sut`, `ctx`, `svc`, `repo`, `q1`, `req`, `sp`.
- The thing being tested: local `systemUnderTest`, created by `CreateSystemUnderTest()`.
- The value returned by the Act step: `result`.
- Expected values: `expected{Thing}` (e.g. `expectedSectionId`).

### 5.3 Test doubles

Name fields after the dependency, without the `I` prefix and **without**
`Mock`/`Substitute`/`Stub`/`Fake` suffixes: `_contentfulService`, `_submissionRepository`,
`_logger`, `_timeProvider`.

### 5.4 Shared library types

- Builders: `{Type}Builder` (e.g. `SubmissionEntityBuilder`)
- Fakes: `Fake{InterfaceWithoutI}` (e.g. `FakeSession`), or `Test{FrameworkType}` for framework base
  classes (e.g. `TestController`)
- Assertion helpers: `{Concern}Assert` (e.g. `ConstructorGuardAssert`)

---

## 6. Test body

1. **Arrange/Act/Assert comments are required:** `// Arrange`, `// Act`, `// Assert`.
   - Omit `// Arrange` only if there is nothing to arrange.
   - Use `// Act & Assert` only when the Act is inside `Assert.Throws`/`ThrowsAsync`.
2. **Exactly one Act** per test — a single call to the system under test (a deliberate repeat call
   to test caching or idempotency is allowed).
3. **One behaviour per test.** Multiple assertions are fine if they all describe that one behaviour.
4. **No logic in tests:** no `if`, `switch`, loops, `try/catch`, or LINQ that computes expected
   values.
5. **Deterministic data only.** Never use `DateTime.Now/UtcNow`, `Guid.NewGuid()`, `Random`, or
   environment-dependent values. Use literals or `const` fields.
6. **Meaningful literals:** `"category-slug"`, `"missing-key"`, `42` — not `"test"`, `"abc"`,
   `"string"`.
7. **Arrange only what matters.** Let builders provide defaults; set only the properties that the
   test is about.
8. No commented-out code, `TODO`s, `Console.WriteLine`, or skipped tests (`Skip =`) without a linked
   issue.

---

## 7. Creating the system under test

- Test doubles are `private readonly` fields, initialised inline.
- Each test class has exactly one `private {Type} CreateSystemUnderTest(...)` method.
- Each test calls it: `var systemUnderTest = CreateSystemUnderTest();`
- **Never** create the system under test in the constructor, a field initialiser, or a shared field.
- Substitutes are configured per test; they are **never replaced**.
- Configuration/option values that vary between tests are passed as **optional parameters** to
  `CreateSystemUnderTest`:

```csharp
private QuestionsViewBuilder CreateSystemUnderTest(
    ContentfulOptionsConfiguration? contentfulOptions = null
) =>
    new(
        _logger,
        _contentfulService,
        _currentUserProvider,
        Options.Create(new ContactOptionsConfiguration { LinkId = "contact-link-id" }),
        Options.Create(new ErrorMessagesConfiguration { ConcurrentUsersOrContentChange = "error" }),
        contentfulOptions ?? new ContentfulOptionsConfiguration { UsePreviewApi = false },
        _questionService,
        _submissionService,
        _establishmentService,
        _matEstablishmentProvider
    );
```

Every test-double field must be used by `CreateSystemUnderTest`. Delete unused fields.

---

## 8. Test doubles (NSubstitute)

1. Substitute **only interfaces**, plus abstract SDK types designed for mocking (e.g.
   `ServiceBusProcessor`, `ServiceBusReceiver`). Never substitute the class under test, concrete
   classes we own, entities, DTOs, or Contentful models — build them (§17).
2. **Stub with explicit arguments.** Use `Arg.Any<T>()` only when the argument genuinely doesn't
   matter to the test.
3. **Verify interactions only when the interaction _is_ the behaviour** (commands and side effects:
   save, delete, send, enqueue, cache write). For queries, assert on the returned value — do not
   also verify the call that produced it.
4. Always give explicit counts: `Received(1)`. Use `DidNotReceive()` / `DidNotReceiveWithAnyArgs()`
   for negatives.
5. Match complex arguments with `Arg.Is<T>(x => ...)`. For collections:
   `Arg.Is<int[]>(ids => ids.SequenceEqual(expectedIds))`.
6. Async: `.Returns(value)`; exceptions via `NSubstitute.ExceptionExtensions`
   (`.ThrowsAsync(new X())`).
7. **Forbidden:** verifying calls by method-name string, reading `ReceivedCalls()` arguments by
   index, or `When(...).Do(...)` to capture arguments when `Arg.Is`/`Arg.Do` would work.

---

## 9. Logging

- Always pass a `FakeLogger<T>` as the logger; never substitute `ILogger<T>`.
- **Only assert on logs when logging is the observable behaviour** — warning/error paths, audit
  events, or swallowed exceptions. Do not assert on informational/debug logs.
- Assert the level and a meaningful fragment of the message:

```csharp
var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
Assert.Equal(LogLevel.Error, logRecord.Level);
Assert.Contains("section-id", logRecord.Message);
```

- Logging tests live in the test class of the class that logs. There is no separate logging test
  project.

---

## 10. Time

- Production code must depend on `TimeProvider` (registered as `TimeProvider.System`) and call
  `timeProvider.GetUtcNow()`. If you find production code using `DateTime.UtcNow`/`Now`, refactor it
  to inject `TimeProvider` before writing the test.
- Tests use a `FakeTimeProvider` field with a fixed start time:

```csharp
private static readonly DateTimeOffset Now = new(2025, 1, 15, 9, 0, 0, TimeSpan.Zero);
private readonly FakeTimeProvider _timeProvider = new(Now);
```

- Use `_timeProvider.Advance(TimeSpan.FromMinutes(5))` to move time. Every date in a test must be
  derived from a fixed value.

---

## 11. Assertions

**Use the most specific assertion available:**

| Need                         | Use                                                                                         |
| ---------------------------- | ------------------------------------------------------------------------------------------- |
| Equality                     | `Assert.Equal(expected, actual)` — expected first                                           |
| Same instance (pass-through) | `Assert.Same(expected, result)`                                                             |
| Exactly one item             | `var item = Assert.Single(collection);` then assert on `item`                               |
| Ordered items                | `Assert.Collection(...)` or `Assert.Equal(expectedArray, result.Select(...).ToArray())`     |
| Contains                     | `Assert.Contains(item, collection)` / `Assert.Contains(collection, predicate)`              |
| Type + cast                  | `var view = Assert.IsType<ViewResult>(result);`                                             |
| Whole object graph           | `Assert.Equivalent(expected, actual)`                                                       |
| Exceptions                   | `var exception = await Assert.ThrowsAsync<T>(() => ...);` then assert `Message`/`ParamName` |
| Does not throw               | `var exception = Record.Exception(() => ...); Assert.Null(exception);`                      |

**Forbidden:**

- `Assert.True(true)` or any assertion that can't fail
- Asserting on a literal (`Assert.IsType<string>("text")`)
- `Assert.NotNull(x)` as the **only** assertion
- `Assert.True(a == b)` → use `Assert.Equal`
- `Assert.True(list.Any(...))` → use `Assert.Contains`
- Tests that arrange a value but never assert on it

---

## 12. Parameterised tests

- Use `[Theory]` when the **same behaviour** is checked against multiple inputs.
- `[InlineData]` for compile-time constants; `TheoryData<...>` + `[MemberData]` for objects.
- The test name describes the category of input (`Slugify_WhenInputContainsPunctuation_RemovesIt`).
- Add a trailing comment to any row whose purpose isn't obvious.
- Do **not** use a `bool`/enum parameter to switch between different behaviours — write separate
  tests.

---

## 13. Async and concurrency

- Tests are `async Task`; never `async void`, `.Result`, or `.Wait()`.
- Pass `TestContext.Current.CancellationToken` to any method that accepts a `CancellationToken`.
- Never use `Task.Delay` or `Thread.Sleep` to wait for something. Use a `TaskCompletionSource` and
  `await signal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)`.
- Test hosted/background services through `StartAsync`/`StopAsync`.

---

## 14. Non-public members

- Test through the public API.
- **Reflection is forbidden** for calling non-public members
  (`GetMethod(..., BindingFlags.NonPublic)`, `Invoke`, etc.). The only permitted reflection is
  inside the shared assertions in §16 and §15 (DTO coverage).
- If logic genuinely needs direct testing, make it `internal` (test projects are granted access via
  `InternalsVisibleTo` in `src/Directory.Build.props`) or extract it into its own class.
- To test an abstract base class, declare a minimal
  `private sealed class Testable{BaseClass} : {BaseClass}` inside the test file.

---

## 15. What to test, by component

| Component                                               | Approach                                                                                                                                                          |
| ------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Core helpers, extensions, models                        | Pure input → output. Prefer theories. Cover edge cases (null, empty, boundaries).                                                                                 |
| Entity `AsDto()` mappings (`Data.Sql`)                  | One test per entity. Set **every** property to a distinct fixed value; assert each; finish with `DtoPropertyCoverageAssert.AllPropertiesAccountedFor<TDto>(...)`. |
| Repositories, `DbContext`, stored procedures            | **Integration tests only** (§19).                                                                                                                                 |
| Contentful repository / query builders                  | Unit tests with substituted `IContentfulClient`. Assert on the built query.                                                                                       |
| Application workflows                                   | Unit tests with substituted repositories. Test every business rule and branch.                                                                                    |
| Application services / providers                        | Unit tests. Pass-throughs follow the light rule below.                                                                                                            |
| Rich-text / markdown renderers                          | Assert the exact HTML string.                                                                                                                                     |
| Controllers                                             | Usually pass-throughs → light rule. Test any real logic (model-state handling, redirects, validation) fully.                                                      |
| View builders                                           | Full behavioural tests. Assert result type, view name, redirect target and view-model contents.                                                                   |
| Tag helpers, view components                            | Process the output; assert tag name, attributes and content. Use shared context/output factories.                                                                 |
| Middleware, filters, attributes, authorisation handlers | Build `HttpContext` with `HttpContextBuilder`; assert response status/headers, `context.Items`, success/failure state, and whether `next` was called.             |
| Redis, Service Bus, Sign-in infrastructure              | Unit tests with SDK abstractions substituted.                                                                                                                     |
| DatabaseUpgrader                                        | Argument parsing; all scripts are embedded and follow naming/ordering rules.                                                                                      |

**Light rule for pass-throughs.** A pass-through is a method whose body only calls one dependency
and returns its result unchanged. Write **one** test per such method:

```csharp
[Fact]
public async Task GetAllSectionsAsync_WhenCalled_ReturnsWorkflowResult()
{
    // Arrange
    var expectedSections = new List<QuestionnaireSectionEntry>();
    _contentfulWorkflow.GetAllSectionsAsync().Returns(expectedSections);
    var systemUnderTest = CreateSystemUnderTest();

    // Act
    var result = await systemUnderTest.GetAllSectionsAsync();

    // Assert
    Assert.Same(expectedSections, result);
}
```

**Do not test:** auto-properties, constants, records/POCOs without logic, framework behaviour, DI
registration without conditional logic, Razor views.

---

## 16. Constructor null-guard tests

Each class with constructor guards gets **one** test per public constructor, which checks all
parameters at once:

```csharp
[Fact]
public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
{
    // Act & Assert
    ConstructorGuardAssert.AllNonNullableParametersAreGuarded<GroupsController>(
        _logger,
        _currentUserProvider,
        _groupsViewBuilder,
        _groupSelectSchoolsToAssessValidator,
        _groupSelectSchoolsToUpdateStatusValidator,
        _pagesViewBuilder
    );
}
```

The helper replaces each non-nullable reference parameter with `null` in turn and asserts
`ArgumentNullException` with the matching `ParamName`. Parameters declared nullable (`IFoo?`) are
skipped. If a non-nullable parameter isn't guarded, the test fails — fix the production code rather
than the test.

Shared implementation (`Dfe.PlanTech.Tests.Shared/Assertions/ConstructorGuardAssert.cs`):

```csharp
using System.Reflection;
using Xunit;

namespace Dfe.PlanTech.Tests.Shared.Assertions;

public static class ConstructorGuardAssert
{
    public static void AllNonNullableParametersAreGuarded<T>(params object?[] validArguments)
    {
        var constructor = typeof(T)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Single(candidate => ArgumentsMatch(candidate.GetParameters(), validArguments));

        var nullabilityContext = new NullabilityInfoContext();

        foreach (var parameter in constructor.GetParameters())
        {
            if (parameter.ParameterType.IsValueType)
            {
                continue;
            }

            if (nullabilityContext.Create(parameter).WriteState == NullabilityState.Nullable)
            {
                continue;
            }

            var arguments = (object?[])validArguments.Clone();
            arguments[parameter.Position] = null;

            var exception = Record.Exception(() => constructor.Invoke(arguments));
            var innerException = exception is TargetInvocationException invocationException
                ? invocationException.InnerException
                : exception;

            if (innerException is not ArgumentNullException argumentNullException)
            {
                Assert.Fail(
                    $"{typeof(T).Name}: parameter '{parameter.Name}' is not null-guarded."
                );
                return;
            }

            Assert.Equal(parameter.Name, argumentNullException.ParamName);
        }
    }

    private static bool ArgumentsMatch(ParameterInfo[] parameters, object?[] arguments) =>
        parameters.Length == arguments.Length
        && parameters
            .Zip(arguments)
            .All(pair =>
                pair.Second is null
                    ? !pair.First.ParameterType.IsValueType
                    : pair.First.ParameterType.IsInstanceOfType(pair.Second)
            );
}
```

---

## 17. Test data builders

All entities, DTOs and Contentful models used in tests are created with builders from
`Dfe.PlanTech.Tests.Shared/Builders/{Area}/` (`Sql/`, `Contentful/`, `Models/`, `ViewModels/`).

Rules:

1. `new XBuilder()` produces a **valid, deterministic** object with no further calls.
2. Fluent `With{Property}(...)` methods return `this`; collection methods take `params`.
3. `Build()` returns a **new** instance on every call.
4. No randomness, no `DateTime.Now`, no `Guid.NewGuid()` — use fixed defaults.
5. SQL entity builders default `Id` to `0` (so EF assigns it in integration tests); unit tests set
   `WithId(...)` explicitly.
6. Linked Contentful graphs (section → question → answer → next question) are built with dedicated
   builder methods. Never use `out` parameters.
7. Builders never depend on test classes.

```csharp
namespace Dfe.PlanTech.Tests.Shared.Builders.Sql;

public class SubmissionEntityBuilder
{
    private static readonly DateTime DefaultDateCreated = new(2025, 1, 15, 9, 0, 0, DateTimeKind.Utc);

    private int _id;
    private int _establishmentId = 1;
    private string _sectionId = "section-id";
    private string _sectionName = "Section name";
    private SubmissionStatus _status = SubmissionStatus.InProgress;
    private DateTime _dateCreated = DefaultDateCreated;
    private readonly List<ResponseEntity> _responses = [];

    public SubmissionEntityBuilder WithId(int id) { _id = id; return this; }
    public SubmissionEntityBuilder WithEstablishmentId(int establishmentId) { _establishmentId = establishmentId; return this; }
    public SubmissionEntityBuilder WithSectionId(string sectionId) { _sectionId = sectionId; return this; }
    public SubmissionEntityBuilder WithStatus(SubmissionStatus status) { _status = status; return this; }
    public SubmissionEntityBuilder WithDateCreated(DateTime dateCreated) { _dateCreated = dateCreated; return this; }
    public SubmissionEntityBuilder WithResponses(params ResponseEntity[] responses) { _responses.AddRange(responses); return this; }

    public SubmissionEntity Build() =>
        new()
        {
            Id = _id,
            EstablishmentId = _establishmentId,
            SectionId = _sectionId,
            SectionName = _sectionName,
            Status = _status,
            DateCreated = _dateCreated,
            Responses = [.. _responses],
        };
}
```

Usage:
`var submission = new SubmissionEntityBuilder().WithStatus(SubmissionStatus.CompleteReviewed).Build();`

---

## 18. Shared fakes

Use these from `Dfe.PlanTech.Tests.Shared/Fakes/`. Never redeclare them inside a test file.

| Fake                       | Purpose                                                                                   |
| -------------------------- | ----------------------------------------------------------------------------------------- |
| `FakeSession`              | In-memory `ISession`                                                                      |
| `TestController`           | `Controller` with `HttpContext`, `TempData` and `Session` wired up; optional request path |
| `HttpContextBuilder`       | Builds `DefaultHttpContext` with user claims, headers, path, `Items` and session          |
| `ClaimsPrincipalBuilder`   | Authenticated/unauthenticated principals with claims                                      |
| `FakeUserActionIdProvider` | Deterministic `IUserActionIdProvider`                                                     |
| `TagHelperTestContext`     | Factory for `TagHelperContext`/`TagHelperOutput`                                          |

Shared assertions (`Assertions/`): `ConstructorGuardAssert`, `DtoPropertyCoverageAssert`.

---

## 19. SQL integration tests

1. All `DbContext`, repository and stored-procedure tests live in
   `Dfe.PlanTech.Data.Sql.IntegrationTests` and run against SQL Server via Testcontainers. EF
   InMemory and SQLite are forbidden.
2. Test classes inherit `DatabaseIntegrationTestBase`. Each test runs inside a transaction that is
   rolled back, so tests must not depend on each other or on execution order.
3. Create the repository in `InitializeAsync` override **or** via `CreateSystemUnderTest()` using
   the base `DbContext`. Use the latter for consistency with unit tests.
4. Arrange data with builders and `DbContext.Add(...)` +
   `SaveChangesAsync(TestContext.Current.CancellationToken)`.
5. **Never assume generated IDs.** Read them from the saved entity.
6. Before asserting on persisted state, call `DbContext.ChangeTracker.Clear()` and re-query, so you
   read from the database rather than tracked entities.
7. For database-generated timestamps (`SYSUTCDATETIME()`), assert with a tolerance window captured
   around the Act step — the only allowed use of real time.
8. The same naming, Arrange/Act/Assert and assertion rules apply.

---

## 20. Coverage

- Target **90% line coverage** per production project. (The SonarCloud quality gate enforces 80% on
  new code; 90% is the team standard.)
- Coverage must come from meaningful behavioural tests. Writing assertion-free tests just to execute
  lines is forbidden.
- Exclusions are configured only in the Sonar settings (`Program.cs`, `wwwroot`, `SeedTestData`).
  Don't add `[ExcludeFromCodeCoverage]` to avoid testing.

---

## 21. SeedTestData

`Dfe.PlanTech.Web.SeedTestData` is a console tool for seeding local databases, not a test project.
Test projects must not reference it, and tests must not depend on seeded data — builders are the
only source of test data.

---

## 22. Formatting and hygiene

- Run `dotnet csharpier format .` before committing.
- Tests must build with no warnings (including xUnit analyser warnings).
- `using` directives go outside the namespace, with `System` usings first; no unused usings.

---

## 23. Checklist before finishing (mandatory for AI agents)

- [ ] File path mirrors `src`; file name = class name = `{ProductionClass}Tests`; file-scoped
      namespace
- [ ] Layout order follows §4; no regions or banner comments
- [ ] Every test is named `{Member}_{When…}_{Verb…}`
- [ ] Every test has `// Arrange`, `// Act`, `// Assert` and exactly one Act
- [ ] `CreateSystemUnderTest()` used; local named `systemUnderTest`; no abbreviations anywhere
- [ ] All test-double fields are used; none have `Mock`/`Substitute` suffixes
- [ ] Data built with shared builders; no per-file `Make*`/`Build*`/`Create*` data helpers
- [ ] No `DateTime.Now/UtcNow`, `Guid.NewGuid()`, `Random`, `Task.Delay`, `Thread.Sleep`
- [ ] No reflection on non-public members; no `Assert.True(true)`; no `NotNull`-only tests
- [ ] Loggers are `FakeLogger<T>`; options use `Options.Create`; time uses `FakeTimeProvider`
- [ ] Constructor guards tested with one `ConstructorGuardAssert` test per constructor
- [ ] Interactions verified only for side effects, with explicit `Received(n)`
- [ ] No duplicated fakes; any helper used by 2+ test classes lives in the shared library
- [ ] `dotnet test` passes and coverage for the project is ≥ 90%
- [ ] `dotnet csharpier format .` has been run
