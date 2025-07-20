using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Core.RoutingDataModels
{
    public class SubmissionRoutingDataModel
    {
        public string? Maturity { get; set; }
        public CmsQuestionnaireQuestionDto? NextQuestion { get; set; }
        public CmsQuestionnaireSectionDto QuestionnaireSection { get; set; } = null!;
        public SubmissionResponsesModel? Submission { get; set; }
        public SubmissionStatus Status { get; init; }
    }
}
