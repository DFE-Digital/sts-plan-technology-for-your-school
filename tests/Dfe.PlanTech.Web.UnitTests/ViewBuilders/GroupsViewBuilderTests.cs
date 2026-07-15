using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Text;
using System.Text.Json;

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
        ISubmissionService? submission = null,
        ICurrentUserProvider? currentUser = null,
        ILogger<GroupsViewBuilder>? logger = null
    )
    {
        contactOpts ??= Opt();
        contentful ??= Substitute.For<IContentfulService>();
        est ??= Substitute.For<IEstablishmentService>();
        group ??= Substitute.For<IGroupService>();
        submission ??= Substitute.For<ISubmissionService>();
        currentUser ??= Substitute.For<ICurrentUserProvider>();
        logger ??= NullLogger<GroupsViewBuilder>.Instance;

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

        return new GroupsViewBuilder(
            logger,
            contactOpts,
            contentful,
            currentUser,
            est,
            group,
            submission
        );
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
        var current = Substitute.For<ICurrentUserProvider>();
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
        var current = Substitute.For<ICurrentUserProvider>();
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

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_Shows_SelectSelfAssessment_When_Assessments_Remain()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();

        var sec1 = MakeSection("SEC-1", 1);
        var sec2 = MakeSection("SEC-2", 1);

        contentful
            .GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug)
            .Returns(new PageEntry());

        contentful
            .GetAllSectionsAsync()
            .Returns(new List<QuestionnaireSectionEntry> { sec1, sec2 });

        contentful
            .GetLinkByIdAsync("contact-123")
            .Returns(new NavigationLinkEntry { Href = "/contact-us" });

        est.GetEstablishmentLinksWithRecommendationCounts(100)
            .Returns(
                new List<SqlEstablishmentLinkDto>
                {
                new()
                {
                    Id = 10,
                    Urn = "URN-1",
                    EstablishmentName = "School A"
                },
                new()
                {
                    Id = 20,
                    Urn = "URN-2",
                    EstablishmentName = "School B"
                }
                }
            );

        est.GetEstablishmentsByReferencesAsync(Arg.Any<string[]>())
            .Returns(
                new List<SqlEstablishmentDto>
                {
                new() { Id = 10 },
                new() { Id = 20 }
                }
            );

        group.GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns(
                new List<SqlSubmissionDto>
                {
                new()
                {
                    Id = 1,
                    EstablishmentId = 10,
                    SectionId = "SEC-1"
                },
                new()
                {
                    Id = 2,
                    EstablishmentId = 20,
                    SectionId = "SEC-1"
                },
                new()
                {
                    Id = 3,
                    EstablishmentId = 10,
                    SectionId = "SEC-2"
                }
                }
            );

        var sut = CreateServiceUnderTest(
            contentful: contentful,
            est: est,
            group: group
        );

        var controller = new TestController();

        // Act
        var action = await sut.RouteToSelectASchoolViewModelAsync(controller);

        // Assert
        var view = Assert.IsType<ViewResult>(action);
        var vm = Assert.IsType<GroupsSelectorViewModel>(view.Model);

        Assert.True(vm.ShowSelectSelfAssessmentToSubmit);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_Hides_SelectSelfAssessment_When_All_Assessments_Complete()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();

        var sec1 = MakeSection("SEC-1", 1);
        var sec2 = MakeSection("SEC-2", 1);

        contentful
            .GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug)
            .Returns(new PageEntry());

        contentful
            .GetAllSectionsAsync()
            .Returns(new List<QuestionnaireSectionEntry> { sec1, sec2 });

        contentful
            .GetLinkByIdAsync("contact-123")
            .Returns(new NavigationLinkEntry { Href = "/contact-us" });

        est.GetEstablishmentLinksWithRecommendationCounts(100)
            .Returns(
                new List<SqlEstablishmentLinkDto>
                {
                new()
                {
                    Id = 10,
                    Urn = "URN-1",
                    EstablishmentName = "School A"
                },
                new()
                {
                    Id = 20,
                    Urn = "URN-2",
                    EstablishmentName = "School B"
                }
                }
            );

        est.GetEstablishmentsByReferencesAsync(Arg.Any<string[]>())
            .Returns(
                new List<SqlEstablishmentDto>
                {
                new() { Id = 10 },
                new() { Id = 20 }
                }
            );

        group.GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns(
                new List<SqlSubmissionDto>
                {
                new()
                {
                    Id = 1,
                    EstablishmentId = 10,
                    SectionId = "SEC-1"
                },
                new()
                {
                    Id = 2,
                    EstablishmentId = 20,
                    SectionId = "SEC-1"
                },
                new()
                {
                    Id = 3,
                    EstablishmentId = 10,
                    SectionId = "SEC-2"
                },
                new()
                {
                    Id = 4,
                    EstablishmentId = 20,
                    SectionId = "SEC-2"
                }
                }
            );

        var sut = CreateServiceUnderTest(
            contentful: contentful,
            est: est,
            group: group
        );

        var controller = new TestController();

        // Act
        var action = await sut.RouteToSelectASchoolViewModelAsync(controller);

        // Assert
        var view = Assert.IsType<ViewResult>(action);
        var vm = Assert.IsType<GroupsSelectorViewModel>(view.Model);

        Assert.False(vm.ShowSelectSelfAssessmentToSubmit);
    }

    // --- RecordGroupSelectionAsync -----------------------------------------

    [Fact]
    public async Task RecordGroupSelectionAsync_Forwards_All_Parameters_To_Service()
    {
        var est = Substitute.For<IEstablishmentService>();
        var currentUser = Substitute.For<ICurrentUserProvider>();

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

        group
            .GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>())
            .Returns(
                new List<SqlSubmissionDto>
                {
                    // SEC-1 completed by both establishments, so uncompleted count should be 0
                    new()
                    {
                        Id = 1,
                        EstablishmentId = 10,
                        SectionId = "SEC-1",
                    },
                    new()
                    {
                        Id = 2,
                        EstablishmentId = 20,
                        SectionId = "SEC-1",
                    },
                    // SEC-2 completed by one establishment, so uncompleted count should be 1
                    new()
                    {
                        Id = 3,
                        EstablishmentId = 10,
                        SectionId = "SEC-2",
                    },
                    // Not part of the MAT establishments, should be ignored
                    new()
                    {
                        Id = 4,
                        EstablishmentId = 999,
                        SectionId = "SEC-1",
                    },
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
                Arg.Is<string[]>(urns => urns.SequenceEqual(new[] { "URN-1", "URN-2" }))
            );

        await group
            .Received(1)
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 10, 20 }))
            );
    }

    [Fact]
    public async Task RouteToViewInProgressAnswers_WhenSubmissionInProgress_ReturnsView()
    {
        var contentful = Substitute.For<IContentfulService>();
        var submissionService = Substitute.For<ISubmissionService>();
        var currentUser = Substitute.For<ICurrentUserProvider>();
        var est = Substitute.For<IEstablishmentService>();

        est.GetEstablishmentByReferenceAsync("900006")
            .Returns(
                new SqlEstablishmentDto
                {
                    Id = 100,
                    OrgName = "Test School",
                    EstablishmentRef = "900006",
                }
            );

        var section = new QuestionnaireSectionEntry
        {
            Name = "Cyber security processes",
            Questions = [new QuestionnaireQuestionEntry { Sys = new SystemDetails("question-1") }],
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
                    Order = 1,
                },
            ]
        )
        {
            DateCreated = new DateTime(2026, 1, 1),
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
            submission: submissionService
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

        contentful.GetAllCategoriesAsync().Returns(new List<QuestionnaireCategoryEntry>());

        var sut = CreateServiceUnderTest(contentful: contentful);
        var controller = new TestController();

        // Act
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteToSelectASelfAssessmentViewModelAsync(controller)
        );

        // Assert
        Assert.Equal("No categories found on groups assessment selection page.", exception.Message);
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

        est.GetEstablishmentLinks(100).Returns(new List<SqlEstablishmentLinkDto>());

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

        await group.DidNotReceive().GetGroupCompletedSubmissionsBySections(Arg.Any<int[]>());
    }

    // --- RouteToSelectSchoolsToAssessViewModelAsync ----------------------------
    [Fact]
    public async Task RouteToSelectSchoolsToAssessViewModelAsync_Builds_View_With_Expected_Model()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var session = Substitute.For<ISession>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var categorySlug = "category-one";
        var sectionSlug = "section-one";

        var section = new QuestionnaireSectionEntry()
        {
            Sys = new SystemDetails { Id = "sec1" },
            InterstitialPage = new PageEntry { Slug = sectionSlug },
            Questions = new List<QuestionnaireQuestionEntry>() { new() { Slug = "question" } },
        };

        contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        var establishmentLinks = new List<SqlEstablishmentLinkDto>()
        {
            new() { Urn = "testRef10", EstablishmentName = "Test 10" },
            new() { Urn = "testRef20", EstablishmentName = "Test 20" },
            new() { Urn = "testRef30", EstablishmentName = "Test 30" },
        };

        est.GetEstablishmentLinks(100).Returns(establishmentLinks);

        var establishment10 = new SqlEstablishmentDto()
        {
            Id = 10,
            EstablishmentRef = "testRef10",
            OrgName = "Test 10",
        };
        var establishment20 = new SqlEstablishmentDto()
        {
            Id = 20,
            EstablishmentRef = "testRef20",
            OrgName = "Test 20",
        };
        var establishment30 = new SqlEstablishmentDto()
        {
            Id = 30,
            EstablishmentRef = "testRef30",
            OrgName = "Test 30",
        };

        est.GetEstablishmentByReferenceAsync("testRef10").Returns(establishment10);
        est.GetEstablishmentByReferenceAsync("testRef20").Returns(establishment20);
        est.GetEstablishmentByReferenceAsync("testRef30").Returns(establishment30);

        var submissionInfo = new List<SubmissionInformationModel>
        {
            new()
            {
                SubmissionId = 1,
                SectionId = section.Id,
                EstablishmentId = 10,
                EstablishmentName = "testName10",
                Status = SubmissionStatus.InProgress,
            },
            new()
            {
                SubmissionId = 2,
                SectionId = section.Id,
                EstablishmentId = 20,
                EstablishmentName = "testName20",
                Status = SubmissionStatus.CompleteReviewed,
            },
            new()
            {
                SubmissionId = 3,
                SectionId = section.Id,
                EstablishmentId = 30,
                EstablishmentName = "testName30",
                Status = SubmissionStatus.NotStarted,
            },
        };

        group
            .GetGroupSubmissionInformationForSection(
                Arg.Is<List<SqlEstablishmentLinkDto>>(x => x.SequenceEqual(establishmentLinks)),
                section.Id
            )
            .Returns(submissionInfo);

        var expected = new GroupsSelectSchoolsToAssessViewModel
        {
            SchoolSubmissionInfo = submissionInfo,
            Section = section,
        };

        var sut = CreateServiceUnderTest(contentful: contentful, est: est, group: group);
        var controller = new TestController()
        {
            ControllerContext = new ControllerContext
            {
                RouteData = new RouteData(),
                HttpContext = httpContext,
            },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        // Act
        var action = await sut.RouteToSelectSchoolsToAssessViewModelAsync(controller, sectionSlug);

        // Assert
        await est.Received(1).GetEstablishmentLinks(100);

        await group
            .Received(1)
            .GetGroupSubmissionInformationForSection(
                Arg.Is<List<SqlEstablishmentLinkDto>>(urns =>
                    urns.SequenceEqual(establishmentLinks)
                ),
                section.Id
            );

        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal("GroupSelectSchoolsToAssess", view.ViewName);

        var vm = Assert.IsType<GroupsSelectSchoolsToAssessViewModel>(view.Model);

        Assert.Equal(section, vm.Section);
        Assert.Equal(categorySlug, vm.CategorySlug);

        Assert.Collection(
            vm.SchoolSubmissionInfo!,
            submission =>
            {
                Assert.Equal(1, submission.SubmissionId);
                Assert.Equal(10, submission.EstablishmentId);
                Assert.Equal("testName10", submission.EstablishmentName);
                Assert.Equal(SubmissionStatus.InProgress, submission.Status);
            },
            submission =>
            {
                Assert.Equal(3, submission.SubmissionId);
                Assert.Equal(30, submission.EstablishmentId);
                Assert.Equal("testName30", submission.EstablishmentName);
                Assert.Equal(SubmissionStatus.NotStarted, submission.Status);
            }
        );
    }

    [Fact]
    public async Task RouteToSelectSchoolsToAssessViewModelAsync_Throws_When_No_Section_Found()
    {
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var session = Substitute.For<ISession>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var categorySlug = "category-2";
        var sectionSlug = "section-2";

        var sut = CreateServiceUnderTest(contentful: contentful, est: est, group: group);
        var controller = new TestController()
        {
            ControllerContext = new ControllerContext
            {
                RouteData = new RouteData(),
                HttpContext = httpContext,
            },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        var establishmentLinks = new List<SqlEstablishmentLinkDto>()
        {
            new SqlEstablishmentLinkDto() { Id = 1 },
            new SqlEstablishmentLinkDto() { Id = 2 },
        };

        est.GetEstablishmentLinks(100).Returns(establishmentLinks);

        contentful.GetSectionBySlugAsync(sectionSlug).Returns((QuestionnaireSectionEntry?)null!);

        var ex = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteToSelectSchoolsToAssessViewModelAsync(controller, sectionSlug)
        );

        Assert.Equal($"Could not find topic for slug '{sectionSlug}'", ex.Message);
    }

    [Fact]
    public async Task RouteToSelectSchoolsToAssessViewModelAsync_Throws_When_No_Establishments()
    {
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var session = Substitute.For<ISession>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var categorySlug = "category-3";
        var sectionSlug = "section-3";
        var section = new QuestionnaireSectionEntry();

        var sut = CreateServiceUnderTest(contentful: contentful, est: est, group: group);
        var controller = new TestController()
        {
            ControllerContext = new ControllerContext
            {
                RouteData = new RouteData(),
                HttpContext = httpContext,
            },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        est.GetEstablishmentLinks(100).Returns(new List<SqlEstablishmentLinkDto>());

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.RouteToSelectSchoolsToAssessViewModelAsync(controller, sectionSlug)
        );

        Assert.Equal($"Could not find linked establishments for group ID: 100", ex.Message);
    }

    // --- SubmitSelectedSchoolsToAssessAndRedirect ----------------------------
    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_ThrowsWhenNoSectionForSlug()
    {
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var session = Substitute.For<ISession>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var categorySlug = "category-4";
        var sectionSlug = "section-4";

        var sut = CreateServiceUnderTest(contentful: contentful, est: est, group: group);
        var controller = new TestController()
        {
            ControllerContext = new ControllerContext
            {
                RouteData = new RouteData(),
                HttpContext = httpContext,
            },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        var establishmentLinks = new List<SqlEstablishmentLinkDto>()
        {
            new SqlEstablishmentLinkDto() { Id = 1 },
            new SqlEstablishmentLinkDto() { Id = 2 },
        };

        est.GetEstablishmentLinks(100).Returns(establishmentLinks);

        contentful.GetSectionBySlugAsync(sectionSlug).Returns((QuestionnaireSectionEntry?)null!);

        //Act/Assert
        var ex = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            sut.RouteToSelectSchoolsToAssessViewModelAsync(controller, sectionSlug)
        );

        Assert.Equal($"Could not find topic for slug '{sectionSlug}'", ex.Message);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_ThrowsWhenSectionSlugNull()
    {
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();

        var categorySlug = "category-4";
        var viewModel = new GroupsSelectSchoolsToAssessViewModel()
        {
            Section = new QuestionnaireSectionEntry(),
            SchoolSubmissionInfo = new List<SubmissionInformationModel>(),
            SelectedSchoolsRefs = ["00001", "00002"],
        };

        var sut = CreateServiceUnderTest(contentful: contentful, est: est, group: group);
        var controller = new TestController()
        {
            ControllerContext = new ControllerContext { RouteData = new RouteData() },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        //Act/Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.SubmitSelectedSchoolsToAssessAndRedirect(controller, null!, viewModel)
        );
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_CallsSubmissionService()
    {
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var submission = Substitute.For<ISubmissionService>();
        var session = Substitute.For<ISession>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var categorySlug = "category-5";
        var sectionSlug = "section-5";
        var section = new QuestionnaireSectionEntry()
        {
            Sys = new SystemDetails { Id = "sec-5" },
            InterstitialPage = new PageEntry { Slug = sectionSlug },
            Questions = new List<QuestionnaireQuestionEntry>() { new() { Slug = "a-question" } },
        };
        var selectedRefs = new string[] { "00001", "00002", "00003" };
        var schoolSubmissions = new List<SubmissionInformationModel>()
        {
            new()
            {
                EstablishmentId = 1,
                EstablishmentRef = "00001",
                SectionId = section.Id,
                Status = SubmissionStatus.NotStarted,
            },
            new()
            {
                EstablishmentId = 2,
                EstablishmentRef = "00002",
                SectionId = section.Id,
                Status = SubmissionStatus.InProgress,
            },
            new()
            {
                EstablishmentId = 3,
                EstablishmentRef = "00003",
                SectionId = section.Id,
                Status = SubmissionStatus.NotStarted,
            },
        };

        var viewModel = new GroupsSelectSchoolsToAssessViewModel()
        {
            Section = section,
            SchoolSubmissionInfo = schoolSubmissions,
            SelectedSchoolsRefs = selectedRefs.ToList(),
        };

        var establishmentLinks = new List<SqlEstablishmentLinkDto>
        {
            new() { Urn = "00001", Id = 1 },
            new() { Urn = "00002", Id = 2 },
            new() { Urn = "00003", Id = 3 },
        };

        var school1 = new SqlEstablishmentDto
        {
            EstablishmentRef = "00001",
            OrgName = "School 1",
            Id = 1,
        };
        var school2 = new SqlEstablishmentDto
        {
            EstablishmentRef = "00002",
            OrgName = "School 2",
            Id = 2,
        };
        var school3 = new SqlEstablishmentDto
        {
            EstablishmentRef = "00003",
            OrgName = "School 3",
            Id = 3,
        };

        est.GetEstablishmentByReferenceAsync("00001").Returns(school1);
        est.GetEstablishmentByReferenceAsync("00002").Returns(school2);
        est.GetEstablishmentByReferenceAsync("00003").Returns(school3);

        est.GetEstablishmentLinks(100).Returns(establishmentLinks);

        var sut = CreateServiceUnderTest(
            contentful: contentful,
            est: est,
            group: group,
            submission: submission
        );
        var controller = new TestController()
        {
            ControllerContext = new ControllerContext
            {
                RouteData = new RouteData(),
                HttpContext = httpContext,
            },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        group
            .GetGroupSubmissionInformationForSection(
                Arg.Any<List<SqlEstablishmentLinkDto>>(),
                Arg.Any<string>()
            )
            .Returns(schoolSubmissions);

        await sut.SubmitSelectedSchoolsToAssessAndRedirect(controller, sectionSlug, viewModel);

        await submission
            .Received(3)
            .GetLatestSubmissionResponsesModel(Arg.Any<int>(), section, (SubmissionStatus?)null);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_CallsEstablishmentService()
    {
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var submission = Substitute.For<ISubmissionService>();
        var session = Substitute.For<ISession>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var categorySlug = "category-5";
        var sectionSlug = "section-5";
        var section = new QuestionnaireSectionEntry()
        {
            Sys = new SystemDetails { Id = "sec-5" },
            InterstitialPage = new PageEntry { Slug = sectionSlug },
            Questions = new List<QuestionnaireQuestionEntry>() { new() { Slug = "some-question" } },
        };
        var selectedRefs = new string[] { "00001", "00002", "00003" };
        var schoolSubmissions = new List<SubmissionInformationModel>()
        {
            new()
            {
                EstablishmentId = 1,
                EstablishmentRef = "00001",
                SectionId = section.Id,
                Status = SubmissionStatus.NotStarted,
            },
            new()
            {
                EstablishmentId = 2,
                EstablishmentRef = "00002",
                SectionId = section.Id,
                Status = SubmissionStatus.InProgress,
            },
            new()
            {
                EstablishmentId = 3,
                EstablishmentRef = "00003",
                SectionId = section.Id,
                Status = SubmissionStatus.NotStarted,
            },
        };

        var viewModel = new GroupsSelectSchoolsToAssessViewModel()
        {
            Section = section,
            SchoolSubmissionInfo = schoolSubmissions,
            SelectedSchoolsRefs = selectedRefs.ToList(),
        };

        var sut = CreateServiceUnderTest(
            contentful: contentful,
            est: est,
            group: group,
            submission: submission
        );
        var controller = new TestController()
        {
            ControllerContext = new ControllerContext
            {
                RouteData = new RouteData(),
                HttpContext = httpContext,
            },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        est.GetEstablishmentLinks(100)
            .Returns(
                new List<SqlEstablishmentLinkDto>
                {
                    new() { Urn = "00001", Id = 1 },
                    new() { Urn = "00002", Id = 2 },
                    new() { Urn = "00003", Id = 3 },
                    new() { Urn = "00004", Id = 4 },
                }
            );

        group
            .GetGroupSubmissionInformationForSection(
                Arg.Any<List<SqlEstablishmentLinkDto>>(),
                Arg.Any<string>()
            )
            .Returns(schoolSubmissions);

        await sut.SubmitSelectedSchoolsToAssessAndRedirect(controller, sectionSlug, viewModel);

        await est.Received(1).GetEstablishmentLinks(100);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_SetsServerSessionForMultiSchoolSelection()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var session = Substitute.For<ISession>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var categorySlug = "category-5";
        var sectionSlug = "section-5";
        var section = new QuestionnaireSectionEntry()
        {
            Sys = new SystemDetails { Id = "sec-5" },
            InterstitialPage = new PageEntry { Slug = sectionSlug },
            Questions = new List<QuestionnaireQuestionEntry> { new() { Slug = "question" } },
        };

        var selectedRefs = new string[] { "00001", "00002", "00003" };

        var schoolSubmissions = new List<SubmissionInformationModel>()
        {
            new()
            {
                EstablishmentId = 1,
                EstablishmentRef = "00001",
                SectionId = section.Id,
                Status = SubmissionStatus.NotStarted,
            },
            new()
            {
                EstablishmentId = 2,
                EstablishmentRef = "00002",
                SectionId = section.Id,
                Status = SubmissionStatus.InProgress,
            },
            new()
            {
                EstablishmentId = 3,
                EstablishmentRef = "00003",
                SectionId = section.Id,
                Status = SubmissionStatus.NotStarted,
            },
        };
        var establishmentLinks = new List<SqlEstablishmentLinkDto>()
        {
            new SqlEstablishmentLinkDto() { Id = 1, Urn = "00001" },
            new SqlEstablishmentLinkDto() { Id = 2, Urn = "00002" },
            new SqlEstablishmentLinkDto() { Id = 3, Urn = "00003" },
        };

        est.GetEstablishmentLinks(100).Returns(establishmentLinks);

        var establishment1 = new SqlEstablishmentDto
        {
            EstablishmentRef = "00001",
            OrgName = "School 1",
            Id = 1,
        };
        var establishment2 = new SqlEstablishmentDto
        {
            EstablishmentRef = "00002",
            OrgName = "School 2",
            Id = 2,
        };
        var establishment3 = new SqlEstablishmentDto
        {
            EstablishmentRef = "00003",
            OrgName = "School 3",
            Id = 3,
        };

        est.GetEstablishmentByReferenceAsync("00001").Returns(establishment1);
        est.GetEstablishmentByReferenceAsync("00002").Returns(establishment2);
        est.GetEstablishmentByReferenceAsync("00003").Returns(establishment3);

        var viewModel = new GroupsSelectSchoolsToAssessViewModel()
        {
            Section = section,
            SchoolSubmissionInfo = schoolSubmissions,
            SelectedSchoolsRefs = selectedRefs.ToList(),
        };

        var sut = CreateServiceUnderTest(contentful: contentful, est: est, group: group);
        var controller = new TestController()
        {
            ControllerContext = new ControllerContext
            {
                RouteData = new RouteData(),
                HttpContext = httpContext,
            },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        group
            .GetGroupSubmissionInformationForSection(
                Arg.Any<List<SqlEstablishmentLinkDto>>(),
                Arg.Any<string>()
            )
            .Returns(schoolSubmissions);

        byte[]? storedBytes = null;

        session
            .When(x => x.Set(SessionConstants.SelectedEstablishmentsKey, Arg.Any<byte[]>()))
            .Do(call =>
            {
                storedBytes = call.ArgAt<byte[]>(1);
            });

        // Act
        await sut.SubmitSelectedSchoolsToAssessAndRedirect(controller, sectionSlug, viewModel);

        var json = Encoding.UTF8.GetString(storedBytes!);

        // Assert
        Assert.NotNull(json);

        var ids = JsonSerializer.Deserialize<IEnumerable<int>>(json!);

        Assert.NotNull(ids);
        Assert.Equal(new[] { 1, 2, 3 }, ids);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_SetsUserContextForSingleSchoolSelection()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var session = Substitute.For<ISession>();
        var submission = Substitute.For<ISubmissionService>();
        var currentUser = Substitute.For<ICurrentUserProvider>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var categorySlug = "category-6";
        var sectionSlug = "section-6";
        var section = new QuestionnaireSectionEntry()
        {
            Sys = new SystemDetails { Id = "sec-6" },
            InterstitialPage = new PageEntry { Slug = sectionSlug },
            Questions = new List<QuestionnaireQuestionEntry> { new() { Slug = "first-question" } },
        };

        var selectedRefs = new List<string> { "00001" };

        var establishmentLinks = new List<SqlEstablishmentLinkDto>
        {
            new()
            {
                Id = 1,
                Urn = "00001",
                EstablishmentName = "School A",
            },
        };

        var establishment = new SqlEstablishmentDto()
        {
            EstablishmentRef = "00001",
            OrgName = "School A",
            Id = 1,
        };

        est.GetEstablishmentByReferenceAsync("00001").Returns(establishment);

        est.GetEstablishmentLinks(100).Returns(establishmentLinks);

        contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        submission
            .GetLatestSubmissionResponsesModel(1, section, (SubmissionStatus?)null)
            .Returns((SubmissionResponsesModel?)null);

        var viewModel = new GroupsSelectSchoolsToAssessViewModel()
        {
            Section = section,
            SchoolSubmissionInfo = null!,
            SelectedSchoolsRefs = selectedRefs.ToList(),
        };

        var sut = CreateServiceUnderTest(
            contentful: contentful,
            est: est,
            group: group,
            submission: submission,
            currentUser: currentUser
        );
        var controller = new TestController()
        {
            ControllerContext = new ControllerContext
            {
                RouteData = new RouteData(),
                HttpContext = httpContext,
            },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        // Act
        await sut.SubmitSelectedSchoolsToAssessAndRedirect(controller, sectionSlug, viewModel);

        // Assert
        currentUser.Received(1).SetGroupSelectedSchool("00001", "School A");
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenSingleSchoolSelected_GetsLatestSubmission()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var submission = Substitute.For<ISubmissionService>();

        var sectionSlug = "section-7";
        var categorySlug = "category-7";

        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails { Id = "sec-7" },
            Questions = new List<QuestionnaireQuestionEntry> { new() { Slug = "first-question" } },
        };

        var schoolSubmissions = new List<SubmissionInformationModel>
        {
            new()
            {
                EstablishmentId = 1,
                EstablishmentRef = "00001",
                SectionId = section.Id,
                Status = SubmissionStatus.NotStarted,
            },
        };

        var viewModel = new GroupsSelectSchoolsToAssessViewModel
        {
            Section = section,
            SchoolSubmissionInfo = schoolSubmissions,
            SelectedSchoolsRefs = ["00001"],
        };

        est.GetEstablishmentLinks(100)
            .Returns(
                new List<SqlEstablishmentLinkDto>
                {
                    new()
                    {
                        Id = 1,
                        Urn = "00001",
                        EstablishmentName = "School A",
                    },
                }
            );

        est.GetEstablishmentByReferenceAsync("00001")
            .Returns(
                new SqlEstablishmentDto
                {
                    EstablishmentRef = "00001",
                    Id = 1,
                    OrgName = "School A",
                }
            );

        contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        var sut = CreateServiceUnderTest(contentful: contentful, est: est, submission: submission);

        var controller = new TestController
        {
            ControllerContext = new ControllerContext { RouteData = new RouteData() },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        // Act
        await sut.SubmitSelectedSchoolsToAssessAndRedirect(controller, sectionSlug, viewModel);

        // Assert
        await submission
            .Received(1)
            .GetLatestSubmissionResponsesModel(1, section, (SubmissionStatus?)null);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenMultiSelected_SetsInProgressToInaccessible()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var submission = Substitute.For<ISubmissionService>();
        var session = Substitute.For<ISession>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var sectionSlug = "section-8";
        var categorySlug = "category-8";

        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails { Id = "sec-8" },
            Questions = new List<QuestionnaireQuestionEntry> { new() { Slug = "first-question" } },
        };

        contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        var schoolSubmissions = new List<SubmissionInformationModel>
        {
            new()
            {
                EstablishmentId = 1,
                EstablishmentRef = "00001",
                Status = SubmissionStatus.NotStarted,
            },
            new()
            {
                SubmissionId = 200,
                EstablishmentId = 2,
                EstablishmentRef = "00002",
                Status = SubmissionStatus.InProgress,
            },
        };

        var viewModel = new GroupsSelectSchoolsToAssessViewModel
        {
            Section = section,
            SchoolSubmissionInfo = schoolSubmissions,
            SelectedSchoolsRefs = ["00001", "00002"],
        };

        est.GetEstablishmentLinks(100)
            .Returns(
                new List<SqlEstablishmentLinkDto>
                {
                    new() { Urn = "00001", EstablishmentName = "Test 1" },
                    new() { Urn = "00002", EstablishmentName = "Test 2" },
                }
            );

        est.GetEstablishmentByReferenceAsync("00001")
            .Returns(
                new SqlEstablishmentDto
                {
                    EstablishmentRef = "00001",
                    Id = 1,
                    OrgName = "Test 1",
                }
            );
        est.GetEstablishmentByReferenceAsync("00002")
            .Returns(
                new SqlEstablishmentDto
                {
                    EstablishmentRef = "00002",
                    Id = 2,
                    OrgName = "Test 2",
                }
            );

        var latestSubmission = new SubmissionResponsesModel(
            200,
            new List<QuestionWithAnswerModel>()
        )
        {
            Status = SubmissionStatus.InProgress,
        };

        submission
            .GetLatestSubmissionResponsesModel(2, section, (SubmissionStatus?)null)
            .Returns(latestSubmission);

        var sut = CreateServiceUnderTest(
            contentful: contentful,
            est: est,
            group: group,
            submission: submission
        );

        var controller = new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
            },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        // Act
        await sut.SubmitSelectedSchoolsToAssessAndRedirect(controller, sectionSlug, viewModel);

        // Assert
        await submission.Received(1).SetSubmissionInaccessibleAsync(2, section.Id);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenSubmissionNotStarted_DoesNotSetInaccessible()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var submission = Substitute.For<ISubmissionService>();
        var session = Substitute.For<ISession>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var sectionSlug = "section-5";
        var categorySlug = "category-5";

        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails { Id = "sec-5" },
            Questions = new List<QuestionnaireQuestionEntry> { new() { Slug = "first-question" } },
        };

        var schoolSubmissions = new List<SubmissionInformationModel>
        {
            new() { EstablishmentId = 1, EstablishmentRef = "00001" },
            new() { EstablishmentId = 2, EstablishmentRef = "00002" },
        };

        var viewModel = new GroupsSelectSchoolsToAssessViewModel
        {
            Section = section,
            SchoolSubmissionInfo = schoolSubmissions,
            SelectedSchoolsRefs = ["00001", "00002"],
        };

        est.GetEstablishmentLinks(100)
            .Returns(
                new List<SqlEstablishmentLinkDto>
                {
                    new() { Id = 1, Urn = "00001" },
                    new() { Id = 2, Urn = "00002" },
                }
            );

        contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        submission
            .GetLatestSubmissionResponsesModel(Arg.Any<int>(), section, (SubmissionStatus?)null)
            .Returns((SubmissionResponsesModel?)null);

        var sut = CreateServiceUnderTest(
            contentful: contentful,
            est: est,
            group: group,
            submission: submission
        );

        var controller = new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
            },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        // Act
        await sut.SubmitSelectedSchoolsToAssessAndRedirect(controller, sectionSlug, viewModel);

        // Assert
        await submission
            .DidNotReceive()
            .SetSubmissionInaccessibleAsync(Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task RouteToViewInProgressAnswers_WhenSchoolNotFound_RedirectsHome()
    {
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();

        est.GetEstablishmentByReferenceAsync("900006").Returns((SqlEstablishmentDto?)null);

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
        var group = Substitute.For<IGroupService>();

        est.GetEstablishmentByReferenceAsync("900006")
            .Returns(
                new SqlEstablishmentDto
                {
                    Id = 100,
                    OrgName = "Test School",
                    EstablishmentRef = "900006",
                }
            );

        var section = new QuestionnaireSectionEntry
        {
            Name = "Cyber security processes",
            Questions = [],
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
            group: group,
            submission: submissionService
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
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenSingleSchoolNotInGroup_Throws()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var group = Substitute.For<IGroupService>();
        var submissionService = Substitute.For<ISubmissionService>();
        var session = Substitute.For<ISession>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var sectionSlug = "section-6";
        var categorySlug = "category-6";

        var section = new QuestionnaireSectionEntry { Sys = new SystemDetails { Id = "sec-5" } };

        var schoolSubmissions = new List<SubmissionInformationModel>
        {
            new() { EstablishmentId = 1, EstablishmentRef = "99999" },
        };

        var viewModel = new GroupsSelectSchoolsToAssessViewModel
        {
            Section = section,
            SchoolSubmissionInfo = schoolSubmissions,
            SelectedSchoolsRefs = ["99999"],
        };

        est.GetEstablishmentLinks(100)
            .Returns(
                new List<SqlEstablishmentLinkDto>
                {
                    new()
                    {
                        Id = 1,
                        Urn = "00001",
                        EstablishmentName = "School A",
                    },
                }
            );

        contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        var sut = CreateServiceUnderTest(
            contentful: contentful,
            est: est,
            group: group,
            submission: submissionService
        );

        var controller = new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
            },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        // Act / Assert
        var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.SubmitSelectedSchoolsToAssessAndRedirect(controller, sectionSlug, viewModel)
        );

        Assert.Equal("Selected school with ref 99999 not linked to user's group", ex.Message);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenAllSelected_SavesAllDisplayedSchoolIdsToSession()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var submissionService = Substitute.For<ISubmissionService>();
        var group = Substitute.For<IGroupService>();
        var session = Substitute.For<ISession>();

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;

        var sectionSlug = "section-9";
        var categorySlug = "category-9";

        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails { Id = "sec-9" },
            Questions = new List<QuestionnaireQuestionEntry>() { new() { Slug = "question" } },
        };

        var schoolSubmissions = new List<SubmissionInformationModel>
        {
            new() { EstablishmentRef = "00001", EstablishmentId = 1 },
            new() { EstablishmentRef = "00002", EstablishmentId = 2 },
            new() { EstablishmentRef = "00003", EstablishmentId = 3 },
        };

        var establishmentLinks = new List<SqlEstablishmentLinkDto>
        {
            new() { Id = 1, Urn = "00001" },
            new() { Id = 2, Urn = "00002" },
            new() { Id = 3, Urn = "00003" },
        };

        est.GetEstablishmentLinks(100).Returns(establishmentLinks);

        var establishment1 = new SqlEstablishmentDto
        {
            EstablishmentRef = "00001",
            OrgName = "School 1",
            Id = 1,
        };
        var establishment2 = new SqlEstablishmentDto
        {
            EstablishmentRef = "00002",
            OrgName = "School 2",
            Id = 2,
        };
        var establishment3 = new SqlEstablishmentDto
        {
            EstablishmentRef = "00003",
            OrgName = "School 3",
            Id = 3,
        };

        est.GetEstablishmentByReferenceAsync("00001").Returns(establishment1);
        est.GetEstablishmentByReferenceAsync("00002").Returns(establishment2);
        est.GetEstablishmentByReferenceAsync("00003").Returns(establishment3);

        contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        var viewModel = new GroupsSelectSchoolsToAssessViewModel
        {
            Section = section,
            SchoolSubmissionInfo = schoolSubmissions,
            SelectedSchoolsRefs = ["all"],
            PresentedSchoolRefs = ["00001", "00002", "00003"],
        };

        byte[]? storedBytes = null;

        session
            .When(x => x.Set(SessionConstants.SelectedEstablishmentsKey, Arg.Any<byte[]>()))
            .Do(call =>
            {
                storedBytes = call.ArgAt<byte[]>(1);
            });

        var sut = CreateServiceUnderTest(
            contentful: contentful,
            est: est,
            group: group,
            submission: submissionService
        );

        var controller = new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
            },
        };
        controller.RouteData.Values["categorySlug"] = categorySlug;

        // Act
        await sut.SubmitSelectedSchoolsToAssessAndRedirect(controller, sectionSlug, viewModel);

        var json = Encoding.UTF8.GetString(storedBytes!);

        // Assert
        Assert.NotNull(json);

        var ids = JsonSerializer.Deserialize<IEnumerable<int>>(json!);

        Assert.NotNull(ids);
        Assert.Equal(new[] { 1, 2, 3 }, ids);
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

        est.GetEstablishmentLinks(100).Returns(new List<SqlEstablishmentLinkDto>());

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
