using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Infrastructure.SignIn.ConnectEvents;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Tests.Shared.Extensions;
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
        ISignInService signIn,
        IGiasService gias,
        ICurrentUserProvider user,
        ILogger<IDfeSignIn> logger
    ) BuildContext(ClaimsPrincipal principal)
    {
        var services = new ServiceCollection();
        var signIn = Substitute.For<ISignInService>();
        var gias = Substitute.For<IGiasService>();
        var user = Substitute.For<ICurrentUserProvider>();
        var establishmentService = Substitute.For<IEstablishmentService>();
        establishmentService
            .GetEstablishmentByIdAsync(Arg.Any<int>())
            .Returns(new SqlEstablishmentDtoBuilder().WithId(1).Build());
        services.AddSingleton(signIn);
        services.AddSingleton(gias);
        services.AddSingleton(user);
        services.AddSingleton(establishmentService);

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
        return (ctx, signIn, gias, user, logger);
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
        await signIn.DidNotReceiveWithAnyArgs().RecordSignInAsync(default!, default!);
        await signIn.DidNotReceiveWithAnyArgs().RecordSignInUserOnlyAsync(default!);
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
        var (ctx, signIn, _, _, logger) = BuildContext(principal);

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert: since GetOrganisation() returns null with no org claims, we should call the user-only workflow
        await signIn.Received(1).RecordSignInUserOnlyAsync(Arg.Any<string>());
        await signIn.DidNotReceiveWithAnyArgs().RecordSignInAsync(default!, default!);

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

        var (ctx, signIn, gias, _, logger) = BuildContext(principal);

        var signInDto = new SqlSignInDto { UserId = 1, EstablishmentId = 2 };
        signIn
            .RecordSignInAsync(Arg.Any<string>(), Arg.Any<EstablishmentModel>())
            .Returns(signInDto);

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert: normal establishments never go through the SAT replacement lookup
        await signIn
            .Received(1)
            .RecordSignInAsync("dsi-ref-123", Arg.Is<EstablishmentModel>(e => e.Urn == "111111"));
        await signIn.DidNotReceiveWithAnyArgs().RecordSignInUserOnlyAsync(default!);
        await gias.DidNotReceiveWithAnyArgs().GetSchoolForSatAsync(default!);

        Assert.True(ctx.Result is null);

        var userIdClaim = ctx.Principal!.Claims.FirstOrDefault(c =>
            c.Type == ClaimConstants.DB_USER_ID
        );
        Assert.NotNull(userIdClaim);
        Assert.Equal("1", userIdClaim!.Value);
    }

    [Fact]
    public async Task RecordUserSignIn_WhenSsatSchoolIsMissing_UsesTrustAsFocusedEstablishment()
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

        var (ctx, signIn, gias, user, logger) = BuildContext(principal);

        var signInDto = new SqlSignInDto
        {
            Id = 1,
            UserId = 1,
            EstablishmentId = 1,
        };
        signIn
            .RecordSignInAsync(Arg.Any<string>(), Arg.Any<EstablishmentModel>())
            .Returns(signInDto);

        gias.GetSchoolForSatAsync(Arg.Any<EstablishmentModel>())
            .Returns(default(SelectedEstablishmentDataModel?));

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert
        Assert.Contains(ctx.Principal!.Claims, claim =>
            claim.Type == ClaimConstants.DB_FOCUSED_ESTABLISHMENT_ID && claim.Value == "1"
        );
        Assert.Contains(ctx.Principal.Claims, claim =>
            claim.Type == ClaimConstants.DB_TRUST_ESTABLISHMENT_ID && claim.Value == "1"
        );
    }

    [Fact]
    public async Task RecordUserSignIn_WhenSatUidIsNotNumeric_UsesTrustFallbackClaims()
    {
        // Arrange
        var org = CreateEstablishment(
            uid: "not-a-number",
            categoryId: DsiConstants.SatOrganisationCategoryId
        );
        var principal = AuthenticatedPrincipal(
            new Claim(ClaimConstants.NameIdentifier, "dsi-ref-bad-uid"),
            OrganisationClaim(org)
        );

        var (ctx, signIn, gias, user, logger) = BuildContext(principal);

        var signInDto = new SqlSignInDto
        {
            Id = 1,
            UserId = 1,
            EstablishmentId = 1,
        };

        signIn
            .RecordSignInAsync(Arg.Any<string>(), Arg.Any<EstablishmentModel>())
            .Returns(signInDto);

        gias.GetSchoolForSatAsync(Arg.Any<EstablishmentModel>())
            .Returns(default(SelectedEstablishmentDataModel?));

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert
        await gias.Received(1)
            .GetSchoolForSatAsync(
                Arg.Is<EstablishmentModel>(organisation => organisation.Uid == "not-a-number")
            );
        Assert.Contains(ctx.Principal!.Claims, claim =>
            claim.Type == ClaimConstants.DB_FOCUSED_ESTABLISHMENT_ID && claim.Value == "1"
        );
        Assert.Contains(ctx.Principal.Claims, claim =>
            claim.Type == ClaimConstants.DB_TRUST_ESTABLISHMENT_ID && claim.Value == "1"
        );
    }

    [Fact]
    public async Task RecordUserSignIn_WhenSatOrganisation_AndDsiProviderReturnsNull_DoesNotWriteAdditionalClaim()
    {
        // Arrange: GIAS finds the school, but the DSI API has no matching organisation for the user -
        // the code should fall back to the original SAT organisation rather than fail.
        var urn = "888888";
        var uid = "99";

        var originalOrg = CreateEstablishment(
            urn: urn,
            uid: uid,
            categoryId: DsiConstants.SatOrganisationCategoryId
        );
        var principal = AuthenticatedPrincipal(
            new Claim(ClaimConstants.NameIdentifier, "dsi-ref-fallback"),
            OrganisationClaim(originalOrg)
        );

        var (ctx, signIn, gias, user, logger) = BuildContext(principal);

        var selectedEstablishmentDataModel = new SelectedEstablishmentDataModel
        {
            EstablishmentId = 1,
            Name = "Test SAT school",
            Urn = urn,
        };

        gias.GetSchoolForSatAsync(Arg.Any<EstablishmentModel>())
            .Returns(selectedEstablishmentDataModel);

        var signInDto = new SqlSignInDto { UserId = 5, EstablishmentId = 6 };
        signIn
            .RecordSignInAsync(Arg.Any<string>(), Arg.Any<EstablishmentModel>())
            .Returns(signInDto);

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert: RecordSignIn is called with the original org, and the single-academy claim
        // still gets added (it's set whenever the SAT branch runs, even on fallback).
        await signIn
            .Received(1)
            .RecordSignInAsync(
                "dsi-ref-fallback",
                Arg.Is<EstablishmentModel>(e => e.Urn == "888888")
            );
    }
}
