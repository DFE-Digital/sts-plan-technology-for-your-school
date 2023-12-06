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

    public async Task<CheckAnswerDto?> GetCheckAnswerDtoForSection(int establishmentId, Section section, CancellationToken cancellationToken = default)
    {
        var checkAnswerDto = await _getLatestResponseListForSubmissionQuery.GetLatestResponses(establishmentId, section.Sys.Id, cancellationToken);
        if (checkAnswerDto?.Responses == null || !checkAnswerDto.Responses.Any())
        {
            return null;
        }

        return RemoveDetachedQuestions(checkAnswerDto, section);
    }

    private static CheckAnswerDto RemoveDetachedQuestions(CheckAnswerDto checkAnswerDto, Section section)
    {
        if (checkAnswerDto == null) throw new ArgumentNullException(nameof(checkAnswerDto));
        if (section == null) throw new ArgumentNullException(nameof(section));

        var attachedQuestions = section.GetAttachedQuestions(checkAnswerDto.Responses).ToList();

        return new CheckAnswerDto()
        {
            SubmissionId = checkAnswerDto.SubmissionId,
            Responses = attachedQuestions
        };
    }
}
