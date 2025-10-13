using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Core.Models;

public class SubmissionResponsesModel
{
    public int SubmissionId { get; init; }

    public DateTime? DateCompleted { get; init; }

    public DateTime? DateCreated { get; init; }

    public DateTime? DateLastUpdated { get; init; }

    public SqlEstablishmentDto Establishment { get; init; }

    public string? Maturity { get; set; }

    public List<QuestionWithAnswerModel> Responses { get; set; }

    public bool HasResponses => Responses is not null && Responses.Count != 0;

    public SubmissionResponsesModel(int submissionId, List<QuestionWithAnswerModel> responses)
    {
        SubmissionId = submissionId;
        Responses = responses;
    }

    public SubmissionResponsesModel(SqlSubmissionDto submission, QuestionnaireSectionEntry section)
    {
        SubmissionId = submission.Id;
        DateCompleted = submission.DateCompleted;
        DateCreated = submission.DateCreated;
        DateLastUpdated = submission.DateLastUpdated;
        Maturity = submission.Maturity;
        Establishment = submission.Establishment;
        Responses = submission.Responses
            .Select(response => new QuestionWithAnswerModel(response, section))
            .GroupBy(questionWithAnswer => questionWithAnswer.QuestionSysId)
            .Select(group => group.OrderByDescending(questionWithAnswer => questionWithAnswer.DateCreated).First())
            .ToList();
    }
}

