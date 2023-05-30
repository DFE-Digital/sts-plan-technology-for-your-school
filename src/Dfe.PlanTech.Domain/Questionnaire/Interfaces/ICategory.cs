using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ICategory : IContentComponent
{
    Header Header { get; }

    IContentComponent[] Content { get; }
}
