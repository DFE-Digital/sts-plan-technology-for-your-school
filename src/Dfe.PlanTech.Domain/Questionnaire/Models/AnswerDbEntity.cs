using Dfe.PlanTech.Domain.Database.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class AnswerDbEntity : IAnswer<Question>, IDbEntity
{
  public long Id { get; set; }

  public string ContentfulId { get; set; } = null!;

  public string Text { get; set; } = null!;

  public long? NextQuestionId { get; set; }

  public Question? NextQuestion { get; set; }

  public string Maturity { get; set; } = null!;

  public long? ParentQuestionId { get; set; }

  public Question? ParentQuestion { get; set; }
}
