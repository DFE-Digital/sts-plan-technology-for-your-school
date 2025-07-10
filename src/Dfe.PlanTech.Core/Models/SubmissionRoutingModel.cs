using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.Models;

public class SubmissionRoutingModel
{
    public SubmissionStatus Status { get; set; }
    public CmsQuestionDto? NextQuestion { get; set; }
    public CmsQuestionnaireSectionDto Section { get; }
    public SqlSubmissionDto? SectionStatus { get; }
}
