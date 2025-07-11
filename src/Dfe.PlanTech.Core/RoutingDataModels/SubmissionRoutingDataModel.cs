using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.RoutingDataModels
{
    public class SubmissionRoutingDataModel
    {
        public string? Maturity { get; set; }
        public CmsQuestionnaireQuestionDto? NextQuestion { get; set; }
        public CmsQuestionnaireSectionDto QuestionnaireSection { get; set; } = null!;
        public SqlSubmissionDto? Submission { get; set; }
        public SubmissionStatus Status { get; init; }
    }
}
