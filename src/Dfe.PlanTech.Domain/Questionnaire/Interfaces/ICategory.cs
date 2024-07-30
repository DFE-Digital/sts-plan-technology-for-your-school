using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ICategory
{

}

public interface ICategory<out THeader, TSection> : ICategory
where THeader : IHeader
where TSection : ISection
{
    public THeader Header { get; }

    public List<TSection> Sections { get; }
}

public interface ICategoryComponent : ICategory<Header, Section>, IContentComponent
{
    public IList<SectionStatusDto> SectionStatuses { get; set; }

    public int Completed { get; set; }

    public bool RetrievalError { get; set; }
}
