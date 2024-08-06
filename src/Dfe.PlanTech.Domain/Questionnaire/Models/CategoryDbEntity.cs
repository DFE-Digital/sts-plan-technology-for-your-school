using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class CategoryDbEntity : ContentComponentDbEntity, ICategory<HeaderDbEntity, SectionDbEntity>
{
    public string HeaderId { get; set; } = null!;

    [DontCopyValue]
    public HeaderDbEntity Header { get; set; } = null!;

    public string InternalName { get; set; } = null!;

    [DontCopyValue]
    public List<SectionDbEntity> Sections { get; set; } = [];
}
