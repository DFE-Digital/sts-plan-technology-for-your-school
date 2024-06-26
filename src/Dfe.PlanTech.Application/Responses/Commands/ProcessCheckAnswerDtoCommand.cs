using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interfaces;

namespace Dfe.PlanTech.Application.Responses.Commands;

public class ProcessCheckAnswerDtoCommand : IProcessCheckAnswerDtoCommand
{
    private readonly IGetLatestResponsesQuery _getLatestResponseListForSubmissionQuery;

    public ProcessCheckAnswerDtoCommand(IGetLatestResponsesQuery getLatestResponseListForSubmissionQuery)
    {
        _getLatestResponseListForSubmissionQuery = getLatestResponseListForSubmissionQuery;
    }

    public async Task<ResponsesForSubmissionDto?> GetCheckAnswerDtoForSection(int establishmentId, ISectionComponent section, CancellationToken cancellationToken = default)
    {
        var checkAnswerDto = await _getLatestResponseListForSubmissionQuery.GetLatestResponses(establishmentId, section.Sys.Id, false, cancellationToken);
        if (checkAnswerDto?.Responses == null || checkAnswerDto.Responses.Count == 0)
        {
            return null;
        }

        return RemoveDetachedQuestions(checkAnswerDto, section);
    }

    private static ResponsesForSubmissionDto RemoveDetachedQuestions(ResponsesForSubmissionDto checkAnswerDto, ISectionComponent section)
    {
        ArgumentNullException.ThrowIfNull(checkAnswerDto);
        ArgumentNullException.ThrowIfNull(section);

        var attachedQuestions = section.GetOrderedResponsesForJourney(checkAnswerDto.Responses).ToList();

        return new ResponsesForSubmissionDto()
        {
            SubmissionId = checkAnswerDto.SubmissionId,
            Responses = attachedQuestions
        };
    }
}
