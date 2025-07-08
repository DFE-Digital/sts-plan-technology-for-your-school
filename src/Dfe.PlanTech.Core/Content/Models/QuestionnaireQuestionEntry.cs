using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Content.Models;

public class QuestionnaireQuestionEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public string? HelpText { get; init; }
    public IEnumerable<QuestionnaireAnswerEntry> Answers { get; init; } = [];
    public string Slug { get; set; } = null!;

    public CmsQuestionDto AsDto() => new(this);
}
