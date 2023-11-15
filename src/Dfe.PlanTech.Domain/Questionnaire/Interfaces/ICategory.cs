using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ICategory : IContentComponent
{
    public Header Header { get; }

    public IContentComponent[] Content { get; }

    public ISection[] Sections { get; }

    public IList<SectionStatusDto> SectionStatuses { get; set; }

    public int Completed { get; set; }

    public bool RetrievalError { get; set; }

}
