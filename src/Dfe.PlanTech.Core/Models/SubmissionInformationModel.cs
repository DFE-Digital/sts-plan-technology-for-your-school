using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Core.Models;

public class SubmissionInformationModel
{
    public int EstablishmentId { get; set; }
    public QuestionnaireSectionEntry Section { get; set; } = null!;
    public SubmissionResponsesModel? SubmissionResponses { get; set; } = null!;
    public QuestionWithAnswerModel OrderedResponses { get; set; } = null!;
    public SqlSectionStatusDto SectionStatus { get; set; } = null!;
}
