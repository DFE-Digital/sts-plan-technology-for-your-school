using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Web.Helpers;

public class CategoryHelper
{
    private readonly IGetSubmissionStatusesQuery _query;
    
    private readonly ILogger<Category> _logger;
    
    public CategoryHelper(IGetSubmissionStatusesQuery query, ILogger<Category> logger)
    {
        _query = query;
        _logger = logger;
    }
    public (IList<SectionStatuses>, bool) RetrieveSectionStatuses(ISection[] sections)
    {
        var sectionStatuses = new List<SectionStatuses>();
        var retrievalError = false;
        try
        {
            sectionStatuses = _query.GetSectionSubmissionStatuses(sections).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError("An exception has occurred while trying to retrieve section progress with the following message - {}", e.Message);
            retrievalError = true;
        }
        return (sectionStatuses, retrievalError);
    }
}