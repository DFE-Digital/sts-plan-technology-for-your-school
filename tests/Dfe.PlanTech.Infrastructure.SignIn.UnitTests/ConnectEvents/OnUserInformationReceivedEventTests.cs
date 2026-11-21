using System.Security.Claims;
using System.Text.Json;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.SignIn.ConnectEvents;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Tests.Shared.Fakes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Infrastructure.SignIn.UnitTests.ConnectEvents;

public class OnUserInformationReceivedEventTests
{
    private const string DsiReference = "dsi-user-42";

    private readonly FakeLogger<IDfeSignIn> _logger = new();
    private readonly ISignInService _signInService = Substitute.For<ISignInService>();
    private readonly IEstablishmentService _establishmentService =
        Substitute.For<IEstablishmentService>();
    private readonly IGiasService _giasService = Substitute.For<IGiasService>();
    private readonly ICurrentUserProvider _currentUserProvider =
        Substitute.For<ICurrentUserProvider>();

    private UserInformationReceivedContext CreateContext(ClaimsPrincipal principal)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_signInService);
        services.AddSingleton(_establishmentService);
        services.AddSingleton(_giasService);
        services.AddSingleton(_currentUserProvider);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
        };
        var scheme = new AuthenticationScheme(
            OpenIdConnectDefaults.AuthenticationScheme,
            null,
            typeof(OpenIdConnectHandler)
        );

        return new UserInformationReceivedContext(
            httpContext,
            scheme,
            new OpenIdConnectOptions(),
            principal,
            new AuthenticationProperties()
        );
    }

    private static ClaimsPrincipal CreatePrincipal(EstablishmentModel? organisation = null)
    {
        var builder = new ClaimsPrincipalBuilder().WithClaim(
            ClaimConstants.NameIdentifier,
            DsiReference
        );

        if (organisation is not null)
        {
            builder.WithClaim(ClaimConstants.Organisation, JsonSerializer.Serialize(organisation));
        }

        return builder.Build();
    }

    [Fact]
    public async Task RecordUserSignIn_WhenUserIsNotAuthenticated_FailsTheContext()
    {
        // Arrange
        var context = CreateContext(new ClaimsPrincipalBuilder().Unauthenticated().Build());

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(_logger, context);

        // Assert
        Assert.NotNull(context.Result);
        Assert.Equal("User is not authenticated.", context.Result.Failure!.Message);
        await _signInService.DidNotReceiveWithAnyArgs().RecordSignInAsync(default!, default!);
        await _signInService.DidNotReceiveWithAnyArgs().RecordSignInUserOnlyAsync(default!);
    }

    [Fact]
    public async Task RecordUserSignIn_WhenNameIdentifierClaimIsMissing_LogsErrorAndFailsTheContext()
    {
        // Arrange
        var context = CreateContext(new ClaimsPrincipalBuilder().Build());

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(_logger, context);

        // Assert
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Contains("no nameidentifier claim", logRecord.Message);
        Assert.Equal(
            "No nameidentifier claim present in user principal.",
            context.Result!.Failure!.Message
        );
    }

    [Fact]
    public async Task RecordUserSignIn_WhenUserHasNoOrganisation_LogsWarningAndRecordsUserOnly()
    {
        // Arrange
        var context = CreateContext(CreatePrincipal());

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(_logger, context);

        // Assert
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Warning, logRecord.Level);
        Assert.Contains(DsiReference, logRecord.Message);
        await _signInService.Received(1).RecordSignInUserOnlyAsync(DsiReference);
        await _signInService.DidNotReceiveWithAnyArgs().RecordSignInAsync(default!, default!);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task RecordUserSignIn_WhenOrganisationIsASchool_AddsUserAndFocusedEstablishmentClaims()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .AsSchool()
            .WithUrn("100701")
            .WithName("Northfield Primary")
            .Build();
        var context = CreateContext(CreatePrincipal(organisation));
        _signInService
            .RecordSignInAsync(DsiReference, Arg.Is<EstablishmentModel>(o => o.Urn == "100701"))
            .Returns(new SqlSignInDtoBuilder().WithUserId(42).WithEstablishmentId(301).Build());
        _establishmentService
            .GetEstablishmentByIdAsync(301)
            .Returns(new SqlEstablishmentDtoBuilder().WithId(301).Build());

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(_logger, context);

        // Assert
        var claims = context.Principal!.Claims.ToList();
        Assert.Contains(claims, c => c.Type == ClaimConstants.DB_USER_ID && c.Value == "42");
        Assert.Contains(claims, c =>
            c.Type == ClaimConstants.DB_ESTABLISHMENT_ID && c.Value == "301"
        );
        Assert.Contains(claims, c =>
            c.Type == ClaimConstants.DB_FOCUSED_ESTABLISHMENT_ID && c.Value == "301"
        );
        Assert.DoesNotContain(claims, c => c.Type == ClaimConstants.DB_TRUST_ESTABLISHMENT_ID);
        var sessionIdClaim = Assert.Single(claims, c => c.Type == ClaimConstants.SessionId);
        Assert.True(Guid.TryParse(sessionIdClaim.Value, out _));
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task RecordUserSignIn_WhenOrganisationIsPresent_SavesUserEstablishmentToSession()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder().AsSchool().WithUrn("100701").Build();
        var context = CreateContext(CreatePrincipal(organisation));
        var userEstablishment = new SqlEstablishmentDtoBuilder().WithId(301).Build();
        _signInService
            .RecordSignInAsync(DsiReference, Arg.Any<EstablishmentModel>())
            .Returns(new SqlSignInDtoBuilder().WithEstablishmentId(301).Build());
        _establishmentService.GetEstablishmentByIdAsync(301).Returns(userEstablishment);

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(_logger, context);

        // Assert
        _currentUserProvider.Received(1).SetUserEstablishment(userEstablishment);
    }

    [Fact]
    public async Task RecordUserSignIn_WhenSignInEstablishmentDiffersFromDatabaseRecord_UsesDatabaseEstablishmentId()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder().AsSchool().WithUrn("100701").Build();
        var context = CreateContext(CreatePrincipal(organisation));
        _signInService
            .RecordSignInAsync(DsiReference, Arg.Any<EstablishmentModel>())
            .Returns(new SqlSignInDtoBuilder().WithEstablishmentId(2).Build());
        _establishmentService
            .GetEstablishmentByIdAsync(2)
            .Returns(new SqlEstablishmentDtoBuilder().WithId(301).Build());

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(_logger, context);

        // Assert
        Assert.Contains(context.Principal!.Claims, c =>
            c.Type == ClaimConstants.DB_ESTABLISHMENT_ID && c.Value == "301"
        );
    }

    [Fact]
    public async Task RecordUserSignIn_WhenSchoolOrganisationHasNoUrn_LogsWarning()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .AsSchool()
            .WithUrn(null)
            .WithName("Northfield Primary")
            .Build();
        var context = CreateContext(CreatePrincipal(organisation));
        _signInService
            .RecordSignInAsync(DsiReference, Arg.Any<EstablishmentModel>())
            .Returns(new SqlSignInDtoBuilder().WithEstablishmentId(301).Build());
        _establishmentService
            .GetEstablishmentByIdAsync(301)
            .Returns(new SqlEstablishmentDtoBuilder().WithId(301).Build());

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(_logger, context);

        // Assert
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Warning, logRecord.Level);
        Assert.Contains("No URN found", logRecord.Message);
        Assert.Contains("Northfield Primary", logRecord.Message);
    }

    [Fact]
    public async Task RecordUserSignIn_WhenSatSchoolIsFound_FocusesSchoolAndRecordsTrust()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .AsSingleAcademyTrust()
            .WithUid("42")
            .Build();
        var context = CreateContext(CreatePrincipal(organisation));
        _signInService
            .RecordSignInAsync(DsiReference, Arg.Any<EstablishmentModel>())
            .Returns(new SqlSignInDtoBuilder().WithEstablishmentId(301).Build());
        _establishmentService
            .GetEstablishmentByIdAsync(301)
            .Returns(new SqlEstablishmentDtoBuilder().WithId(301).Build());
        _giasService
            .GetSchoolForSatAsync(Arg.Is<EstablishmentModel>(o => o.Uid == "42"))
            .Returns(
                new SelectedEstablishmentDataModel
                {
                    EstablishmentId = 701,
                    Urn = "100701",
                    Name = "Northfield Primary",
                }
            );

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(_logger, context);

        // Assert
        var claims = context.Principal!.Claims.ToList();
        Assert.Contains(claims, c =>
            c.Type == ClaimConstants.DB_FOCUSED_ESTABLISHMENT_ID && c.Value == "701"
        );
        Assert.Contains(claims, c =>
            c.Type == ClaimConstants.DB_TRUST_ESTABLISHMENT_ID && c.Value == "301"
        );
        Assert.Empty(_logger.Collector.GetSnapshot());
    }

    [Theory]
    [InlineData(DsiConstants.SatOrganisationCategoryId, "for single-academy login")]
    [InlineData(DsiConstants.SSatOrganisationCategoryId, "for secure single-academy login")]
    public async Task RecordUserSignIn_WhenSatSchoolIsNotFound_LogsWarningAndFocusesTrust(
        string categoryId,
        string expectedLogFragment
    )
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .WithCategory(categoryId, "Trust")
            .WithGroupUid("group-42")
            .Build();
        var context = CreateContext(CreatePrincipal(organisation));
        _signInService
            .RecordSignInAsync(DsiReference, Arg.Any<EstablishmentModel>())
            .Returns(new SqlSignInDtoBuilder().WithEstablishmentId(301).Build());
        _establishmentService
            .GetEstablishmentByIdAsync(301)
            .Returns(new SqlEstablishmentDtoBuilder().WithId(301).Build());
        _giasService
            .GetSchoolForSatAsync(Arg.Any<EstablishmentModel>())
            .Returns((SelectedEstablishmentDataModel?)null);

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(_logger, context);

        // Assert
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Warning, logRecord.Level);
        Assert.Contains(expectedLogFragment, logRecord.Message);
        Assert.Contains("group-42", logRecord.Message);
        Assert.Contains(context.Principal!.Claims, c =>
            c.Type == ClaimConstants.DB_FOCUSED_ESTABLISHMENT_ID && c.Value == "301"
        );
        Assert.Contains(context.Principal.Claims, c =>
            c.Type == ClaimConstants.DB_TRUST_ESTABLISHMENT_ID && c.Value == "301"
        );
    }

    [Fact]
    public async Task RecordUserSignIn_WhenGiasCannotResolveSatSchool_ThrowsInvalidGiasDataException()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder().AsSecureSingleAcademyTrust().Build();
        var context = CreateContext(CreatePrincipal(organisation));
        _signInService
            .RecordSignInAsync(DsiReference, Arg.Any<EstablishmentModel>())
            .Returns(new SqlSignInDtoBuilder().WithEstablishmentId(301).Build());
        _establishmentService
            .GetEstablishmentByIdAsync(301)
            .Returns(new SqlEstablishmentDtoBuilder().WithId(301).Build());
        _giasService
            .GetSchoolForSatAsync(Arg.Any<EstablishmentModel>())
            .ThrowsAsync(new InvalidGiasDataException("School not found for SSAT"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidGiasDataException>(() =>
            OnUserInformationReceivedEvent.RecordUserSignIn(_logger, context)
        );
        Assert.Equal("School not found for SSAT", exception.Message);
    }

    [Fact]
    public async Task RecordUserSignIn_WhenOrganisationIsAMat_AddsTrustClaimWithoutFocusedEstablishment()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder().AsMultiAcademyTrust().Build();
        var context = CreateContext(CreatePrincipal(organisation));
        _signInService
            .RecordSignInAsync(DsiReference, Arg.Any<EstablishmentModel>())
            .Returns(new SqlSignInDtoBuilder().WithEstablishmentId(301).Build());
        _establishmentService
            .GetEstablishmentByIdAsync(301)
            .Returns(new SqlEstablishmentDtoBuilder().WithId(301).Build());

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(_logger, context);

        // Assert
        var claims = context.Principal!.Claims.ToList();
        Assert.Contains(claims, c =>
            c.Type == ClaimConstants.DB_TRUST_ESTABLISHMENT_ID && c.Value == "301"
        );
        Assert.DoesNotContain(claims, c => c.Type == ClaimConstants.DB_FOCUSED_ESTABLISHMENT_ID);
        await _giasService.DidNotReceive().GetSchoolForSatAsync(Arg.Any<EstablishmentModel>());
    }

    [Fact]
    public async Task RecordUserSignIn_WhenMatHasNoGroupUid_LogsWarning()
    {
        // Arrange
        var organisation = new EstablishmentModelBuilder()
            .AsMultiAcademyTrust()
            .WithName("Northfield Academy Trust")
            .WithGroupUid(null)
            .Build();
        var context = CreateContext(CreatePrincipal(organisation));
        _signInService
            .RecordSignInAsync(DsiReference, Arg.Any<EstablishmentModel>())
            .Returns(new SqlSignInDtoBuilder().WithEstablishmentId(301).Build());
        _establishmentService
            .GetEstablishmentByIdAsync(301)
            .Returns(new SqlEstablishmentDtoBuilder().WithId(301).Build());

        // Act
        await OnUserInformationReceivedEvent.RecordUserSignIn(_logger, context);

        // Assert
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Warning, logRecord.Level);
        Assert.Contains("No group UID found", logRecord.Message);
        Assert.Contains("Northfield Academy Trust", logRecord.Message);
    }
}
