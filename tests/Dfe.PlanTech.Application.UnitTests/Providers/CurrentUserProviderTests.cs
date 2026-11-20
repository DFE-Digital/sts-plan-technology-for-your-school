using System.Text.Json;
using Dfe.PlanTech.Application.Providers;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Assertions;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Tests.Shared.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Providers;

public class CurrentUserProviderTests
{
    private readonly FakeLogger<CurrentUserProvider> _logger = new();
    private readonly HttpContextAccessor _contextAccessor = new();
    private readonly IEstablishmentService _establishmentService =
        Substitute.For<IEstablishmentService>();
    private readonly IGiasService _giasService = Substitute.For<IGiasService>();

    private CurrentUserProvider CreateSystemUnderTest(
        EstablishmentModel? organisation = null,
        FakeSession? session = null
    )
    {
        var contextBuilder = new HttpContextBuilder()
            .WithClaim(ClaimConstants.DB_ESTABLISHMENT_ID, "301")
            .WithClaim(ClaimConstants.DB_USER_ID, "42")
            .WithSession(session ?? new FakeSession());

        if (organisation is not null)
        {
            contextBuilder.WithClaim(
                ClaimConstants.Organisation,
                JsonSerializer.Serialize(organisation)
            );
        }

        _contextAccessor.HttpContext = contextBuilder.Build();

        return new CurrentUserProvider(
            _logger,
            _contextAccessor,
            _establishmentService,
            _giasService
        );
    }

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        ConstructorGuardAssert.AllNonNullableParametersAreGuarded<CurrentUserProvider>(
            _logger,
            _contextAccessor,
            _establishmentService,
            _giasService
        );
    }

    [Fact]
    public async Task SetGroupSchoolsAsync_WhenGroupSchoolsAreNotInSession_SavesSchools()
    {
        // Arrange
        var expectedSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(701)
            .WithUrn("100701")
            .WithName("Northfield Primary")
            .Build();
        var session = new FakeSession();
        _establishmentService
            .GetEstablishmentsByReferencesAsync(Arg.Any<IEnumerable<string>>())
            .Returns([
                new SqlEstablishmentDtoBuilder().WithId(701).WithEstablishmentRef("100701").Build(),
            ]);
        _establishmentService
            .GetOrCreateSchoolsAsync(Arg.Any<IEnumerable<SelectedEstablishmentDataModel>>())
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest(session: session);

        // Act
        await systemUnderTest.SetGroupSchoolsAsync([expectedSchool]);

        // Assert
        var result = session.GetGroupSchools();
        var school = Assert.Single(result);
        Assert.Equal(expectedSchool.EstablishmentId, school.EstablishmentId);
        Assert.Equal(expectedSchool.Urn, school.Urn);
        Assert.Equal(expectedSchool.Name, school.Name);
        Assert.Empty(_logger.Collector.GetSnapshot());
        await _establishmentService.DidNotReceive().GetEstablishmentByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task SetSelectedSchoolsAsync_WhenSelectionIsOutsideTheGroup_ClearsAndThrows()
    {
        // Arrange
        var session = new FakeSession();
        session.SetUserEstablishment(new SqlEstablishmentDtoBuilder().Build());
        session.SetGroupSchools([
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(701)
                .WithUrn("100701")
                .WithName("Northfield Primary")
                .Build(),
        ]);
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder()
                .WithCategory(DsiConstants.MatOrganisationCategoryId, "Multi-academy trust")
                .Build(),
            session
        );
        var invalidSelection = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(702)
            .WithUrn("100702")
            .WithName("Southfield Primary")
            .Build();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            systemUnderTest.SetSelectedSchoolsAsync(invalidSelection)
        );

        // Assert
        Assert.Equal(
            "One or more selected establishments are not in the user's group.",
            exception.Message
        );
        Assert.Empty(session.GetSelectedSchools());
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Warning, logRecord.Level);
        Assert.Contains("not within their group", logRecord.Message);
    }

    [Fact]
    public async Task SetSelectedSchoolsAsync_WhenSelectionIsEmpty_ClearsAndReturns()
    {
        // Arrange
        var session = new FakeSession();
        session.SetSelectedEstablishments(701, "100701", "Northfield Primary");
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder()
                .WithCategory(DsiConstants.MatOrganisationCategoryId, "Multi-academy trust")
                .Build(),
            session
        );

        // Act
        await systemUnderTest.SetSelectedSchoolsAsync([]);

        // Assert
        Assert.Empty(session.GetSelectedSchools());
        await _establishmentService.DidNotReceive().GetEstablishmentByIdAsync(Arg.Any<int>());
        await _giasService.DidNotReceive().GetSchoolsForMatAsync(Arg.Any<EstablishmentModel>());
    }

    [Fact]
    public void ClearFocusedEstablishment_WhenUserCanSelectSchools_RemovesFocusedSchool()
    {
        // Arrange
        var session = new FakeSession();
        session.SetFocusedEstablishment(701, "100701", "Northfield Primary");
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder()
                .WithCategory(DsiConstants.MatOrganisationCategoryId, "Multi-academy trust")
                .Build(),
            session
        );

        // Act
        systemUnderTest.ClearFocusedEstablishment();

        // Assert
        Assert.Null(session.GetFocusedEstablishment());
    }

    [Fact]
    public async Task SetSelectedSchoolsAsync_WhenGroupIsNotInSession_LoadsItFromGias()
    {
        // Arrange
        var firstGroupSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(701)
            .WithUrn("100701")
            .WithName("Northfield Primary")
            .Build();
        var selectedSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(702)
            .WithUrn("100702")
            .WithName("Southfield Primary")
            .Build();
        var groupSchools = new List<SelectedEstablishmentDataModel>
        {
            firstGroupSchool,
            selectedSchool,
        };
        var session = new FakeSession();
        session.SetUserEstablishment(new SqlEstablishmentDtoBuilder().Build());
        _giasService.GetSchoolsForMatAsync(Arg.Any<EstablishmentModel>()).Returns(groupSchools);
        _establishmentService
            .GetEstablishmentsByReferencesAsync(Arg.Any<IEnumerable<string>>())
            .Returns([
                new SqlEstablishmentDtoBuilder().WithId(701).WithEstablishmentRef("100701").Build(),
                new SqlEstablishmentDtoBuilder().WithId(702).WithEstablishmentRef("100702").Build(),
            ]);
        _establishmentService
            .GetOrCreateSchoolsAsync(Arg.Any<IEnumerable<SelectedEstablishmentDataModel>>())
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest(
            new EstablishmentModelBuilder()
                .WithCategory(DsiConstants.MatOrganisationCategoryId, "Multi-academy trust")
                .Build(),
            session
        );

        // Act
        await systemUnderTest.SetSelectedSchoolsAsync(selectedSchool);

        // Assert
        Assert.Equal(
            ["100701", "100702"],
            session.GetGroupSchools().Select(school => school.Urn).ToArray()
        );
        Assert.Equal(
            ["100702"],
            session.GetSelectedSchools().Select(school => school.Urn).ToArray()
        );
        await _giasService.Received(1).GetSchoolsForMatAsync(Arg.Any<EstablishmentModel>());
    }
}
