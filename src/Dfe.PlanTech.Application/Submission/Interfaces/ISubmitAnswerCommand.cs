using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Submission.Interfaces;

public interface ISubmitAnswerCommand
{
    public Task<int> SubmitAnswer(SubmitAnswerDto submitAnswerDto, string sectionId, string sectionName);
    public Task<string?> GetNextQuestionId(string questionId, string chosenAnswerId);
    public Task<bool> NextQuestionIsAnswered(int submissionId, string nextQuestionId);

    public Task<(Domain.Questionnaire.Models.Question? Question, Domain.Submissions.Models.Submission? Submission)>
        GetQuestionWithSubmission(
            int? submissionId,
            string? questionRef,
            string sectionId,
            string? sectionName,
            CancellationToken cancellationToken);
}