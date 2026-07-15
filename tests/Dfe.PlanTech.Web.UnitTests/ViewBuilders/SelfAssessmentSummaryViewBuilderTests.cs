using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class SelfAssessmentSummaryViewBuilderTests
{
    private readonly ILogger<SelfAssessmentSummaryViewBuilder> _logger =
        Substitute.For<ILogger<SelfAssessmentSummaryViewBuilder>>();

    private readonly IContentfulService _contentful =
        Substitute.For<IContentfulService>();

    private readonly ICurrentUserProvider _currentUser =
        Substitute.For<ICurrentUserProvider>();

    private readonly IGroupService _groupService =
        Substitute.For<IGroupService>();

    private readonly IHttpContextAccessor _httpContextAccessor =
        Substitute.For<IHttpContextAccessor>();

    private SelfAssessmentSummaryViewBuilder CreateSut() =>
        new(
            _logger,
            _contentful,
            _currentUser,
            _groupService,
            _httpContextAccessor
        );

    private static Controller MakeController()
    {
        var controller = new DummyController();
        var httpContext = new DefaultHttpContext
        {
            Session = new TestSession()
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenSchoolUser_ReturnsSchoolSummaryViewModel()
    {
        var sut = CreateSut();
        var controller = MakeController();

        _currentUser.IsMat.Returns(false);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);

        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("section-1"),
            Name = "Cyber security"
        };

        _contentful.GetSectionBySlugAsync("cyber-security").Returns(section);
        _contentful.GetAllCategoriesAsync().Returns([]);

        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Is<int[]>(ids =>
                ids.SequenceEqual(new[] { 123 })
            ))
            .Returns(
            [
                new SqlSubmissionDto
                {
                    Id = 1,
                    EstablishmentId = 123,
                    SectionId = "section-1",
                    SectionName = "Cyber security"
                },
                new SqlSubmissionDto
                {
                    Id = 2,
                    EstablishmentId = 123,
                    SectionId = "other-section",
                    SectionName = "Other"
                }
            ]);

        var result = await sut.RouteToSelfAssessmentSummary(
            controller,
            "digital-leadership",
            "cyber-security"
        );

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(SelfAssessmentSummaryViewBuilder.SelfAssessmentSummaryViewName, view.ViewName);

        var model = Assert.IsType<SelfAssessmentSummaryViewModel>(view.Model);

        Assert.False(model.IsMatSummary);
        Assert.Equal("Cyber security", model.SectionName);
        Assert.Equal("digital leadership", model.CategoryName);
        Assert.Equal(1, model.CompletedSchoolCount);
        Assert.Equal("/digital-leadership", model.SubmitAnotherSelfAssessmentHref);
        Assert.Equal(UrlConstants.HomePage, model.BackToHomeHref);

        var link = Assert.Single(model.RecommendationLinks);
        Assert.Equal("View the recommendations for cyber security", link.LinkText);
        Assert.Equal("/digital-leadership", link.Href);
        Assert.Null(link.SchoolUrn);
        Assert.Null(link.SchoolName);
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenMatUser_ReturnsMatSummaryViewModel()
    {
        var sut = CreateSut();
        var controller = MakeController();

        _httpContextAccessor.HttpContext.Returns(controller.HttpContext);

        _currentUser.IsMat.Returns(true);

        IEnumerable<int> selectedEstablishmentIds = [101, 102];

        controller.HttpContext.Session.SetValue(
            SessionConstants.SelectedEstablishmentsKey,
            selectedEstablishmentIds
        );

        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("section-1"),
            Name = "Cyber security"
        };

        _contentful.GetSectionBySlugAsync("cyber-security").Returns(section);
        _contentful.GetAllCategoriesAsync().Returns([]);

        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Is<int[]>(ids =>
                ids.SequenceEqual(new[] { 101, 102 })
            ))
            .Returns(
            [
                new SqlSubmissionDto
                {
                    Id = 1,
                    EstablishmentId = 101,
                    SectionId = "section-1",
                    SectionName = "Cyber security",
                    Establishment = new SqlEstablishmentDto
                    {
                        OrgName = "School One",
                        EstablishmentRef = "100001"
                    }
                },
                new SqlSubmissionDto
                {
                    Id = 2,
                    EstablishmentId = 102,
                    SectionId = "section-1",
                    SectionName = "Cyber security",
                    Establishment = new SqlEstablishmentDto
                    {
                        OrgName = "School Two",
                        EstablishmentRef = "100002"
                    }
                },
                new SqlSubmissionDto
                {
                    Id = 3,
                    EstablishmentId = 102,
                    SectionId = "other-section",
                    SectionName = "Other",
                    Establishment = new SqlEstablishmentDto
                    {
                        OrgName = "School Two",
                        EstablishmentRef = "100002"
                    }
                }
            ]);

        var result = await sut.RouteToSelfAssessmentSummary(
            controller,
            "digital-leadership",
            "cyber-security"
        );

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(SelfAssessmentSummaryViewBuilder.SelfAssessmentSummaryViewName, view.ViewName);

        var model = Assert.IsType<SelfAssessmentSummaryViewModel>(view.Model);

        Assert.True(model.IsMatSummary);
        Assert.Equal("Cyber security", model.SectionName);
        Assert.Equal("digital leadership", model.CategoryName);
        Assert.Equal(2, model.CompletedSchoolCount);
        Assert.Equal(
            $"/groups/{UrlConstants.GroupSelfAssessmentSelectionSlug}",
            model.SubmitAnotherSelfAssessmentHref
        );

        Assert.Collection(
            model.RecommendationLinks,
            link =>
            {
                Assert.Equal("School One", link.LinkText);
                Assert.Equal("School One", link.SchoolName);
                Assert.Equal("100001", link.SchoolUrn);
                Assert.Equal("/digital-leadership", link.Href);
            },
            link =>
            {
                Assert.Equal("School Two", link.LinkText);
                Assert.Equal("School Two", link.SchoolName);
                Assert.Equal("100002", link.SchoolUrn);
                Assert.Equal("/digital-leadership", link.Href);
            }
        );
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenMatHasNoSelectedEstablishments_UsesActiveEstablishment()
    {
        var sut = CreateSut();
        var controller = MakeController();

        _httpContextAccessor.HttpContext.Returns(controller.HttpContext);

        _currentUser.IsMat.Returns(true);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(999);

        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("section-1"),
            Name = "Cyber security"
        };

        _contentful.GetSectionBySlugAsync("cyber-security").Returns(section);
        _contentful.GetAllCategoriesAsync().Returns([]);

        _groupService
            .GetGroupCompletedSubmissionsBySections(Arg.Is<int[]>(ids =>
                ids.SequenceEqual(new[] { 999 })
            ))
            .Returns(
            [
                new SqlSubmissionDto
                {
                    Id = 1,
                    EstablishmentId = 999,
                    SectionId = "section-1",
                    SectionName = "Cyber security",
                    Establishment = new SqlEstablishmentDto
                    {
                        OrgName = "Active School",
                        EstablishmentRef = "900999"
                    }
                }
            ]);

        var result = await sut.RouteToSelfAssessmentSummary(
            controller,
            "digital-leadership",
            "cyber-security"
        );

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SelfAssessmentSummaryViewModel>(view.Model);

        Assert.True(model.IsMatSummary);
        Assert.Equal(1, model.CompletedSchoolCount);

        var link = Assert.Single(model.RecommendationLinks);
        Assert.Equal("Active School", link.LinkText);
        Assert.Equal("900999", link.SchoolUrn);

        await _groupService
            .Received(1)
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 999 }))
            );
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenSectionMissing_ThrowsContentfulDataUnavailableException()
    {
        var sut = CreateSut();
        var controller = MakeController();

        _contentful
            .GetSectionBySlugAsync("missing-section")
            .Returns((QuestionnaireSectionEntry?)null);

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteToSelfAssessmentSummary(
                controller,
                "category",
                "missing-section"
            )
        );
    }

    [Fact]
    public async Task RouteToSelfAssessmentSummary_WhenSectionIdMissing_ThrowsContentfulDataUnavailableException()
    {
        var sut = CreateSut();
        var controller = MakeController();

        _contentful
            .GetSectionBySlugAsync("missing-section-id")
            .Returns(new QuestionnaireSectionEntry
            {
                Name = "Section"
            });

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteToSelfAssessmentSummary(
                controller,
                "category",
                "missing-section-id"
            )
        );
    }

    [Fact]
    public void Constructor_WithNullGroupService_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new SelfAssessmentSummaryViewBuilder(
                _logger,
                _contentful,
                _currentUser,
                null!,
                _httpContextAccessor
            )
        );

        Assert.Equal("groupService", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullHttpContextAccessor_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new SelfAssessmentSummaryViewBuilder(
                _logger,
                _contentful,
                _currentUser,
                _groupService,
                null!
            )
        );

        Assert.Equal("httpContextAccessor", ex.ParamName);
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = [];

        public IEnumerable<string> Keys => _store.Keys;

        public string Id { get; } = Guid.NewGuid().ToString();

        public bool IsAvailable => true;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value) =>
            _store.TryGetValue(key, out value!);
    }

    private sealed class DummyController : Controller { }
}
