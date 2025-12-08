using System.Reflection;
using System.Security.Claims;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.SignIn.ConnectEvents;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.SignIn.UnitTests.ConnectEvents;

public class OnUserInformationReceivedEventTests
{
    private static (UserInformationReceivedContext ctx, ISignInWorkflow wf, ILogger<DfeSignIn> logger) BuildContext(ClaimsPrincipal principal)
    {
        var services = new ServiceCollection();
        var wf = Substitute.For<ISignInWorkflow>();
        services.AddSingleton(wf);

        var sp = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = sp };
        var scheme = new AuthenticationScheme(OpenIdConnectDefaults.AuthenticationScheme, null, typeof(OpenIdConnectHandler));
        var options = new OpenIdConnectOptions();
        var properties = new AuthenticationProperties();

        var ctx = new UserInformationReceivedContext(httpContext, scheme, options, principal, properties);

        var logger = Substitute.For<ILogger<DfeSignIn>>();
        return (ctx, wf, logger);
    }

    private static ClaimsPrincipal AuthenticatedPrincipal(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, authenticationType: "cookies");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task RecordUserSignIn_When_NotAuthenticated_FailsContext()
    {
        // Arrange: no principal OR unauthenticated identity
        var (ctx, wf, logger) = BuildContext(principal: new ClaimsPrincipal(new ClaimsIdentity())); // IsAuthenticated = false

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert
        Assert.NotNull(ctx.Result);
        Assert.False(ctx.Result!.Succeeded);
        Assert.NotNull(ctx.Result.Failure);
        await wf.DidNotReceiveWithAnyArgs().RecordSignIn(default!, default!);
        await wf.DidNotReceiveWithAnyArgs().RecordSignInUserOnly(default!);
    }

    [Fact]
    public async Task RecordUserSignIn_When_EstablishmentMissing_LogsWarning_And_RecordsUserOnly()
    {
        // Arrange: authenticated principal but no establishment-related claims
        var principal = AuthenticatedPrincipal(new Claim(ClaimConstants.NameIdentifier, "dsi-ref-123"));
        var (ctx, wf, logger) = BuildContext(principal);

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, ctx);

        // Assert: since GetOrganisation() returns null with no org claims, we should call the user-only workflow
        await wf.Received(1).RecordSignInUserOnly(Arg.Any<string>());
        await wf.DidNotReceiveWithAnyArgs().RecordSignIn(default!, default!);

        // Context not failed
        Assert.True(ctx.Result is null);
    }

    // The rest of the behavior (adding DB_USER_ID and DB_ESTABLISHMENT_ID) is in a private method.
    // We verify it directly via reflection to avoid depending on the claim-parsing extension methods.
    [Fact]
    public void AddClaimsToPrincipal_Adds_DbUserId_And_DbEstablishmentId()
    {
        // Arrange
        var principal = AuthenticatedPrincipal();
        var (ctx, _, _) = BuildContext(principal);
        var signIn = new SqlSignInDto
        {
            UserId = 42,
            EstablishmentId = 999
        };

        // Invoke private static AddClaimsToPrincipal(UserInformationReceivedContext, SqlSignInDto)
        var mi = typeof(OnUserInformationReceivedEvent)
            .GetMethod("AddClaimsToPrincipal", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(mi);

        mi!.Invoke(null, new object[] { ctx, signIn });

        // Assert
        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimConstants.DB_USER_ID);
        var estIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimConstants.DB_ESTABLISHMENT_ID);

        Assert.NotNull(userIdClaim);
        Assert.Equal("42", userIdClaim!.Value);

        Assert.NotNull(estIdClaim);
        Assert.Equal("999", estIdClaim!.Value);
    }

    [Fact]
    public void AddClaimsToPrincipal_When_PrincipalNull_DoesNothing()
    {
        var (ctx, _, _) = BuildContext(principal: null!);
        var signIn = new SqlSignInDto { UserId = 1, EstablishmentId = 2 };

        var mi = typeof(OnUserInformationReceivedEvent)
            .GetMethod("AddClaimsToPrincipal", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(mi);

        // Should not throw
        mi!.Invoke(null, [ctx, signIn]);
    }

    [Fact]
    public void AddClaimsToPrincipal_When_EstablishmentIdNull_Throws()
    {
        var principal = AuthenticatedPrincipal();
        var (ctx, _, _) = BuildContext(principal);
        var signIn = new SqlSignInDto { UserId = 7, EstablishmentId = null };

        var mi = typeof(OnUserInformationReceivedEvent)
            .GetMethod("AddClaimsToPrincipal", BindingFlags.NonPublic | BindingFlags.Static)!;

        var ex = Assert.Throws<TargetInvocationException>(() =>
            mi.Invoke(null, new object[] { ctx, signIn }));

        // Inner exception should be the InvalidDataException thrown by the method
        Assert.IsType<InvalidDataException>(ex.InnerException);
        Assert.Equal("EstablishmentId", ex.InnerException!.Message);
    }
}
