using Contentful.Core.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Core.Content.Models;

public class QuestionnaireAnswerEntry : Entry<ContentComponent>
{
    public string Text { get; init; } = null!;

    public QuestionEntry? NextQuestion { get; init; }

    public string Maturity { get; init; } = null!;

    public CmsAnswerDto AsDto => new(this);
}
