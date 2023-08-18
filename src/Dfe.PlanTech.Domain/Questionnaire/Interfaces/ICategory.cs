using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface ICategory : IContentComponent
{
    
    public IGetSubmissionStatusesQuery Query{ get; set; }
        
    public ILogger<Category> Logger { get; set; }
    
    public Header Header { get; }

    public IContentComponent[] Content { get; }

    public ISection[] Sections { get; }

    public IList<SectionStatuses> SectionStatuses { get; set; }

    public int Completed { get; set; }
    
    public bool RetrievalError { get; set; }

    public void RetrieveSectionStatuses();
}
