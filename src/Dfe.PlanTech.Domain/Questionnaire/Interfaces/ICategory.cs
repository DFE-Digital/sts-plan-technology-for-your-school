using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ICategory : IContentComponent
{
    public Header Header { get; }

    public IContentComponent[] Content { get; }
    
    public ISection[] Sections { get; }
}
