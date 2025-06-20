using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Domain.DataTransferObjects;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Domain.Helpers;

public static class SubmissionStatusHelper
{
    public static Tag GetGroupsSubmissionStatusTag(bool retrievalError, SectionStatusDto? sectionStatus, ISystemTime systemTime)
    {

        var previouslyCompleted = sectionStatus?.LastCompletionDate != null;
        var currentCompleted = sectionStatus?.Completed == true;
        var lastEdited = LastEditedDate(sectionStatus?.DateUpdated, systemTime);
        var lastCompleted = LastEditedDate(sectionStatus?.LastCompletionDate, systemTime);

        string message;
        string colour;

        if (retrievalError)
        {
            message = "Unable to retrieve status";
            colour = "Red";
        }
        else if (currentCompleted)
        {
            message = $"Completed {lastEdited}";
            colour = "Blue";
        }
        else if (previouslyCompleted)
        {
            message = $"Completed {lastCompleted}";
            colour = "Blue";
        }
        else
        {
            message = "Not started";
            colour = "Grey";
        }

        return new Tag($"{message}", TagColourConstants.GetMatchingColour(colour));
    }

    public static string? LastEditedDate(DateTime? date, ISystemTime systemTime)
    {
        if (date == null)
            return null;
        var localTime = TimeZoneHelpers.ToUkTime(date.Value);
        return localTime.Date == systemTime.Today.Date
            ? $"at {DateTimeHelper.FormattedTime(localTime)}"
            : $"on {DateTimeHelper.FormattedDateShort(localTime)}";
    }

    public static async Task<Category> RetrieveSectionStatuses(Category category, ILogger logger, IGetSubmissionStatusesQuery query, int? schoolId = null)
    {
        try
        {
            category.SectionStatuses = await query.GetSectionSubmissionStatuses(category.Sections, schoolId);
            category.Completed = category.SectionStatuses.Count(x => x.Completed);
            category.RetrievalError = false;
            return category;
        }
        catch (Exception e)
        {
            logger.LogError(e,
                             "An exception has occurred while trying to retrieve section progress with the following message - {message}",
                             e.Message);
            category.RetrievalError = true;
            return category;
        }
    }

    public static string GetTotalSections(IEnumerable<Category> categories) => categories.Sum(category => category.Sections.Count).ToString();
}
