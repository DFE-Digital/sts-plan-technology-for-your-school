using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class AnswerDbEntity : ContentComponentDbEntity, IAnswer<QuestionDbEntity>, IComparableDbEntity<AnswerDbEntity, string>
{
    public string Text { get; set; } = null!;

    public string? NextQuestionId { get; set; }

    public QuestionDbEntity? NextQuestion { get; set; }

    public string Maturity { get; set; } = null!;

    [DontCopyValue]
    public string? ParentQuestionId { get; set; }

    public QuestionDbEntity? ParentQuestion { get; set; }

    public List<RecommendationChunkDbEntity> RecommendationChunks { get; set; } = [];
    public List<RecommendationSectionDbEntity> RecommendationSections { get; set; } = [];

    public bool Matches(AnswerDbEntity other) => base.Matches(other);
}
