using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class QuestionnaireQuestionEntry : TransformableEntry<QuestionnaireQuestionEntry, CmsQuestionnaireQuestionDto>
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public string? HelpText { get; init; }
    public IEnumerable<QuestionnaireAnswerEntry> Answers { get; set; } = [];
    public string Slug { get; set; } = null!;

    protected override Func<QuestionnaireQuestionEntry, CmsQuestionnaireQuestionDto> Constructor => entry => new(entry);
}
