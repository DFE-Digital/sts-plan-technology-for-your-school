using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Infrastructure.SignIn.ConnectEvents;
using Dfe.PlanTech.UnitTests.Shared.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.SignIn.UnitTests.ConnectEvents;

public class OnUserInformationReceivedEventTests
{
    private static (
        UserInformationReceivedContext ctx,
        ISignInWorkflow wf,
        IGiasRepository gias,
        IDsiOrganisationProvider dsiProvider,
        ILogger<IDfeSignIn> logger
    ) BuildContext(ClaimsPrincipal principal)
    {
        var services = new ServiceCollection();
        var signIn = Substitute.For<ISignInWorkflow>();
        var gias = Substitute.For<IGiasRepository>();
        var dsiProvider = Substitute.For<IDsiOrganisationProvider>();
        services.AddSingleton(signIn);
        services.AddSingleton(gias);
        services.AddSingleton(dsiProvider);

        var sp = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = sp };
        var scheme = new AuthenticationScheme(
            OpenIdConnectDefaults.AuthenticationScheme,
            null,
            typeof(OpenIdConnectHandler)
        );
        var options = new OpenIdConnectOptions();
        var properties = new AuthenticationProperties();

        var ctx = Substitute.For<UserInformationReceivedContext>(
            httpContext,
            scheme,
            options,
            principal,
            properties
        );

