using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

namespace Dfe.PlanTech.Web.Workflows.WorkflowModels
{
    public class SubmissionResponsesModel
    {
        public List<QuestionWithAnswerModel> Responses { get; init; } = [];

        public int SubmissionId { get; init; }

        public bool HasResponses => Responses != null && Responses.Count > 0;

        public SubmissionResponsesModel(SubmissionEntity submission)
        {
            SubmissionId = submission.Id;
            Responses = submission.Responses
                .Select(response => new QuestionWithAnswerModel(response))
                .GroupBy(questionWithAnswer => questionWithAnswer.QuestionRef)
                .Select(group => group.OrderByDescending(questionWithAnswer => questionWithAnswer.DateCreated).First())
                .ToList();
        }
    }
}
