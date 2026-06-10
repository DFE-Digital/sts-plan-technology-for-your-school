using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class GroupsViewBuilderTests
{
    // --- helpers ------------------------------------------------------------

    private static IOptions<ContactOptionsConfiguration> Opt(string linkId = "contact-123") =>
        Options.Create(new ContactOptionsConfiguration { LinkId = linkId });

    private static GroupsViewBuilder CreateServiceUnderTest(
        IOptions<ContactOptionsConfiguration>? contactOpts = null,
        IContentfulService? contentful = null,
        IEstablishmentService? est = null,
        IGroupService? group = null,
        ICurrentUser? currentUser = null,
        ILogger<GroupsViewBuilder>? logger = null,
        ISubmissionService? submissionService = null
    )
    {
        contactOpts ??= Opt();
        contentful ??= Substitute.For<IContentfulService>();
        est ??= Substitute.For<IEstablishmentService>();
        group ??= Substitute.For<IGroupService>();
        currentUser ??= Substitute.For<ICurrentUser>();
        logger ??= NullLogger<GroupsViewBuilder>.Instance;
        submissionService ??= Substitute.For<ISubmissionService>();

        // Set up test scenario: A MAT/Group user who needs to select a school
        // User Organisation (the MAT/Group they belong to)
        var groupDsiId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        currentUser.UserOrganisationId.Returns(100);
        currentUser.UserOrganisationDsiId.Returns(groupDsiId);
        currentUser.UserOrganisationName.Returns("Test Academy Trust");
        currentUser.UserOrganisationUrn.Returns("GRP-100");
        currentUser.UserOrganisationTypeName.Returns("Multi-Academy Trust");
        currentUser.UserOrganisationReference.Returns("GRP-100");

        // For the "select a school" flow, ActiveEstablishmentId should match UserOrganisationId
        // since they haven't selected a specific school yet
        currentUser.GetActiveEstablishmentIdAsync().Returns(100);

        // No selected establishment by default - tests that need this should set it up explicitly
        // ActiveEstablishmentId, ActiveEstablishmentName, etc. not set
        // GroupSelectedSchoolUrn not set

        return new GroupsViewBuilder(logger, contactOpts, contentful, currentUser, est, group, submissionService);
    }

    private static QuestionnaireCategoryEntry MakeCategory(
        params QuestionnaireSectionEntry[] sections
    ) =>
        new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("cat-1"),
            Header = new ComponentHeaderEntry { Text = "Header" },
            Sections = sections.ToList(),
        };

    private static QuestionnaireSectionEntry MakeSection(string id, int countAnswers = 0) =>
        new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(id),
            Name = $"Sec {id}",
            Questions = Enumerable.Repeat(new QuestionnaireQuestionEntry(), countAnswers).ToList(),
            CoreRecommendations = Enumerable
                .Repeat(new RecommendationChunkEntry(), countAnswers)
                .ToList(),
        };

    // --- ctor guards --------------------------------------------------------

    [Fact]
    public void Ctor_Null_ContactOptions_Throws()
    {
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var current = Substitute.For<ICurrentUser>();
        var submissionService = Substitute.For<ISubmissionService>();

        Assert.Throws<ArgumentNullException>(() =>
            new GroupsViewBuilder(
                NullLogger<GroupsViewBuilder>.Instance,
                null!,
                contentful,
                current,
                est,
                group,
                submissionService
            )
        );
    }

    [Fact]
    public void Ctor_Null_Services_Throw()
    {
        var opts = Opt();
        var contentful = Substitute.For<IContentfulService>();
        var current = Substitute.For<ICurrentUser>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var submissionService = Substitute.For<ISubmissionService>();

        Assert.Throws<ArgumentNullException>(() =>
            new GroupsViewBuilder(
                NullLogger<GroupsViewBuilder>.Instance,
                opts,
                contentful,
                current,
                null!,
                group,
                submissionService
            )
        );
    }

    // --- RouteToSelectASchoolViewModelAsync --------------------------------

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_Builds_View_With_Expected_Model()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();

        // selection page content (only Content list is used)
        contentful
            .GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug)
            .Returns(
                new PageEntry
                {
                    Content = new List<ContentfulEntry> { new MissingComponentEntry() },
                }
            );
        var sec1 = MakeSection("Sec1", 3);
        var sec2 = MakeSection("Sec2", 2);
        var sec3 = MakeSection("Sec3", 5);
        var cat1 = MakeCategory(sec1, sec2);
        var cat2 = MakeCategory(sec3);
        var allSections = new List<QuestionnaireSectionEntry> { sec1, sec2, sec3 };

        contentful.GetAllSectionsAsync().Returns(allSections);

        // home page content: supplies categories with sections
        var homePage = new PageEntry
        {
            Content = new List<ContentfulEntry> { cat1, cat2 },
        };
        contentful.GetPageBySlugAsync(Arg.Any<string>()).Returns(homePage);

        // contact link
        contentful
            .GetLinkByIdAsync("contact-123")
            .Returns(new NavigationLinkEntry { Href = "/contact-us" });

        var est = Substitute.For<IEstablishmentService>();
        est.GetEstablishmentLinksWithRecommendationCounts(100)
            .Returns(
                new List<SqlEstablishmentLinkDto>
                {
                    new SqlEstablishmentLinkDto
                    {
                        Id = 1,
                        EstablishmentName = "School A",
                        InProgressOrCompletedRecommendationsCount = 2,
                    },
                    new SqlEstablishmentLinkDto
                    {
                        Id = 2,
                        EstablishmentName = "School B",
                        InProgressOrCompletedRecommendationsCount = 1,
                    },
                }
            );

        var sut = CreateServiceUnderTest(contentful: contentful, est: est);
        var controller = new TestController();

        // Act
        var action = await sut.RouteToSelectASchoolViewModelAsync(controller);

        // Assert
        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal("GroupsSelectSchool", view.ViewName);
        Assert.Equal("Select a school", controller.ViewData[StatePassingMechanismConstants.Title]);

        var vm = Assert.IsType<GroupsSelectorViewModel>(view.Model);
        Assert.Equal("Test Academy Trust", vm.GroupName);
        Assert.Equal("Test Academy Trust", vm.Title.Text);
        Assert.Equal("10", vm.TotalRecommendations); // total of all values passed to MakeSection above
        Assert.Null(vm.ProgressRetrievalErrorMessage);
        Assert.Equal("/contact-us", vm.ContactLinkHref);

        // establishments list comes from service
        Assert.Equal(2, vm.GroupEstablishments.Count);
        await contentful.Received(1).GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug);
        await contentful.Received(1).GetLinkByIdAsync("contact-123");
        await est.Received(1).GetEstablishmentLinksWithRecommendationCounts(100);
    }

    // --- RecordGroupSelectionAsync -----------------------------------------

    [Fact]
    public async Task RecordGroupSelectionAsync_Forwards_All_Parameters_To_Service()
    {
        var est = Substitute.For<IEstablishmentService>();
        var currentUser = Substitute.For<ICurrentUser>();

        // Set up a different MAT/group than the default to test parameter forwarding
        var customGroupDsiId = Guid.Parse("20000000-0000-0000-0000-000000000002");

        var sut = CreateServiceUnderTest(est: est, currentUser: currentUser);

        // Override defaults with custom test values
        currentUser.DsiReference.Returns("dsi-123");
        currentUser.UserOrganisationId.Returns(200);
        currentUser.UserOrganisationDsiId.Returns(customGroupDsiId);
        currentUser.UserOrganisationName.Returns("Custom Academy Trust");
        currentUser.UserOrganisationUrn.Returns("GRP-200");

        await sut.RecordGroupSelectionAsync("URN-007", "Bond Primary");

        await est.Received(1)
            .RecordGroupSelection(
                "dsi-123",
                200,
                Arg.Is<EstablishmentModel>(m =>
                    m.Id == customGroupDsiId
                    && m.Name == "Custom Academy Trust"
                    && m.Urn == "GRP-200"
                ),
                "URN-007",
                "Bond Primary"
            );
    }

    // --- RouteToSelectASelfAssessmentViewModelAsync ----------------------------

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_Builds_View_With_Expected_Model()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();

        var sec1 = MakeSection("SEC-1");
        var sec2 = MakeSection("SEC-2");
        var category = MakeCategory(sec1, sec2);

        contentful
            .GetAllCategoriesAsync()
            .Returns(new List<QuestionnaireCategoryEntry> { category });

        est.GetEstablishmentLinks(100)
            .Returns(
                new List<SqlEstablishmentLinkDto>
                {
                    new() { Urn = "URN-1" },
                    new() { Urn = "URN-2" },
                    new() { Urn = "URN-2" }, // duplicate should be removed
                    new() { Urn = "" }, // blank should be ignored
                    new() { Urn = " " }, // whitespace should be ignored
                }
            );

        est.GetEstablishmentsByReferencesAsync(Arg.Any<string[]>())
            .Returns(
                new List<SqlEstablishmentDto>
                {
                    new() { Id = 10 },
                    new() { Id = 20 },
                    new() { Id = 10 }, // duplicate establishment id should be removed
                }
            );

        group.GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns(
                new List<SqlSubmissionDto>
                {
                    // SEC-1 completed by both establishments, so uncompleted count should be 0
                    new() { Id = 1, EstablishmentId = 10, SectionId = "SEC-1" },
                    new() { Id = 2, EstablishmentId = 20, SectionId = "SEC-1" },

                    // SEC-2 completed by one establishment, so uncompleted count should be 1
                    new() { Id = 3, EstablishmentId = 10, SectionId = "SEC-2" },

                    // Not part of the MAT establishments, should be ignored
                    new() { Id = 4, EstablishmentId = 999, SectionId = "SEC-1" },
                }
            );

        var sut = CreateServiceUnderTest(contentful: contentful, est: est, group: group);
        var controller = new TestController();

        // Act
        var action = await sut.RouteToSelectASelfAssessmentViewModelAsync(controller);

        // Assert
        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal("GroupsSelectSelfAssessment", view.ViewName);

        var vm = Assert.IsType<GroupSelectAssessmentViewModel>(view.Model);
        Assert.Equal("Test Academy Trust", vm.GroupName);

        var categoryVm = Assert.Single(vm.Categories);
        Assert.Equal("Header", categoryVm.CategoryName);

        Assert.Collection(
            categoryVm.Sections,
            section =>
            {
                Assert.Equal("Sec SEC-1", section.SectionName);
                Assert.Equal(0, section.UncompletedGroupSubmissions);
            },
            section =>
            {
                Assert.Equal("Sec SEC-2", section.SectionName);
                Assert.Equal(1, section.UncompletedGroupSubmissions);
            }
        );

        await est.Received(1).GetEstablishmentLinks(100);

        await est.Received(1)
            .GetEstablishmentsByReferencesAsync(
                Arg.Is<string[]>(urns =>
                    urns.SequenceEqual(new[] { "URN-1", "URN-2" })
                )
            );

        await group.Received(1)
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids =>
                    ids.SequenceEqual(new[] { 10, 20 })
                )
            );
    }

    [Fact]
    public async Task RouteToViewInProgressAnswers_WhenSubmissionInProgress_ReturnsView()
    {
        var contentful = Substitute.For<IContentfulService>();
        var submissionService = Substitute.For<ISubmissionService>();
        var currentUser = Substitute.For<ICurrentUser>();
        var est = Substitute.For<IEstablishmentService>();

        est.GetEstablishmentByReferenceAsync("900006")
            .Returns(new SqlEstablishmentDto
            {
                Id = 100,
                OrgName = "Test School",
                EstablishmentRef = "900006"
            });

        var section = new QuestionnaireSectionEntry
        {
            Name = "Cyber security processes",
            Questions =
            [
                new QuestionnaireQuestionEntry
            {
                Sys = new SystemDetails("question-1")
            }
            ]
        };

        contentful.GetSectionBySlugAsync("cyber-security-processes").Returns(section);

        var submission = new SubmissionResponsesModel(
           1,
           [
               new QuestionWithAnswerModel
               {
                    QuestionSysId = "question-1",
                    QuestionText = "Question 1",
                    AnswerText = "Answer 1",
                    Order = 1
               }
           ])
        {
            DateCreated = new DateTime(2026, 1, 1)
        };

        var routingData = new SubmissionRoutingDataModel(
            nextQuestion: null,
            questionnaireSection: section,
            submission: submission,
            status: SubmissionStatus.InProgress
        );

        submissionService
            .GetSubmissionRoutingDataAsync(100, section, SubmissionStatus.InProgress)
            .Returns(routingData);

        var sut = CreateServiceUnderTest(
          contentful: contentful,
          est: est,
          currentUser: currentUser,
          submissionService: submissionService
      );

        var controller = new TestController();

        var result = await sut.RouteToViewInProgressAnswers(
            controller,
            "cyber-security-standard",
            "cyber-security-processes",
            "900006"
        );

        var view = Assert.IsType<ViewResult>(result);

        Assert.Equal(ReviewAnswersViewBuilder.ViewAnswersViewName, view.ViewName);

        var model = Assert.IsType<ViewAnswersViewModel>(view.Model);

        Assert.True(model.IsMatInProgressView);
        Assert.Equal("Test School", model.SchoolName);
        Assert.Equal(1, model.QuestionsAnswered);
        Assert.Equal(1, model.TotalQuestions);
        Assert.True(model.ShowInProgressDisclaimer);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_Throws_When_No_Categories_Found()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();

        contentful
            .GetAllCategoriesAsync()
            .Returns(new List<QuestionnaireCategoryEntry>());

        var sut = CreateServiceUnderTest(contentful: contentful);
        var controller = new TestController();

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            sut.RouteToSelectASelfAssessmentViewModelAsync(controller)
        );

        // Assert
        Assert.Equal(
            "No categories found on groups assessment selection page.",
            exception.Message
        );
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_When_No_Establishments_Does_Not_Call_GroupService()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();

        var sec1 = MakeSection("SEC-1");
        var category = MakeCategory(sec1);

        contentful
            .GetAllCategoriesAsync()
            .Returns(new List<QuestionnaireCategoryEntry> { category });

        est.GetEstablishmentLinks(100)
            .Returns(new List<SqlEstablishmentLinkDto>());

        est.GetEstablishmentsByReferencesAsync(Arg.Any<string[]>())
            .Returns(new List<SqlEstablishmentDto>());

        var sut = CreateServiceUnderTest(contentful: contentful, est: est, group: group);
        var controller = new TestController();

        // Act
        var action = await sut.RouteToSelectASelfAssessmentViewModelAsync(controller);

        // Assert
        var view = Assert.IsType<ViewResult>(action);
        var vm = Assert.IsType<GroupSelectAssessmentViewModel>(view.Model);

        var categoryVm = Assert.Single(vm.Categories);
        var sectionVm = Assert.Single(categoryVm.Sections);

        Assert.Equal("Sec SEC-1", sectionVm.SectionName);
        Assert.Equal(0, sectionVm.UncompletedGroupSubmissions);

        await group
            .DidNotReceive()
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>());
    }

    [Fact]
    public async Task RouteToViewInProgressAnswers_WhenSchoolNotFound_RedirectsHome()
    {
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();

        est.GetEstablishmentByReferenceAsync("900006")
            .Returns((SqlEstablishmentDto?)null);

        var sut = CreateServiceUnderTest(contentful: contentful, est: est);
        var controller = new TestController();

        var result = await sut.RouteToViewInProgressAnswers(
            controller,
            "cyber-security-standard",
            "cyber-security-processes",
            "900006"
        );

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetByRoute", redirect.ActionName);
    }

    [Fact]
    public async Task RouteToViewInProgressAnswers_WhenSubmissionNotStarted_RedirectsHome()
    {
        var contentful = Substitute.For<IContentfulService>();
        var submissionService = Substitute.For<ISubmissionService>();
        var est = Substitute.For<IEstablishmentService>();

        est.GetEstablishmentByReferenceAsync("900006")
            .Returns(new SqlEstablishmentDto
            {
                Id = 100,
                OrgName = "Test School",
                EstablishmentRef = "900006"
            });

        var section = new QuestionnaireSectionEntry
        {
            Name = "Cyber security processes",
            Questions = []
        };

        contentful.GetSectionBySlugAsync("cyber-security-processes").Returns(section);

        var routingData = new SubmissionRoutingDataModel(
            nextQuestion: null,
            questionnaireSection: section,
            submission: null,
            status: SubmissionStatus.NotStarted
        );

        submissionService
            .GetSubmissionRoutingDataAsync(100, section, SubmissionStatus.InProgress)
            .Returns(routingData);

        var sut = CreateServiceUnderTest(
            contentful: contentful,
            est: est,
            submissionService: submissionService
        );

        var controller = new TestController();

        var result = await sut.RouteToViewInProgressAnswers(
            controller,
            "cyber-security-standard",
            "cyber-security-processes",
            "900006"
        );

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetByRoute", redirect.ActionName);
    }

    [Fact]
    public async Task RouteToSelectASelfAssessmentViewModelAsync_Populates_Section_Slugs()
    {
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();

        var section = MakeSection("SEC-1");
        var category = MakeCategory(section);

        contentful
            .GetAllCategoriesAsync()
            .Returns(new List<QuestionnaireCategoryEntry> { category });

        est.GetEstablishmentLinks(100)
            .Returns(new List<SqlEstablishmentLinkDto>());

        est.GetEstablishmentsByReferencesAsync(Arg.Any<string[]>())
            .Returns(new List<SqlEstablishmentDto>());

        var sut = CreateServiceUnderTest(contentful: contentful, est: est, group: group);
        var controller = new TestController();

        var action = await sut.RouteToSelectASelfAssessmentViewModelAsync(controller);

        var view = Assert.IsType<ViewResult>(action);
        var vm = Assert.IsType<GroupSelectAssessmentViewModel>(view.Model);

        var categoryVm = Assert.Single(vm.Categories);
        var sectionVm = Assert.Single(categoryVm.Sections);

        Assert.Equal("header", sectionVm.CategorySlug);
        Assert.Equal("sec-sec1", sectionVm.SectionSlug);
    }
}
