using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Application.Caching.Interfaces;

public interface ISectionCacher
{
    public string? CurrentSection { get; }

    public void SetCurrentSection(string? currentSection);

    public Page AddCurrentSectionTitle(Page page);
}
