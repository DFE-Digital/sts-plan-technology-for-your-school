using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;

namespace Dfe.PlanTech.Application.Responses.Commands;

public class ProcessSubmissionResponsesDto : IProcessSubmissionResponsesCommand
{
    private readonly IGetLatestResponsesQuery _getLatestResponseListForSubmissionQuery;

    public ProcessSubmissionResponsesDto(IGetLatestResponsesQuery getLatestResponseListForSubmissionQuery)
    {
        _getLatestResponseListForSubmissionQuery = getLatestResponseListForSubmissionQuery;
    }

    public async Task<SubmissionResponsesDto?> GetSubmissionResponsesDtoForSection(int establishmentId, ISectionComponent section, bool completed = false, CancellationToken cancellationToken = default)
    {
        var submissionResponsesDto = await _getLatestResponseListForSubmissionQuery.GetLatestResponses(establishmentId, section.Sys.Id, completed, cancellationToken);
        if (submissionResponsesDto?.Responses == null || submissionResponsesDto.Responses.Count == 0)
        {
            return null;
        }

        return RemoveDetachedQuestions(submissionResponsesDto, section);
    }

    private static SubmissionResponsesDto RemoveDetachedQuestions(SubmissionResponsesDto submissionResponsesDto, ISectionComponent section)
    {
        ArgumentNullException.ThrowIfNull(submissionResponsesDto);
        ArgumentNullException.ThrowIfNull(section);

        var attachedQuestions = section.GetOrderedResponsesForJourney(submissionResponsesDto.Responses).ToList();

        return new SubmissionResponsesDto()
        {
            SubmissionId = submissionResponsesDto.SubmissionId,
            Responses = attachedQuestions
        };
    }
}
