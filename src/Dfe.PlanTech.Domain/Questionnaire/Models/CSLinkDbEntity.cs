using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class CSLinkDbEntity : ContentComponentDbEntity, ICSLink
{
    public string InternalName { get; set; } = null!;

    public string Url { get; set; } = null!;

    public string LinkText { get; set; } = null!;
}
