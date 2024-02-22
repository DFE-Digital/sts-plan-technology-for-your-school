using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetSubTopicRecommendationFromContentfulQueryTests
{
    private readonly IContentRepository _repoSubstitute = Substitute.For<IContentRepository>();
    private readonly GetSubTopicRecommendationFromContentfulQuery _getSubTopicRecommendationFromContentfulQuery;
    private readonly Section _subTopicOne;
    private readonly Section _subTopicTwo;

    private readonly SubTopicRecommendation _subTopicRecommendationOne;
    private readonly SubTopicRecommendation _subTopicRecommendationTwo;

    public GetSubTopicRecommendationFromContentfulQueryTests()
    {
        _subTopicOne = new()
        {
            Sys = new SystemDetails()
            {
                Id = "SubTopicId"
            }
        };

        _subTopicTwo = new()
        {
            Sys = new SystemDetails()
            {
                Id = "IdForAnotherSubTopic"
            }
        };

        _subTopicRecommendationOne = new()
        {
            Subtopic = _subTopicOne
        };

        _subTopicRecommendationTwo = new()
        {
            Subtopic = _subTopicTwo
        };

        _repoSubstitute.GetEntities<SubTopicRecommendation>().Returns(new List<SubTopicRecommendation>() { _subTopicRecommendationOne, _subTopicRecommendationTwo });

        _getSubTopicRecommendationFromContentfulQuery = new(_repoSubstitute);
    }

    [Fact]
    public async Task GetSubTopicRecommendation_Returns_Correct_SubTopicRecommendation_From_SectionOne()
    {
        SubTopicRecommendation? subTopicRecommendation = await _getSubTopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicOne);

        Assert.NotNull(subTopicRecommendation);
        Assert.Equal(_subTopicRecommendationOne, subTopicRecommendation);
    }

    [Fact]
    public async Task GetSubTopicRecommendation_Returns_Correct_SubTopicRecommendation_From_SectionTwo()
    {
        SubTopicRecommendation? subTopicRecommendation = await _getSubTopicRecommendationFromContentfulQuery.GetSubTopicRecommendation(_subTopicTwo);

        Assert.NotNull(subTopicRecommendation);
        Assert.Equal(_subTopicRecommendationTwo, subTopicRecommendation);
    }
}