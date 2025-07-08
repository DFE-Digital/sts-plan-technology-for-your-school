using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class QuestionEntry : ContentComponent, IQuestion<QuestionnaireAnswerEntry>
{
    /// <summary>
    /// Actual question text
    /// </summary>
    public string Text { get; init; } = null!;

    /// <summary>
    /// Optional help text
    /// </summary>
    public string? HelpText { get; init; }

    public List<QuestionnaireAnswerEntry> Answers { get; init; } = new();

    public string Slug { get; set; } = null!;

    public CmsQuestionDto AsDto() => new(this);
}
