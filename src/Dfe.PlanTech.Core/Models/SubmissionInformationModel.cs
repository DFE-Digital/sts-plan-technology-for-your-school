using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Core.RoutingDataModel
{
    public class SubmissionInformationModel
    {
        public int EstablishmentId { get; set; }

        public CmsQuestionnaireSectionDto Section { get; set; } = null!;

        public SubmissionResponsesModel? SubmissionResponses { get; set; } = null!;

        public QuestionWithAnswerModel OrderedResponses { get; set; } = null!;

        public SqlSectionStatusDto SectionStatus { get; set; } = null!;
    }
}
