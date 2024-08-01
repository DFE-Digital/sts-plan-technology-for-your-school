using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class CSLink : ContentComponent, ICSLink
{
    public string Url { get; set; } = null!;

    public string LinkText { get; set; } = null!;
}
