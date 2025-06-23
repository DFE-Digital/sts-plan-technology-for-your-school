using Dfe.PlanTech.Core.DataTransferObjects;

namespace Dfe.PlanTech.Core.Models
{
    public class SubmissionResponsesModel
    {
        public List<QuestionWithAnswerModel> Responses { get; init; } = [];

        public int SubmissionId { get; init; }

        public bool HasResponses => Responses != null && Responses.Count > 0;

        public SubmissionResponsesModel(SqlSubmissionDto submission)
        {
            SubmissionId = submission.Id;
            Responses = submission.Responses
                .Select(response => new QuestionWithAnswerModel(response))
                .GroupBy(questionWithAnswer => questionWithAnswer.QuestionSysId)
                .Select(group => group.OrderByDescending(questionWithAnswer => questionWithAnswer.DateCreated).First())
                .ToList();
        }
    }
}
