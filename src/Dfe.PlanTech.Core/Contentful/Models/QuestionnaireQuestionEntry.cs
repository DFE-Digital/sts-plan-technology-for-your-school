using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class QuestionnaireQuestionEntry : TransformableEntry<QuestionnaireQuestionEntry, CmsQuestionDto>
{
    public string Id => SystemProperties.Id;
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public string? HelpText { get; init; }
    public IEnumerable<QuestionnaireAnswerEntry> Answers { get; init; } = [];
    public string Slug { get; set; } = null!;

    public QuestionnaireQuestionEntry() : base(entry => new CmsQuestionDto(entry)) { }
}
