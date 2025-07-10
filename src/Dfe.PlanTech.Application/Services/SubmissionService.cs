using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Application.Context;
using Dfe.PlanTech.Application.Workflows;

namespace Dfe.PlanTech.Application.Services;

public class SubmissionService(
    CurrentUser currentUser,
    ContentfulWorkflow contentfulWorkflow,
    ResponseWorkflow responseWorkflow,
    SubmissionWorkflow submissionWorkflow,
    UserWorkflow userWorkflow
)
{
    private readonly CurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly ContentfulWorkflow _contentfulWorkflow = contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));
    private readonly ResponseWorkflow _responseWorkflow = responseWorkflow ?? throw new ArgumentNullException(nameof(responseWorkflow));
    private readonly SubmissionWorkflow _submissionWorkflow = submissionWorkflow ?? throw new ArgumentNullException(nameof(submissionWorkflow));
    private readonly UserWorkflow _userWorkflow = userWorkflow ?? throw new ArgumentNullException(nameof(userWorkflow));

    /// <summary>
    /// Gets the current status or most recently completed status for the
    /// current user's establishment and the given section
    /// </summary>
    /// <param name="sectionSlug"></param>
    /// <param name="complete"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    /// <exception cref="ContentfulDataUnavailableException"></exception>
    public async Task<SubmissionInformationModel> GetSubmissionInformationAsync(int establishmentId, string sectionSlug, bool isCompleted)
    {
        var section = await _contentfulWorkflow.GetSectionBySlug(sectionSlug)
            ?? throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");

        var responses = await GetSubmissionResponsesForSection(establishmentId, section.Sys.Id!);
        var sectionSubmissionStatus = await _submissionWorkflow.GetSectionSubmissionStatusAsync(establishmentId, section.Sys.Id!, isCompleted);

        return new SubmissionInformationModel
        {
            EstablishmentId = establishmentId,
            Section = section,
            SubmissionResponses = responses,
            OrderedResponses = _responseWorkflow.GetOrderedResponsesForJourney(section.Sys.Id, responses),
            SectionStatus = sectionSubmissionStatus
        };
    }

    public async Task RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(int establishmentId, string sectionId)
    {
        // Check if an in-progress submission already exists
        var inProgressSubmission = await _submissionWorkflow.GetInProgressSubmissionAsync(establishmentId, sectionId);
        if (inProgressSubmission != null)
        {
            await _submissionWorkflow.SetSubmissionInaccessibleAsync(inProgressSubmission.Id);
        }

        await _submissionWorkflow.CloneLatestCompletedSubmission(establishmentId, sectionId);
    }

    public async Task<SubmissionResponsesModel?> GetSubmissionResponsesForSection(int establishmentId, string sectionId, bool completed = false, CancellationToken cancellationToken = default)
    {
        var section = _contentfulWorkflow.GetSectionBySlug()

        var submissionResponses = await _responseWorkflow.GetLatestResponsesForJourney(establishmentId, sectionId, completed);
        if (submissionResponses is null || !submissionResponses.HasResponses)
        {
            return null;
        }

        // Remove detached questions
        submissionResponses.Responses = await _responseWorkflow.GetOrderedResponsesForJourney(sectionId, submissionResponses.Responses);
        return submissionResponses;
    }

    public async Task DeleteCurrentSubmission(string sectionId)
    {
        var establishmentId = _currentUser.EstablishmentId;
        if (establishmentId is null)
        {
            throw new InvalidDataException($"User has no {nameof(establishmentId)}");
        }
        []
        await _submissionWorkflow.SetSubmissionInaccessibleAsync(establishmentId.Value, sectionId);
    }
}
