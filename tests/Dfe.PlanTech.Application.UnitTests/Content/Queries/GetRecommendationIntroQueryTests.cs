using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetRecommendationIntroQueryTests
{
    private readonly IGetSubTopicRecommendation _getSubTopicRecommendationSubstitute = Substitute.For<IGetSubTopicRecommendation>();
    private readonly GetRecommendationIntroQuery _getRecommendationIntroQuery;

    private readonly RecommendationIntro _recommendationIntroLowMaturity;
    private readonly RecommendationIntro _recommendationIntroMediumMaturity;
    private readonly RecommendationIntro _recommendationIntroHighMaturity;

    private readonly Section _subTopic;

    public GetRecommendationIntroQueryTests()
    {
        _recommendationIntroLowMaturity = new()
        {
            Header = new()
            {
                Text = "Low Maturity"
            },
            Maturity = "Low"
        };

        _recommendationIntroMediumMaturity = new()
        {
            Header = new()
            {
                Text = "Medium Maturity"
            },
            Maturity = "Medium"
        };

        _recommendationIntroHighMaturity = new()
        {
            Header = new()
            {
                Text = "High Maturity"
            },
            Maturity = "High"
        };

        SubTopicRecommendation subTopicRecommendation = new()
        {
            Intros = new()
            {
                _recommendationIntroLowMaturity,
                _recommendationIntroMediumMaturity,
                _recommendationIntroHighMaturity
            }
        };

        _subTopic = new()
        {
            Sys = new SystemDetails()
            {
                Id = "SubTopicId"
            }
        };

        _getSubTopicRecommendationSubstitute.GetSubTopicRecommendation(_subTopic).Returns(subTopicRecommendation);

        _getRecommendationIntroQuery = new(_getSubTopicRecommendationSubstitute);
    }

    [Fact]
    public async Task GetRecommendationIntroForSubtopic_Returns_LowMaturity_Intro()
    {
        RecommendationIntro? recommendationIntro = await _getRecommendationIntroQuery.GetRecommendationIntroForSubtopic(_subTopic, Maturity.Low);

        Assert.NotNull(recommendationIntro);
        Assert.Equal(_recommendationIntroLowMaturity, recommendationIntro);
    }

    [Fact]
    public async Task GetRecommendationIntroForSubtopic_Returns_MediumMaturity_Intro()
    {
        RecommendationIntro? recommendationIntro = await _getRecommendationIntroQuery.GetRecommendationIntroForSubtopic(_subTopic, Maturity.Medium);

        Assert.NotNull(recommendationIntro);
        Assert.Equal(_recommendationIntroMediumMaturity, recommendationIntro);
    }

    [Fact]
    public async Task GetRecommendationIntroForSubtopic_Returns_HighMaturity_Intro()
    {
        RecommendationIntro? recommendationIntro = await _getRecommendationIntroQuery.GetRecommendationIntroForSubtopic(_subTopic, Maturity.High);

        Assert.NotNull(recommendationIntro);
        Assert.Equal(_recommendationIntroHighMaturity, recommendationIntro);
    }
}