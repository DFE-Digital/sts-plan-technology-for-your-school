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

    public RecommendationPage? TryGetRecommendationForMaturity(Maturity maturity) => Recommendations.FirstOrDefault(recommendation => recommendation.Maturity == maturity);

    public RecommendationPage? GetRecommendationForMaturity(string? maturity)
    {
        if (maturity is null)
            maturity = string.Empty;

        Maturity maturityResponse;

        if (!Enum.TryParse(maturity, out maturityResponse)) maturityResponse = Maturity.Unknown;

        return TryGetRecommendationForMaturity(maturityResponse);

    }
}
