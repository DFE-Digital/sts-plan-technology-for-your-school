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

    public RecommendationPage[] Recommendations { get; init; } = Array.Empty<RecommendationPage>();

    public RecommendationPage? TryGetRecommendationForMaturity(Maturity maturity) => Array.Find(Recommendations, recommendation => recommendation.Maturity == maturity);

    public RecommendationPage? GetRecommendationForMaturity(string? maturity)
    {
        if (string.IsNullOrEmpty(maturity) || !Enum.TryParse(maturity, out Maturity maturityResponse)) return null;

        return TryGetRecommendationForMaturity(maturityResponse);

    }
}
