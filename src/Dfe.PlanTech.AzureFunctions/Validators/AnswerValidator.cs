

using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.Validators;

public class AnswerValidator
{
  private readonly CmsDbContext _db;

  public AnswerValidator(CmsDbContext db)
  {
    _db = db;
  }

  public async Task<bool> ValidateAnswer(ContentComponentDbEntity incoming, ContentComponentDbEntity? existing)
  {
    if (incoming is not AnswerDbEntity answer) return false;

    if (existing == null) return true;

    var sectionForQuestion
  }

  public bool Accepts(ContentComponentDbEntity component) => component is AnswerDbEntity;
}
