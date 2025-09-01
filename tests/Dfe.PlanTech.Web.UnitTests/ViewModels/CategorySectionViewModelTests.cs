using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.UnitTests.ViewModels;

public class CategorySectionViewModelTests
{
    private static QuestionnaireSectionEntry Section(string? slug, string name = "Networking")
        => new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("sec-1"),
            Name = name,
               InterstitialPage = slug is null
                ? new PageEntry {  Sys = new SystemDetails(), Slug = string.Empty }
                : new PageEntry { Sys = new SystemDetails("p-1"), Slug = slug }
        };

    private static CategorySectionRecommendationViewModel Rec()
        => new CategorySectionRecommendationViewModel(); // minimal; contents not used by tests

    [Fact]
    public void When_Slug_Missing_Sets_Error_And_RetrievalError_Status()
    {
        var section = Section(slug: null, name: "Devices");
        var vm = new CategorySectionViewModel(section, Rec(), sectionStatus: null, hadRetrievalError: false);

        Assert.Equal(SectionProgressStatus.RetrievalError, vm.ProgressStatus);
        Assert.Equal("Slug  unavailable", vm.ErrorMessage); // Slug is null -> interpolates as blank
        Assert.Equal(string.Empty, vm.Slug);
        Assert.Equal("Devices", vm.Name);
        Assert.NotNull(vm.Recommendation);
    }

    [Fact]
    public void RetrievalError_Flag_Overrides_To_RetrievalError()
    {
        var section = Section(slug: "networking");
        var status = new SqlSectionStatusDto
        {
            Completed = true,
            LastCompletionDate = DateTime.UtcNow
        };

        var vm = new CategorySectionViewModel(section, Rec(), status, hadRetrievalError: true);

        Assert.Equal(SectionProgressStatus.RetrievalError, vm.ProgressStatus);
        Assert.Null(vm.ErrorMessage); // slug present, so no slug error message
        Assert.Equal("networking", vm.Slug);
    }

    [Fact]
    public void When_Completed_Status_Is_Completed()
    {
        var section = Section(slug: "security");
        var status = new SqlSectionStatusDto
        {
            Completed = true,
            LastCompletionDate = DateTime.UtcNow.AddDays(-3)
        };

        var vm = new CategorySectionViewModel(section, Rec(), status, hadRetrievalError: false);

        Assert.Equal(SectionProgressStatus.Completed, vm.ProgressStatus);
        Assert.Null(vm.ErrorMessage);
    }

    [Fact]
    public void Started_Never_Completed_Status_Is_StartedNeverCompleted()
    {
        var section = Section(slug: "devices");
        var status = new SqlSectionStatusDto
        {
            Completed = false,
            LastCompletionDate = null
        };

        var vm = new CategorySectionViewModel(section, Rec(), status, hadRetrievalError: false);

        Assert.Equal(SectionProgressStatus.StartedNeverCompleted, vm.ProgressStatus);
        Assert.Null(vm.ErrorMessage);
    }

    [Fact]
    public void Completed_Then_Started_New_Status_Is_CompletedStartedNew()
    {
        var section = Section(slug: "hosting");
        var status = new SqlSectionStatusDto
        {
            Completed = false,
            // Presence of a last completion date with Completed == false means: previously completed, now started again
            LastCompletionDate = DateTime.UtcNow.AddDays(-10)
        };

        var vm = new CategorySectionViewModel(section, Rec(), status, hadRetrievalError: false);

        Assert.Equal(SectionProgressStatus.CompletedStartedNew, vm.ProgressStatus);
        Assert.Null(vm.ErrorMessage);
    }

    [Fact]
    public void When_No_Status_NotStarted()
    {
        var section = Section(slug: "connectivity");

        var vm = new CategorySectionViewModel(section, Rec(), sectionStatus: null, hadRetrievalError: false);

        Assert.Equal(SectionProgressStatus.NotStarted, vm.ProgressStatus);
        Assert.Null(vm.ErrorMessage);
        Assert.Equal("connectivity", vm.Slug);
        Assert.Equal("Networking", vm.Name);
    }

    [Fact]
    public void Copies_Basic_Fields()
    {
        var section = Section(slug: "backup", name: "Backup and DR");
        var rec = Rec();

        var vm = new CategorySectionViewModel(section, rec, sectionStatus: null, hadRetrievalError: false);

        Assert.Equal("Backup and DR", vm.Name);
        Assert.Equal("backup", vm.Slug);
        Assert.Same(rec, vm.Recommendation);
    }
}
