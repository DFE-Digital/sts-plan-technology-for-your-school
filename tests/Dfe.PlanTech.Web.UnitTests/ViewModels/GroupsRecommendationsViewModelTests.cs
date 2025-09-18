using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.ViewModels;

namespace Dfe.PlanTech.Web.UnitTests.ViewModels;

public class GroupsRecommendationsViewModelTests
{
    [Fact]
    public void AllContent_Includes_Intro_Chunks_And_CustomIntro()
    {
        // Arrange
        var custom = new FakeHeaderWithContent("custom");
        var chunk1 = new RecommendationChunkEntry { Sys = new SystemDetails("c1") };
        var chunk2 = new RecommendationChunkEntry { Sys = new SystemDetails("c2") };

        var vm = new GroupsRecommendationsViewModel
        {
            SelectedEstablishmentName = "School A",
            SelectedEstablishmentId = 123,
            SectionName = "Networking",
            GroupsCustomRecommendationIntro = custom,
            Chunks = new List<RecommendationChunkEntry> { chunk1, chunk2 },
            Slug = "networking",
            SubmissionResponses = new List<QuestionWithAnswerModel>()
        };

        // Act
        var all = vm.AllContent.ToList();

        // Assert: Intro, custom intro, then chunks
        Assert.Equal(4, all.Count);
        Assert.Same(custom, all[1]);
        Assert.Same(chunk1, all[2]);
        Assert.Same(chunk2, all[3]);
    }

    [Fact]
    public void AllContent_Excludes_Null_Intro_And_CustomIntro()
    {
        var chunk = new RecommendationChunkEntry { Sys = new SystemDetails("c1") };

        var vm = new GroupsRecommendationsViewModel
        {
            Chunks = new List<RecommendationChunkEntry> { chunk },
            SubmissionResponses = new List<QuestionWithAnswerModel>()
        };

        var all = vm.AllContent.ToList();

        Assert.Single(all);
        Assert.Same(chunk, all[0]);
    }

    [Fact]
    public void AllContent_Empty_When_No_Content()
    {
        var vm = new GroupsRecommendationsViewModel
        {
            Chunks = new List<RecommendationChunkEntry>(),
            SubmissionResponses = new List<QuestionWithAnswerModel>()
        };

        Assert.Empty(vm.AllContent);
    }

    // Simple fake so we can inject something implementing IHeaderWithContent
    private class FakeHeaderWithContent : IHeaderWithContent
    {
        public FakeHeaderWithContent(string header) => Header = new ComponentHeaderEntry { Text = header };
        public ComponentHeaderEntry Header { get; set; }

        public string HeaderText => throw new NotImplementedException();

        public List<ContentfulEntry> Content => throw new NotImplementedException();

        public string LinkText => throw new NotImplementedException();

        public string SlugifiedLinkText => throw new NotImplementedException();
    }
}
