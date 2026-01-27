using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.UnitTests.ViewModels;

public class CategoryLandingSectionViewModelTests
{
    private static QuestionnaireSectionEntry Section(
        string? slug,
        string name = "Networking",
        string shortDesc = "Keep things safe"
    ) =>
        new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("sec-1"),
            Name = name,
            ShortDescription = shortDesc,
            InterstitialPage = slug is null
                ? new PageEntry { Sys = new SystemDetails(), Slug = string.Empty }
                : new PageEntry { Sys = new SystemDetails("p-1"), Slug = slug },
        };

    private static CategoryLandingSectionRecommendationsViewModel Recs() =>
        new CategoryLandingSectionRecommendationsViewModel
        {
            // add minimal fields if your type requires them; not used by these tests
        };

    [Fact]
    public void When_Slug_Missing_Sets_Error_And_RetrievalError_Status()
    {
        var section = Section(slug: null, name: "Devices", shortDesc: "desc");

        var vm = new CategoryLandingSectionViewModel(
            section,
            Recs(),
            sectionStatus: null,
            hadRetrievalError: false
        );

        Assert.Equal(SubmissionStatus.RetrievalError, vm.ProgressStatus);
        Assert.Equal("Devices at  unavailable", vm.ErrorMessage); // Slug is null => string interpolation shows empty after 'at '
        Assert.Null(vm.DateUpdated);
        Assert.Null(vm.LastCompletionDate);
        Assert.Equal("Devices", vm.Name);
        Assert.Equal("desc", vm.ShortDescription);
        Assert.Equal(string.Empty, vm.Slug);
    }

    [Fact]
    public void When_RetrievalError_Flag_True_Status_Is_RetrievalError_Even_If_Completed()
    {
        var section = Section(slug: "networking");
        var status = new SqlSectionStatusDto
        {
            SectionId = "sec-1",
            DateUpdated = DateTime.UtcNow.AddDays(-1),
            LastCompletionDate = DateTime.UtcNow.AddDays(-1),
        };

        var vm = new CategoryLandingSectionViewModel(
            section,
            Recs(),
            status,
            hadRetrievalError: true
        );

        Assert.Equal(SubmissionStatus.RetrievalError, vm.ProgressStatus);
        Assert.NotNull(vm.DateUpdated);
        Assert.False(string.IsNullOrWhiteSpace(vm.DateUpdated));
        Assert.NotNull(vm.LastCompletionDate);
        Assert.False(string.IsNullOrWhiteSpace(vm.LastCompletionDate));
        Assert.Equal("networking", vm.Slug);
        Assert.Null(vm.ErrorMessage);
    }

    [Fact]
    public void When_Completed_Status_Is_Completed_And_LastCompletionDate_Is_Formatted()
    {
        var section = Section(slug: "security");
        var status = new SqlSectionStatusDto
        {
            SectionId = "sec-1",
            DateUpdated = new DateTime(2024, 10, 10, 12, 0, 0, DateTimeKind.Utc),
            LastCompletionDate = new DateTime(2024, 10, 11, 12, 0, 0, DateTimeKind.Utc),
            Status = SubmissionStatus.CompleteReviewed,
        };

        var vm = new CategoryLandingSectionViewModel(
            section,
            Recs(),
            status,
            hadRetrievalError: false
        );

        Assert.Equal(SubmissionStatus.CompleteReviewed, vm.ProgressStatus);
        Assert.NotNull(vm.DateUpdated);
        Assert.False(string.IsNullOrWhiteSpace(vm.DateUpdated));
        Assert.NotNull(vm.LastCompletionDate);
        Assert.False(string.IsNullOrWhiteSpace(vm.LastCompletionDate));
        Assert.Null(vm.ErrorMessage);
    }

    [Fact]
    public void When_Started_But_Not_Completed_Status_Is_InProgress_And_LastCompletionDate_Empty_String()
    {
        var section = Section(slug: "devices");
        var status = new SqlSectionStatusDto
        {
            SectionId = "sec-1",
            DateUpdated = DateTime.UtcNow.AddDays(-2),
            LastCompletionDate = null,
            Status = SubmissionStatus.InProgress,
        };

        var vm = new CategoryLandingSectionViewModel(
            section,
            Recs(),
            status,
            hadRetrievalError: false
        );

        Assert.Equal(SubmissionStatus.InProgress, vm.ProgressStatus);
        Assert.NotNull(vm.DateUpdated);
        Assert.False(string.IsNullOrWhiteSpace(vm.DateUpdated));
        Assert.Equal(string.Empty, vm.LastCompletionDate); // explicitly empty string per code path
        Assert.Null(vm.ErrorMessage);
    }

    [Fact]
    public void When_NotStarted_Status_Is_NotStarted_And_No_Dates()
    {
        var section = Section(slug: "hosting");

        var vm = new CategoryLandingSectionViewModel(
            section,
            Recs(),
            sectionStatus: null,
            hadRetrievalError: false
        );

        Assert.Equal(SubmissionStatus.NotStarted, vm.ProgressStatus);
        Assert.Null(vm.DateUpdated);
        Assert.Null(vm.LastCompletionDate);
        Assert.Equal("hosting", vm.Slug);
        Assert.Null(vm.ErrorMessage);
    }

    [Fact]
    public void Copies_Basic_Fields_From_Section_And_Recommendations()
    {
        var section = Section(slug: "connectivity", name: "Connectivity", shortDesc: "Get online");
        var recs = Recs();

        var vm = new CategoryLandingSectionViewModel(
            section,
            recs,
            sectionStatus: null,
            hadRetrievalError: false
        );

        Assert.Equal("Connectivity", vm.Name);
        Assert.Equal("Get online", vm.ShortDescription);
        Assert.Equal("connectivity", vm.Slug);
        Assert.Same(recs, vm.Recommendations);
    }
}
