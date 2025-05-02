using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Helpers;

public class SubmissionStatusHelpers
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

        return new Tag($"{message}", TagColour.GetMatchingColour(colour));
    }

    public static string? LastEditedDate(DateTime? date, ISystemTime systemTime)
    {
        if (date == null)
            return null;
        var localTime = TimeZoneHelpers.ToUkTime(date.Value);
        return localTime.Date == systemTime.Today.Date
            ? $"at {DateTimeFormatter.FormattedTime(localTime)}"
            : $"on {DateTimeFormatter.FormattedDateShort(localTime)}";
    }

    public static string GetTotalSections(Page dashboardContent)
    {
        var sectionCount = 0;
        var categories = dashboardContent.Content.OfType<Category>();

        foreach (var category in categories)
        {
            sectionCount += category.Sections.Count;
        }

        return sectionCount.ToString();
    }
}
