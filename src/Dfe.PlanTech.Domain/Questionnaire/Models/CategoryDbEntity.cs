using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class CategoryDbEntity : ContentComponentDbEntity, ICategory<HeaderDbEntity, ContentComponentDbEntity, SectionDbEntity>
{
  public HeaderDbEntity Header { get; set; } = null!;
  public List<ContentComponentDbEntity> Content { get; set; } = new();
  public List<SectionDbEntity> Sections { get; set; } = new();
}