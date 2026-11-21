using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class SelfAssessmentSummaryViewBuilderTests
{
    private const int FocusedEstablishmentId = 123;
    private const int FirstSelectedEstablishmentId = 101;
    private const int SecondSelectedEstablishmentId = 102;
    private const string SectionId = "section-1";
    private const string SectionSlug = "cyber-security";
    private const string CategorySlug = "digital-leadership";

    private static readonly QuestionnaireSectionEntry Section = new()
    {
        Sys = new SystemDetails(SectionId),
        Name = "Cyber security",
    };

    private readonly FakeLogger<SelfAssessmentSummaryViewBuilder> _logger = new();
    private readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();
    private readonly ICurrentUserProvider _currentUserProvider =
        Substitute.For<ICurrentUserProvider>();
    private readonly IGroupService _groupService = Substitute.For<IGroupService>();
    private readonly IHttpContextAccessor _httpContextAccessor =
        Substitute.For<IHttpContextAccessor>();

    private readonly TestController _controller = new();

    private SelfAssessmentSummaryViewBuilder CreateSystemUnderTest() =>
        new(_logger, _contentfulService, _currentUserProvider, _groupService, _httpContextAccessor);

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        // The logger is not null-guarded in production, so it is excluded here.
        Assert.Throws<ArgumentNullException>("contentfulService", () =>
            new SelfAssessmentSummaryViewBuilder(
                _logger,
                null!,
                _currentUserProvider,
                _groupService,
                _httpContextAccessor
            )
        );
        Assert.Throws<ArgumentNullException>("currentUser", () =>
            new SelfAssessmentSummaryViewBuilder(
                _logger,
                _contentfulService,
                null!,
                _groupService,
                _httpContextAccessor
            )
        );
        Assert.Throws<ArgumentNullException>("groupService", () =>
            new SelfAssessmentSummaryViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                null!,
                _httpContextAccessor
            )
        );
        Assert.Throws<ArgumentNullException>("httpContextAccessor", () =>
            new SelfAssessmentSummaryViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                _groupService,
                null!
            )
        );
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenSectionIsMissing_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync("missing-section")
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSelfAssessmentSummary(
                _controller,
                CategorySlug,
                "missing-section"
            )
        );
        Assert.Contains("missing-section", exception.Message);
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenSectionHasNoId_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync("section-without-id")
            .Returns(new QuestionnaireSectionEntry { Name = "Cyber security" });
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToSelfAssessmentSummary(
                _controller,
                CategorySlug,
                "section-without-id"
            )
        );
        Assert.Contains("section-without-id", exception.Message);
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenSchoolUserHasCompletedSection_ReturnsSchoolSummaryView()
    {
        // Arrange
        _currentUserProvider.CanSelectSchools.Returns(false);
        _currentUserProvider.IsMat.Returns(false);
        _currentUserProvider
            .GetFocusedEstablishmentAsync()
            .Returns(
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FocusedEstablishmentId)
                    .Build()
            );
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(Section);
        _contentfulService.GetAllCategoriesAsync().Returns([]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(establishmentIds =>
                    establishmentIds.SequenceEqual(new[] { FocusedEstablishmentId })
                )
            )
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = FocusedEstablishmentId,
                    SectionId = SectionId,
                },
                new SqlSubmissionDto
                {
                    EstablishmentId = FocusedEstablishmentId,
                    SectionId = "other-section",
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelfAssessmentSummary(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(SelfAssessmentSummaryViewBuilder.SelfAssessmentSummaryViewName, view.ViewName);
        var model = Assert.IsType<SelfAssessmentSummaryViewModel>(view.Model);
        Assert.False(model.IsMatSummary);
        Assert.Equal("Cyber security", model.SectionName);
        Assert.Equal("digital leadership", model.CategoryName);
        Assert.Equal(1, model.CompletedSchoolCount);
        Assert.True(model.ShowSubmitAnotherSelfAssessment);
        Assert.Equal("/digital-leadership", model.SubmitAnotherSelfAssessmentHref);
        Assert.Equal(UrlConstants.HomePage, model.BackToHomeHref);
        var link = Assert.Single(model.RecommendationLinks);
        Assert.Equal("View the recommendations for cyber security", link.LinkText);
        Assert.Equal("/digital-leadership", link.Href);
        Assert.Null(link.SchoolUrn);
        Assert.Null(link.SchoolName);
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenSchoolUserHasNoCompletedSection_ReturnsViewWithoutLinks()
    {
        // Arrange
        _currentUserProvider.CanSelectSchools.Returns(false);
        _currentUserProvider.IsMat.Returns(false);
        _currentUserProvider
            .GetFocusedEstablishmentAsync()
            .Returns(
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FocusedEstablishmentId)
                    .Build()
            );
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(Section);
        _contentfulService.GetAllCategoriesAsync().Returns([]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = FocusedEstablishmentId,
                    SectionId = "other-section",
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelfAssessmentSummary(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SelfAssessmentSummaryViewModel>(view.Model);
        Assert.Equal(0, model.CompletedSchoolCount);
        Assert.Empty(model.RecommendationLinks);
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenSectionBelongsToCategory_UsesCategoryHeaderAndLandingPageSlug()
    {
        // Arrange
        _currentUserProvider.CanSelectSchools.Returns(false);
        _currentUserProvider.IsMat.Returns(false);
        _currentUserProvider
            .GetFocusedEstablishmentAsync()
            .Returns(
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FocusedEstablishmentId)
                    .Build()
            );
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(Section);
        _contentfulService
            .GetAllCategoriesAsync()
            .Returns([
                new QuestionnaireCategoryEntry
                {
                    Header = new ComponentHeaderEntry
                    {
                        Text = "Digital leadership and governance",
                    },
                    LandingPage = new PageEntry { Slug = "leadership-governance-standard" },
                    Sections = [Section],
                },
            ]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = FocusedEstablishmentId,
                    SectionId = SectionId,
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelfAssessmentSummary(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SelfAssessmentSummaryViewModel>(view.Model);
        Assert.Equal("Digital leadership and governance", model.CategoryName);
        Assert.Equal("/leadership-governance-standard", model.SubmitAnotherSelfAssessmentHref);
        var link = Assert.Single(model.RecommendationLinks);
        Assert.Equal("/leadership-governance-standard", link.Href);
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenMatHasCompletedSchools_ReturnsMatSummaryView()
    {
        // Arrange
        _currentUserProvider.CanSelectSchools.Returns(true);
        _currentUserProvider.IsMat.Returns(true);
        _currentUserProvider
            .GetSelectedSchools()
            .Returns([
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FirstSelectedEstablishmentId)
                    .Build(),
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(SecondSelectedEstablishmentId)
                    .Build(),
            ]);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(Section);
        _contentfulService
            .GetAllCategoriesAsync()
            .Returns([
                new QuestionnaireCategoryEntry
                {
                    Header = new ComponentHeaderEntry
                    {
                        Text = "Digital leadership and governance",
                    },
                    LandingPage = new PageEntry { Slug = "leadership-governance-standard" },
                    Sections = [Section],
                },
            ]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(establishmentIds =>
                    establishmentIds.SequenceEqual(
                        new[] { FirstSelectedEstablishmentId, SecondSelectedEstablishmentId }
                    )
                )
            )
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = FirstSelectedEstablishmentId,
                    SectionId = SectionId,
                    Establishment = new SqlEstablishmentDtoBuilder()
                        .WithId(FirstSelectedEstablishmentId)
                        .WithOrgName("School One")
                        .WithEstablishmentRef("100001")
                        .Build(),
                },
                new SqlSubmissionDto
                {
                    EstablishmentId = SecondSelectedEstablishmentId,
                    SectionId = SectionId,
                    Establishment = new SqlEstablishmentDtoBuilder()
                        .WithId(SecondSelectedEstablishmentId)
                        .WithOrgName("School Two")
                        .WithEstablishmentRef("100002")
                        .Build(),
                },
                new SqlSubmissionDto
                {
                    EstablishmentId = SecondSelectedEstablishmentId,
                    SectionId = "other-section",
                    Establishment = new SqlEstablishmentDtoBuilder()
                        .WithId(SecondSelectedEstablishmentId)
                        .WithOrgName("School Two")
                        .WithEstablishmentRef("100002")
                        .Build(),
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelfAssessmentSummary(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(SelfAssessmentSummaryViewBuilder.SelfAssessmentSummaryViewName, view.ViewName);
        var model = Assert.IsType<SelfAssessmentSummaryViewModel>(view.Model);
        Assert.True(model.IsMatSummary);
        Assert.Equal("Cyber security", model.SectionName);
        Assert.Equal("Digital leadership and governance", model.CategoryName);
        Assert.Equal(2, model.CompletedSchoolCount);
        Assert.Equal(
            $"/trust/{UrlConstants.GroupSelfAssessmentSelectionSlug}",
            model.SubmitAnotherSelfAssessmentHref
        );
        Assert.Collection(
            model.RecommendationLinks,
            link =>
            {
                Assert.Equal("School One", link.LinkText);
                Assert.Equal("School One", link.SchoolName);
                Assert.Equal("100001", link.SchoolUrn);
                Assert.Equal("/trust/leadership-governance-standard", link.Href);
            },
            link =>
            {
                Assert.Equal("School Two", link.LinkText);
                Assert.Equal("School Two", link.SchoolName);
                Assert.Equal("100002", link.SchoolUrn);
                Assert.Equal("/trust/leadership-governance-standard", link.Href);
            }
        );
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenMatSchoolHasNoOrganisationName_ReturnsUnknownEstablishmentLinkText()
    {
        // Arrange
        _currentUserProvider.CanSelectSchools.Returns(true);
        _currentUserProvider.IsMat.Returns(true);
        _currentUserProvider
            .GetSelectedSchools()
            .Returns([
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FirstSelectedEstablishmentId)
                    .Build(),
            ]);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(Section);
        _contentfulService.GetAllCategoriesAsync().Returns([]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = FirstSelectedEstablishmentId,
                    SectionId = SectionId,
                    Establishment = new SqlEstablishmentDtoBuilder()
                        .WithId(FirstSelectedEstablishmentId)
                        .WithOrgName(null)
                        .WithEstablishmentRef("100001")
                        .Build(),
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelfAssessmentSummary(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SelfAssessmentSummaryViewModel>(view.Model);
        var link = Assert.Single(model.RecommendationLinks);
        Assert.Equal("Unknown Establishment", link.LinkText);
        Assert.Null(link.SchoolName);
        Assert.Equal("100001", link.SchoolUrn);
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenMatHasNoCompletedSchools_RedirectsToGroupSelfAssessmentSelection()
    {
        // Arrange
        _currentUserProvider.CanSelectSchools.Returns(true);
        _currentUserProvider.IsMat.Returns(true);
        _currentUserProvider
            .GetSelectedSchools()
            .Returns([
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FirstSelectedEstablishmentId)
                    .Build(),
            ]);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(Section);
        _contentfulService.GetAllCategoriesAsync().Returns([]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns([
                new SqlSubmissionDto
                {
                    EstablishmentId = FirstSelectedEstablishmentId,
                    SectionId = "other-section",
                },
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToSelfAssessmentSummary(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal($"/trust/{UrlConstants.GroupSelfAssessmentSelectionSlug}", redirect.Url);
    }
}
