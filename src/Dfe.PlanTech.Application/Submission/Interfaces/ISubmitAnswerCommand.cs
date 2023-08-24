using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Submission.Interfaces;

public interface ISubmitAnswerCommand
{
  public Task<int> SubmitAnswer(SubmitAnswerDto submitAnswerDto, string sectionId, string sectionName);

  public Task<string?> GetNextQuestionId(string questionId, string chosenAnswerId);

  public Task<bool> NextQuestionIsAnswered(int submissionId, string nextQuestionId);

  public Task<Question?> GetNextUnansweredQuestion(int submissionId);

  public Task<Question> GetQuestionnaireQuestion(string questionId, string? section, CancellationToken cancellationToken);

  public Task<Domain.Submissions.Models.Submission?> GetOngoingSubmission(string sectionId);
}