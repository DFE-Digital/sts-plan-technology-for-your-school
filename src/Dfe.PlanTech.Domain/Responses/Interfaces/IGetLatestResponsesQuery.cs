using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Responses.Interfaces;

public interface IGetLatestResponsesQuery
{
    Task<QuestionWithAnswer?> GetLatestResponseForQuestion(int establishmentId, string sectionId, string questionId, CancellationToken cancellationToken = default);

    Task<CheckAnswerDto?> GetLatestResponses(int establishmentId, string sectionId, CancellationToken cancellationToken = default);
}