        var logger = Substitute.For<ILogger<IDfeSignIn>>();
        return (ctx, signIn, gias, dsiProvider, logger);
    }

    private static ClaimsPrincipal AuthenticatedPrincipal(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, authenticationType: "cookies");
        return new ClaimsPrincipal(identity);
    }

    private static EstablishmentModel CreateEstablishment(
        string? urn = null,
        string? uid = null,
        string? categoryId = null
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            Urn = urn,
            Uid = uid,
            Category = categoryId is null
                ? null
                : new IdWithNameModel { Id = categoryId, Name = "Category" },
        };

    private static Claim OrganisationClaim(EstablishmentModel establishment) =>
        new(ClaimConstants.Organisation, JsonSerializer.Serialize(establishment));

    private static GiasEstablishmentEntity CreateSchool(int urn) =>
        new()
        {
            Urn = urn,
            EstablishmentName = "Test School",
            EstablishmentStatusCode = 1,
            LocalAuthorityCode = 1,
            PhaseCode = 1,
            TypeOfEstablishmentCode = 1,
            SyncedAt = DateTime.UtcNow,
        };

    [Fact]
    public async Task RecordUserSignIn_When_NotAuthenticated_FailsContext()
    {
        // Arrange: no principal OR unauthenticated identity
        var (ctx, signIn, _, _, logger) = BuildContext(
            principal: new ClaimsPrincipal(new ClaimsIdentity())
        ); // IsAuthenticated = false

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert
        Assert.NotNull(ctx.Result);
        Assert.False(ctx.Result!.Succeeded);
        Assert.NotNull(ctx.Result.Failure);
        await signIn.DidNotReceiveWithAnyArgs().RecordSignIn(default!, default!);
        await signIn.DidNotReceiveWithAnyArgs().RecordSignInUserOnly(default!);
    }

    [Fact]
    public async Task RecordUserSignIn_When_DsiReferencecIsNull_LogsError_And_Fails()
    {
        // Arrange: authenticated principal but no establishment-related claims
        var principal = AuthenticatedPrincipal([]);
        var (ctx, signIn, _, _, logger) = BuildContext(principal);

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert
        var logMessage = logger.ReceivedLogMessages().FirstOrDefault();
        Assert.NotNull(logMessage);
        Assert.Equal(LogLevel.Error, logMessage.LogLevel);
        Assert.Equal(
            "Authentication failed: no nameidentifier claim found for user.",
            logMessage.Message
        );

        ctx.Received(1).Fail("No nameidentifier claim present in user principal.");

        // Context failed
        Assert.NotNull(ctx.Result.Failure);
    }

    [Fact]
    public async Task RecordUserSignIn_When_EstablishmentMissing_LogsWarning_And_RecordsUserOnly()
    {
        // Arrange: authenticated principal but no establishment-related claims
        var principal = AuthenticatedPrincipal(
            new Claim(ClaimConstants.NameIdentifier, "dsi-ref-123")
        );
        var (ctx, wf, _, _, logger) = BuildContext(principal);

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert: since GetOrganisation() returns null with no org claims, we should call the user-only workflow
        await wf.Received(1).RecordSignInUserOnly(Arg.Any<string>());
        await wf.DidNotReceiveWithAnyArgs().RecordSignIn(default!, default!);

        // Context not failed
        Assert.True(ctx.Result is null);
    }

    [Fact]
    public async Task RecordUserSignIn_WhenNonSatOrganisationPresent_RecordsSignIn_AndAddsDbClaims()
    {
        // Arrange: authenticated principal with a non-SAT/SSAT organisation claim
        var establishment = CreateEstablishment(
            urn: "111111",
            categoryId: DsiConstants.EstablishmentCategoryId
        );
        var principal = AuthenticatedPrincipal(
            new Claim(ClaimConstants.NameIdentifier, "dsi-ref-123"),
            OrganisationClaim(establishment)
        );

        var (ctx, signIn, gias, dsiProvider, logger) = BuildContext(principal);

        var signInDto = new SqlSignInDto { UserId = 1, EstablishmentId = 2 };
        signIn.RecordSignIn(Arg.Any<string>(), Arg.Any<EstablishmentModel>()).Returns(signInDto);

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert: normal establishments never go through the SAT replacement lookup
        await signIn
            .Received(1)
            .RecordSignIn("dsi-ref-123", Arg.Is<EstablishmentModel>(e => e.Urn == "111111"));
        await signIn.DidNotReceiveWithAnyArgs().RecordSignInUserOnly(default!);
        await gias.DidNotReceiveWithAnyArgs().GetSingleAcademySchool(default);
        await dsiProvider
            .DidNotReceiveWithAnyArgs()
            .GetOrganisationForUserAsync(default!, default!);

        Assert.True(ctx.Result is null);

        var userIdClaim = ctx.Principal!.Claims.FirstOrDefault(c =>
            c.Type == ClaimConstants.DB_USER_ID
        );
        Assert.NotNull(userIdClaim);
        Assert.Equal("1", userIdClaim!.Value);

        // No single-academy claim should be added outside the SAT/SSAT path
        var satClaim = ctx.Principal.Claims.FirstOrDefault(c =>
            c.Type == ClaimConstants.SINGLE_ACADEMY_ORGANISATION
        );
        Assert.Null(satClaim);
    }

    [Fact]
    public async Task RecordUserSignIn_WhenSatOrganisation_AndReplacementFound_UsesReplacement_AndAddsSingleAcademyClaim()
    {
        // Arrange: a SAT-category organisation whose Uid resolves to a GIAS school,
        // which in turn resolves to a replacement establishment via the DSI API.
        var originalOrg = CreateEstablishment(
            uid: "555",
            categoryId: DsiConstants.SatOrganisationCategoryId
        );
        var originalOrgClaim = OrganisationClaim(originalOrg);
        var principal = AuthenticatedPrincipal(
            new Claim(ClaimConstants.NameIdentifier, "dsi-ref-456"),
            originalOrgClaim
        );

        var (ctx, signIn, gias, dsiProvider, logger) = BuildContext(principal);

        var school = CreateSchool(777777);
        gias.GetSingleAcademySchool(555).Returns(school);

        var replacementOrg = CreateEstablishment(
            urn: "777777",
            categoryId: DsiConstants.EstablishmentCategoryId
        );
        dsiProvider.GetOrganisationForUserAsync("dsi-ref-456", "777777").Returns(replacementOrg);

        var signInDto = new SqlSignInDto { UserId = 10, EstablishmentId = 20 };
        signIn.RecordSignIn(Arg.Any<string>(), Arg.Any<EstablishmentModel>()).Returns(signInDto);

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert
        await signIn
            .Received(1)
            .RecordSignIn(
                "dsi-ref-456",
                Arg.Is<EstablishmentModel>(e =>
                    e.Uid == "555" && e.Category!.Id == DsiConstants.SatOrganisationCategoryId
                )
            );
        await gias.Received(1).GetSingleAcademySchool(555);
        await dsiProvider.Received(1).GetOrganisationForUserAsync("dsi-ref-456", "777777");

        var satClaim = ctx.Principal!.Claims.FirstOrDefault(c =>
            c.Type == ClaimConstants.SINGLE_ACADEMY_ORGANISATION
        );
        Assert.NotNull(satClaim);

        var deserialised = JsonSerializer.Deserialize<EstablishmentModel>(satClaim!.Value);
        Assert.Equal("777777", deserialised!.Urn);
    }

    [Fact]
    public async Task RecordUserSignIn_WhenSSatOrganisation_AndSchoolNotFound_ThrowsWithSSatMessage()
    {
        // Arrange
        var org = CreateEstablishment(
            uid: "42",
            categoryId: DsiConstants.SSatOrganisationCategoryId
        );
        var principal = AuthenticatedPrincipal(
            new Claim(ClaimConstants.NameIdentifier, "dsi-ref-ssat"),
            OrganisationClaim(org)
        );

        var (ctx, signIn, gias, _, logger) = BuildContext(principal);

        var signInDto = new SqlSignInDto
        {
            Id = 1,
            UserId = 1,
            EstablishmentId = 1,
        };
        signIn.RecordSignIn(Arg.Any<string>(), Arg.Any<EstablishmentModel>()).Returns(signInDto);

        gias.GetSingleAcademySchool(42).Returns((GiasEstablishmentEntity?)null);

        // Act / Assert
        var ex = await Assert.ThrowsAsync<InvalidGiasDataException>(() =>
            OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx)
        );

        Assert.Contains("SSAT", ex.Message);
        Assert.Contains("42", ex.Message);
    }

    [Fact]
    public async Task RecordUserSignIn_WhenSatOrganisation_AndUidNotParseable_DoesNotQueryGias_AndThrows()
    {
        // Arrange: Uid isn't a valid int, so int.TryParse fails and the GIAS lookup is skipped entirely -
        // the school remains null and the method should still throw.
        var org = CreateEstablishment(
            uid: "not-a-number",
            categoryId: DsiConstants.SatOrganisationCategoryId
        );
        var principal = AuthenticatedPrincipal(
            new Claim(ClaimConstants.NameIdentifier, "dsi-ref-bad-uid"),
            OrganisationClaim(org)
        );

        var (ctx, signIn, gias, _, logger) = BuildContext(principal);

        var signInDto = new SqlSignInDto
        {
            Id = 1,
            UserId = 1,
            EstablishmentId = 1,
        };

        signIn.RecordSignIn(Arg.Any<string>(), Arg.Any<EstablishmentModel>()).Returns(signInDto);

        // Act / Assert
        var ex = await Assert.ThrowsAsync<InvalidGiasDataException>(() =>
            OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx)
        );

        Assert.Contains("SAT", ex.Message);
        await gias.DidNotReceiveWithAnyArgs().GetSingleAcademySchool(default);
    }

    [Fact]
    public async Task RecordUserSignIn_WhenSatOrganisation_AndDsiProviderReturnsNull_DoesNotWriteAdditionalClaim()
    {
        // Arrange: GIAS finds the school, but the DSI API has no matching organisation for the user -
        // the code should fall back to the original SAT organisation rather than fail.
        var originalOrg = CreateEstablishment(
            urn: "888888",
            uid: "99",
            categoryId: DsiConstants.SatOrganisationCategoryId
        );
        var principal = AuthenticatedPrincipal(
            new Claim(ClaimConstants.NameIdentifier, "dsi-ref-fallback"),
            OrganisationClaim(originalOrg)
        );

        var (ctx, signIn, gias, dsiProvider, logger) = BuildContext(principal);

        gias.GetSingleAcademySchool(99).Returns(CreateSchool(888888));
        dsiProvider
            .GetOrganisationForUserAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns((EstablishmentModel?)null);

        var signInDto = new SqlSignInDto { UserId = 5, EstablishmentId = 6 };
        signIn.RecordSignIn(Arg.Any<string>(), Arg.Any<EstablishmentModel>()).Returns(signInDto);

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert: RecordSignIn is called with the original org, and the single-academy claim
        // still gets added (it's set whenever the SAT branch runs, even on fallback).
        await signIn
            .Received(1)
            .RecordSignIn("dsi-ref-fallback", Arg.Is<EstablishmentModel>(e => e.Urn == "888888"));

        var satClaim = ctx.Principal!.Claims.FirstOrDefault(c =>
            c.Type == ClaimConstants.SINGLE_ACADEMY_ORGANISATION
        );
        Assert.Null(satClaim);
    }

    // The rest of the behavior (adding DB_USER_ID and DB_ESTABLISHMENT_ID) is in a private method.
    // We verify it directly via reflection to avoid depending on the claim-parsing extension methods.
    [Fact]
    public void AddClaimsToPrincipal_Adds_DbUserId_And_DbEstablishmentId()
    {
        // Arrange
        var principal = AuthenticatedPrincipal();
        var (ctx, _, _, _, _) = BuildContext(principal);
        var signIn = new SqlSignInDto { UserId = 42, EstablishmentId = 999 };

        // Invoke private static AddClaimsToPrincipal(UserInformationReceivedContext, SqlSignInDto)
        var mi = typeof(OnUserInformationReceivedEvent).GetMethod(
            "AddClaimsToPrincipal",
            BindingFlags.NonPublic | BindingFlags.Static
        );
        Assert.NotNull(mi);

        mi!.Invoke(null, new object[] { ctx, signIn });

        // Assert
        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimConstants.DB_USER_ID);
        var estIdClaim = principal.Claims.FirstOrDefault(c =>
            c.Type == ClaimConstants.DB_ESTABLISHMENT_ID
        );

        Assert.NotNull(userIdClaim);
        Assert.Equal("42", userIdClaim!.Value);

        Assert.NotNull(estIdClaim);
        Assert.Equal("999", estIdClaim!.Value);
    }

    [Fact]
    public void AddClaimsToPrincipal_When_PrincipalNull_DoesNothing()
    {
        var (ctx, _, _, _, _) = BuildContext(principal: null!);
        var signIn = new SqlSignInDto { UserId = 1, EstablishmentId = 2 };

        var mi = typeof(OnUserInformationReceivedEvent).GetMethod(
            "AddClaimsToPrincipal",
            BindingFlags.NonPublic | BindingFlags.Static
        );
        Assert.NotNull(mi);

        // Should not throw
        mi!.Invoke(null, [ctx, signIn]);
    }

    [Fact]
    public void AddClaimsToPrincipal_When_EstablishmentIdNull_Throws()
    {
        var principal = AuthenticatedPrincipal();
        var (ctx, _, _, _, _) = BuildContext(principal);
        var signIn = new SqlSignInDto { UserId = 7, EstablishmentId = null };

        var mi = typeof(OnUserInformationReceivedEvent).GetMethod(
            "AddClaimsToPrincipal",
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

        var ex = Assert.Throws<TargetInvocationException>(() =>
            mi.Invoke(null, new object[] { ctx, signIn })
        );

        // Inner exception should be the InvalidDataException thrown by the method
        Assert.IsType<InvalidDataException>(ex.InnerException);
        Assert.Equal("EstablishmentId", ex.InnerException!.Message);
    }
}
