using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.UnitTests.ViewModels;

public class GroupsCategorySectionViewModelTests
{
    private static QuestionnaireSectionEntry Section(string slug, string name = "Networking")
        => new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("sec-1"),
            Name = name,
            InterstitialPage = new PageEntry { Sys = new SystemDetails("p-1"), Slug = slug }
        };

    private static CategorySectionRecommendationViewModel Rec(string? sectionSlug)
        => new CategorySectionRecommendationViewModel
        {
            SectionSlug = sectionSlug
        };

    [Fact]
    public void Copies_Basic_Fields_And_Composes_RecommendationSlug()
    {
        var section = Section("networking", "Connectivity");
        var rec = Rec("security");

        var vm = new GroupsCategorySectionViewModel(section, rec, sectionStatus: null, hadRetrievalError: false);

        Assert.Equal("Connectivity", vm.Name);
        Assert.Equal("networking", vm.Slug);

        var expected = $"{UrlConstants.GroupsSlug}/recommendations/security";
        Assert.Equal(expected, vm.RecommendationSlug);
        Assert.Same(rec, vm.Recommendation);
        Assert.Null(vm.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RecommendationSlug_Is_Empty_When_Recommendation_SectionSlug_Missing(string? recSlug)
    {
        var section = Section("devices");
        var rec = Rec(recSlug);

        var vm = new GroupsCategorySectionViewModel(section, rec, sectionStatus: null, hadRetrievalError: false);

        Assert.Equal(string.Empty, vm.RecommendationSlug);
    }

    [Fact]
    public void ErrorMessage_Set_When_Name_Missing_But_Tag_Computed_Normally()
    {
        var section = Section("devices", name: ""); // Name missing
        var rec = Rec("devices");

        var vm = new GroupsCategorySectionViewModel(section, rec, sectionStatus: null, hadRetrievalError: false);

        // Error message reflects missing name (note: interpolates null/empty after the space)
        Assert.Equal(" unavailable", vm.ErrorMessage);

        // With no sectionStatus and no retrieval error, Tag should be "Not started" (Grey)
        Assert.StartsWith("Not started", vm.Tag.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Tag_UnableToRetrieve_When_RetrievalError_Flag_True()
    {
        var section = Section("security");
        var rec = Rec("security");
        var status = new SqlSectionStatusDto
        {
            Completed = true,
            LastCompletionDate = DateTime.UtcNow.AddDays(-2)
        };

        var vm = new GroupsCategorySectionViewModel(section, rec, status, hadRetrievalError: true);

        Assert.Equal("Unable to retrieve status", vm.Tag.Text);
        // Colour should map to Red via TagColourHelper; we only assert non-empty
        Assert.False(string.IsNullOrWhiteSpace(vm.Tag.Colour?.ToString()));
    }

    [Fact]
    public void Tag_Completed_On_PreviousCompletionDate_When_PreviouslyCompleted()
    {
        var section = Section("networking");
        var rec = Rec("networking");

        // Previously completed means LastCompletionDate has value
        var status = new SqlSectionStatusDto
        {
            Completed = false,                         // current state not completed
            LastCompletionDate = DateTime.UtcNow.AddDays(-3), // previously completed
            DateUpdated = DateTime.UtcNow.AddDays(-1)
        };

        var vm = new GroupsCategorySectionViewModel(section, rec, status, hadRetrievalError: false);

        Assert.StartsWith("Completed on ", vm.Tag.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Tag_Completed_At_Time_When_CurrentlyCompleted_Today()
    {
        var section = Section("security");
        var rec = Rec("security");

        // Currently completed (Completed == true), with DateUpdated today
        var status = new SqlSectionStatusDto
        {
            Completed = true,
            LastCompletionDate = null,
            DateUpdated = DateTime.UtcNow // same date as now => "at HH:mm"
        };

        var vm = new GroupsCategorySectionViewModel(section, rec, status, hadRetrievalError: false);

        Assert.StartsWith("Completed at ", vm.Tag.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Tag_NotStarted_When_No_Status()
    {
        var section = Section("devices");
        var rec = Rec("devices");

        var vm = new GroupsCategorySectionViewModel(section, rec, sectionStatus: null, hadRetrievalError: false);

        Assert.StartsWith("Not started", vm.Tag.Text, StringComparison.OrdinalIgnoreCase);
    }
}
