using Dfe.PlanTech.Application.Configuration;
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
        var establishmentId = Guid.Empty;

        // sensible defaults used by multiple tests
        currentUser.EstablishmentId.Returns(999);
        currentUser.Organisation.Returns(new EstablishmentModel { Id = establishmentId, Name = "The Group" });
        currentUser.GroupSelectedSchoolUrn.Returns("URN-ABC");
        est.GetLatestSelectedGroupSchoolAsync("URN-ABC")
           .Returns(new SqlEstablishmentDto { Id = 42, OrgName = "Selected School", EstablishmentRef = "URN-ABC" });

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
               Arg.Any<IEnumerable<QuestionnaireCategoryEntry>>(), 999)
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
        Assert.Equal("The Group", vm.GroupName);
        Assert.Equal("The Group", vm.Title.Text);
        Assert.Equal("3", vm.TotalSections);       // 2 + 1
        Assert.Null(vm.ProgressRetrievalErrorMessage);
        Assert.Equal("/contact-us", vm.ContactLinkHref);

        // establishments list comes from service
        Assert.Equal(2, vm.GroupEstablishments.Count);
        await contentful.Received(1).GetPageBySlugAsync(UrlConstants.GroupsSelectionPageSlug);
        await contentful.Received(1).GetLinkByIdAsync("contact-123");
        await est.Received(1).GetEstablishmentLinksWithSubmissionStatusesAndCounts(
            Arg.Is<IEnumerable<QuestionnaireCategoryEntry>>(e => e.Count() == 2), 999);
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
        var establishmentGuid = Guid.NewGuid();

        var sut = CreateServiceUnderTest(est: est, currentUser: currentUser);
        currentUser.EstablishmentId.Returns(1000);
        currentUser.Organisation.Returns(new EstablishmentModel { Id = establishmentGuid, Name = "Group X" });
        currentUser.DsiReference.Returns("dsi-123"); // if your BaseViewBuilder wraps this, still fine to use

        await sut.RecordGroupSelectionAsync("URN-007", "Bond Primary");

        await est.Received(1).RecordGroupSelection(
            "dsi-123",
            1000,
            Arg.Is<EstablishmentModel>(m => m.Id == establishmentGuid && m.Name == "Group X"),
            "URN-007",
            "Bond Primary");
    }
}
