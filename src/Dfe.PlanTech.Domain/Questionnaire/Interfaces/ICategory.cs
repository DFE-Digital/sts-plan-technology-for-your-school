using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ICategory
{

}

public interface ICategory<THeader, TContentComponent, TSection> : ICategory
where THeader : IHeader
where TContentComponent : IContentComponentType
where TSection : ISection
{
    public THeader Header { get; }

    public List<TContentComponent> Content { get; }

    public List<TSection> Sections { get; }
}

public interface ICategoryComponent : ICategory<Header, ContentComponent, Section>, IContentComponent
{
    public IList<SectionStatusDto> SectionStatuses { get; set; }

    public int Completed { get; set; }

    public bool RetrievalError { get; set; }
}