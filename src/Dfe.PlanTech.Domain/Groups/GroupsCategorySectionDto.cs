using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Groups;

public class GroupsCategorySectionDto
{
    public string? Slug { get; init; }

    public string Name { get; init; }

    public Tag Tag { get; init; }

    public string? ErrorMessage { get; init; }

    public GroupsCategorySectionDto(
        string? slug,
        string name,
        bool retrievalError,
        SectionStatusDto? sectionStatus,
        ISystemTime systemTime)
    {
        Slug = slug;
        Name = name;
        var previouslyCompleted = sectionStatus?.LastCompletionDate != null;
        var currentCompleted = sectionStatus?.Completed == true;
        var lastEdited = LastEditedDate(sectionStatus?.DateUpdated, systemTime);
        var lastCompleted = LastEditedDate(sectionStatus?.LastCompletionDate, systemTime);
        if (string.IsNullOrWhiteSpace(slug))
        {
            ErrorMessage = $"{Name} unavailable";
            Tag = new Tag();
        }
        else if (retrievalError)
            Tag = new Tag("unable to retrieve status", TagColour.Red);
        else if (currentCompleted)
            Tag = new Tag($"Completed {lastEdited}", TagColour.Blue);
        else if (previouslyCompleted)
            Tag = new Tag($"Completed {lastCompleted}", TagColour.Blue);
        else
            Tag = new Tag("not started", TagColour.Grey);
    }

    private static string? LastEditedDate(DateTime? date, ISystemTime systemTime)
    {
        if (date == null)
            return null;
        var localTime = TimeZoneHelpers.ToUkTime(date.Value);
        return localTime.Date == systemTime.Today.Date
            ? $"at {DateTimeFormatter.FormattedTime(localTime)}"
            : $"on {DateTimeFormatter.FormattedDateShort(localTime)}";
    }
}
