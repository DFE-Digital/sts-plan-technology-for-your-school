using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ICSLink
{
    public string Url { get; }

    public string LinkText { get; }
}
