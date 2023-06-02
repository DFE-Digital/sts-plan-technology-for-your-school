using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class Category : ContentComponent, ICategory
{
    public Header Header { get; set; } = null!;

    public IContentComponent[] Content { get; set; } = Array.Empty<IContentComponent>();

    public ISection[] Sections { get; set; } = Array.Empty<ISection>();
}
