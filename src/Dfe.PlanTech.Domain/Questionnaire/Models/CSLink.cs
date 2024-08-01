using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class CSLink : ContentComponent, ICSLink
{
    public string Id { get; set; }
    public string Url { get; set; }

    public string LinkText { get; set; }
}