using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Core.RoutingDataModel
{
    public class SubmissionResponsesModel
    {
        public int SubmissionId { get; init; }

        public DateTime? DateCompleted { get; init; }

        public List<QuestionWithAnswerModel> Responses { get; set; } = [];

        public bool HasResponses => Responses != null && Responses.Any();

        public SubmissionResponsesModel(SqlSubmissionDto submission)
        {
            SubmissionId = submission.Id;
            DateCompleted = submission.DateCompleted;
            Responses = submission.Responses
                .Select(response => new QuestionWithAnswerModel(response))
                .GroupBy(questionWithAnswer => questionWithAnswer.QuestionSysId)
                .Select(group => group.OrderByDescending(questionWithAnswer => questionWithAnswer.DateCreated).First())
                .ToList();
        }
    }
}
