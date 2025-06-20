using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Enums;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class SubtopicRecommendationEntry : ContentfulEntry
    {
        public List<RecommendationIntroEntry> Intros { get; init; } = [];

        public RecommendationSectionEntry Section { get; init; } = null!;

        public SectionEntry Subtopic { get; init; } = null!;

        public RecommendationIntroEntry? GetRecommendationByMaturity(string maturity)
        => Intros.FirstOrDefault(intro => intro.Maturity == maturity);

        public RecommendationIntroEntry? GetRecommendationByMaturity(Maturity maturity)
        => Intros.FirstOrDefault(intro => intro.Maturity == maturity.ToString());
    }
}
