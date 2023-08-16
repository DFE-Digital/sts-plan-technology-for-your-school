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

    public bool TryGetRecommendationForMaturity(Maturity maturity, out RecommendationPage? recommendationPage)
    {
        recommendationPage = Recommendations.FirstOrDefault(recommendation => recommendation.Maturity == maturity);
        return recommendationPage != null;
    }

    public RecommendationPage? GetRecommendationForMaturity(string maturity)
    {
        if (maturity is null)
            maturity = string.Empty;

        RecommendationPage? recommendationPage;
        Maturity maturityResponse;

        if (!Enum.TryParse(maturity, out maturityResponse)) maturityResponse = Maturity.Unknown;

        if (!TryGetRecommendationForMaturity(maturityResponse, out recommendationPage))
        {
            // TODO: Log Recommendation Page Not Found
        }

        return recommendationPage;

    }
}
