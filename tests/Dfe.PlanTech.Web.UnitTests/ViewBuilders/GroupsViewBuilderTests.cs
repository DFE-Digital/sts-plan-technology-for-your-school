﻿using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
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

    private sealed class TestController : Controller { }

    private static IOptions<ContactOptionsConfiguration> Opt(string linkId = "contact-123")
        => Options.Create(new ContactOptionsConfiguration { LinkId = linkId });

    private static GroupsViewBuilder CreateServiceUnderTest(
        IOptions<ContactOptionsConfiguration>? contactOpts = null,
        IContentfulService? contentful = null,
        IEstablishmentService? est = null,
        ISubmissionService? sub = null,
        ICurrentUser? currentUser = null,
        ILogger<BaseViewBuilder>? logger = null)
    {
        contactOpts ??= Opt();
        contentful ??= Substitute.For<IContentfulService>();
        est ??= Substitute.For<IEstablishmentService>();
        sub ??= Substitute.For<ISubmissionService>();
        currentUser ??= Substitute.For<ICurrentUser>();
        logger ??= NullLogger<BaseViewBuilder>.Instance;

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
        currentUser.ActiveEstablishmentId.Returns(100);

        // No selected establishment by default - tests that need this should set it up explicitly
        // ActiveEstablishmentId, ActiveEstablishmentName, etc. not set
        // GroupSelectedSchoolUrn not set

        return new GroupsViewBuilder(logger, contactOpts, contentful, est, sub, currentUser);
    }

    private static QuestionnaireCategoryEntry MakeCategory(params QuestionnaireSectionEntry[] sections)
        => new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("cat-1"),
            Header = new ComponentHeaderEntry { Text = "Header" },
            Sections = sections.ToList()
        };

    private static QuestionnaireSectionEntry MakeSection(string id, int countAnswers = 0)
        => new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(id),
            Name = $"Sec {id}",
            Questions = Enumerable.Repeat(new QuestionnaireQuestionEntry(), countAnswers).ToList()
        };

    // --- ctor guards --------------------------------------------------------

    [Fact]
    public void Ctor_Null_ContactOptions_Throws()
    {
        var contentful = Substitute.For<IContentfulService>();
        var est = Substitute.For<IEstablishmentService>();
        var sub = Substitute.For<ISubmissionService>();
        var current = Substitute.For<ICurrentUser>();

        Assert.Throws<ArgumentNullException>(() =>
            new GroupsViewBuilder(NullLogger<BaseViewBuilder>.Instance, null!, contentful, est, sub, current));
    }

    [Fact]
    public void Ctor_Null_Services_Throw()
    {
        var opts = Opt();
        var contentful = Substitute.For<IContentfulService>();
        var current = Substitute.For<ICurrentUser>();
        var sub = Substitute.For<ISubmissionService>();
        var est = Substitute.For<IEstablishmentService>();

        Assert.Throws<ArgumentNullException>(() =>
            new GroupsViewBuilder(NullLogger<BaseViewBuilder>.Instance, opts, contentful, null!, sub, current));

        Assert.Throws<ArgumentNullException>(() =>
            new GroupsViewBuilder(NullLogger<BaseViewBuilder>.Instance, opts, contentful, est, null!, current));
    }

    // --- RouteToSelectASchoolViewModelAsync --------------------------------

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_Builds_View_With_Expected_Model()
    {
        // Arrange
        var contentful = Substitute.For<IContentfulService>();

        // selection page content (only Content list is used)
        contentful.GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug)
                  .Returns(new PageEntry { Content = new List<ContentfulEntry> { new MissingComponentEntry() } });

        // home page content: supplies categories with sections
        var cat1 = MakeCategory(MakeSection("S1"), MakeSection("S2"));
        var cat2 = MakeCategory(MakeSection("S3"));
        var homePage = new PageEntry { Content = new List<ContentfulEntry> { cat1, cat2 } };
        contentful.GetPageBySlugAsync(Arg.Any<string>())
                  .Returns(homePage);

        // contact link
        contentful.GetLinkByIdAsync("contact-123")
                  .Returns(new NavigationLinkEntry { Href = "/contact-us" });

        var est = Substitute.For<IEstablishmentService>();
        est.GetEstablishmentLinksWithSubmissionStatusesAndCounts(
               Arg.Any<IEnumerable<QuestionnaireCategoryEntry>>(), 100)
           .Returns(new List<SqlEstablishmentLinkDto>
           {
               new SqlEstablishmentLinkDto { Id = 1, EstablishmentName = "School A", CompletedSectionsCount = 2 },
               new SqlEstablishmentLinkDto { Id = 2, EstablishmentName = "School B", CompletedSectionsCount = 1 },
           });

        var sut = CreateServiceUnderTest(contentful: contentful, est: est);
        var controller = new TestController();

        // Act
        var action = await sut.RouteToSelectASchoolViewModelAsync(controller);

        // Assert
        var view = Assert.IsType<ViewResult>(action);
        Assert.Equal("GroupsSelectSchool", view.ViewName);
        Assert.Equal("Select a school", controller.ViewData["Title"]);

        var vm = Assert.IsType<GroupsSelectorViewModel>(view.Model);
        Assert.Equal("Test Academy Trust", vm.GroupName);
        Assert.Equal("Test Academy Trust", vm.Title.Text);
        Assert.Equal("3", vm.TotalSections);       // 2 + 1
        Assert.Null(vm.ProgressRetrievalErrorMessage);
        Assert.Equal("/contact-us", vm.ContactLinkHref);

        // establishments list comes from service
        Assert.Equal(2, vm.GroupEstablishments.Count);
        await contentful.Received(1).GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug);
        await contentful.Received(1).GetLinkByIdAsync("contact-123");
        await est.Received(1).GetEstablishmentLinksWithSubmissionStatusesAndCounts(
            Arg.Is<IEnumerable<QuestionnaireCategoryEntry>>(e => e.Count() == 2), 100);
    }

    [Fact]
    public async Task RouteToSelectASchoolViewModelAsync_Throws_When_No_Categories()
    {
        var contentful = Substitute.For<IContentfulService>();

        var sut = CreateServiceUnderTest(contentful: contentful);
        contentful.GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug)
                  .Returns(new PageEntry { Content = null });
        // home page with content that contains NO QuestionnaireCategoryEntry
        var homePage = new PageEntry { Content = [new MissingComponentEntry()] };
        contentful.GetPageBySlugAsync(Arg.Any<string>())
                  .Returns(homePage);

        var controller = new TestController();

        var ex = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.RouteToSelectASchoolViewModelAsync(controller));

        Assert.Contains("There are no categories to display", ex.Message);
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

        await est.Received(1).RecordGroupSelection(
            "dsi-123",
            200,
            Arg.Is<EstablishmentModel>(m => m.Id == customGroupDsiId && m.Name == "Custom Academy Trust" && m.Urn == "GRP-200"),
            "URN-007",
            "Bond Primary");
    }
}
