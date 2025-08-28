using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Core.Models
{
    public class SubmissionResponsesModel
    {
        public int SubmissionId { get; init; }

        public DateTime? DateCompleted { get; init; }

        public string? Maturity { get; set; }

        public List<QuestionWithAnswerModel> Responses { get; set; }

        public bool HasResponses => Responses is not null && Responses.Count != 0;

        public SubmissionResponsesModel(SqlSubmissionDto submission, QuestionnaireSectionEntry section)
        {
            SubmissionId = submission.Id;
            DateCompleted = submission.DateCompleted;
            Maturity = submission.Maturity;
            Responses = submission.Responses
                .Select(response => new QuestionWithAnswerModel(response, section))
                .GroupBy(questionWithAnswer => questionWithAnswer.QuestionSysId)
                .Select(group => group.OrderByDescending(questionWithAnswer => questionWithAnswer.DateCreated).First())
                .ToList();
        }
    }
}
