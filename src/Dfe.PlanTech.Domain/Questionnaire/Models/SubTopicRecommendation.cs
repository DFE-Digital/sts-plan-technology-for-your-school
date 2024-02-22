using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class SubTopicRecommendation : ContentComponent, ISubTopicRecommendation<Answer, ContentComponent, Header, RecommendationChunk, RecommendationIntro, RecommendationSection, Section>
{
    public List<RecommendationIntro> Intros { get; init; } = [];

    public RecommendationSection RecommendationSection { get; init; } = null!;

    public Section Subtopic { get; init; } = null!;
}