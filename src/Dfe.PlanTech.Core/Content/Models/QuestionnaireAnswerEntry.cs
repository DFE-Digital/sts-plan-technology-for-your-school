using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Content.Models;

public class QuestionnaireAnswerEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;
    public QuestionnaireQuestionEntry? NextQuestion { get; init; }
    public string Maturity { get; init; } = null!;

    public CmsAnswerDto AsDto => new(this);
}
