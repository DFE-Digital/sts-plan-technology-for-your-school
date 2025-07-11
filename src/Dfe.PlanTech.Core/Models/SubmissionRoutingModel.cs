using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.RoutingDataModel;

public class SubmissionRoutingModel
{
    public SubmissionStatus Status { get; set; }
    public CmsQuestionnaireQuestionDto? NextQuestion { get; set; }
    public CmsQuestionnaireSectionDto Section { get; }
    public SqlSubmissionDto? SectionStatus { get; }
}
