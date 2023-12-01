using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class AnswerDbEntity : ContentComponentDbEntity, IAnswer<Question>
{
  public string Text { get; set; } = null!;

  public long? NextQuestionId { get; set; }

  public Question? NextQuestion { get; set; }

  public string Maturity { get; set; } = null!;

  public long? ParentQuestionId { get; set; }

  public Question? ParentQuestion { get; set; }
}
