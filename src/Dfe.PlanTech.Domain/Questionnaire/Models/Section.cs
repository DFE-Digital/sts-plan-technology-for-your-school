using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

/// <summary>
/// A sub-section of a <see chref="Category"/>
/// </summary>
public class Section : ContentComponent, ISection
{
    public string Name { get; init; } = null!;

    public Question[] Questions { get; init; } = Array.Empty<Question>();

    public string FirstQuestionId => Questions.Select(question => question.Sys.Id).FirstOrDefault() ?? "";

    public Page InterstitialPage { get; init; } = null!;

    public new Sys Sys { get; init; } = null!;

    public RecommendationPage[] Recommendations { get; init; } = Array.Empty<RecommendationPage>();

    public RecommendationPage GetRecommendationForMaturity(Maturity maturity)
     => Recommendations.FirstOrDefault(recommendation => recommendation.Maturity == maturity) ??
        throw new KeyNotFoundException($"Could not find recommendation with maturity {maturity}");

    public RecommendationPage GetRecommendationForMaturity(string maturity)
    {
        if (maturity is null)
            maturity = string.Empty;

        if (Enum.TryParse(maturity, out Maturity maturityResponse))
        {
            return GetRecommendationForMaturity(maturityResponse);
        }
        else
        {
            return GetRecommendationForMaturity(Maturity.Unknown);
        }

    }
}
