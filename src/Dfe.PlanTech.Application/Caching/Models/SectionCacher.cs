using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Application.Caching.Models;

public class SectionCacher : ISectionCacher
{
    public const string CACHE_KEY = "CurrentSection";
    
    private readonly ICacher _cacher;

    public SectionCacher(ICacher cacher)
    {
        _cacher = cacher;
    }
    
    public string? CurrentSection => _cacher.Get<string>(CACHE_KEY);
    
    public void SetCurrentSection(string? currentSection) => _cacher.Set(CACHE_KEY, TimeSpan.FromMinutes(10), currentSection);
    
    public Page AddCurrentSectionTitle(Page page){
        page.CurrentSectionTitle = CurrentSection;
        
        return page;
    }
}
