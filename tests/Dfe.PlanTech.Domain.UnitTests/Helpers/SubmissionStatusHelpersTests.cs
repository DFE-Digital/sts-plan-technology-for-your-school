using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Domain.UnitTests;
public class SubmissionStatusHelpersTests
{
    [Theory]
    [InlineData(true, null, null, "Unable to retrieve status", "Red")]
    [InlineData(false, true, false, "Completed on", "Blue")] // previously completed
    [InlineData(false, false, null, "Not started", "Grey")] // never completed
    public void GetGroupsSubmissionStatusTag_ShouldReturnCorrectTagBasedOnStatus(
    bool retrievalError, bool? completed, bool? completedToday, string expectedMessageStart, string expectedColour)
    {
        // Arrange
        var systemTime = Substitute.For<ISystemTime>();
        var now = DateTime.UtcNow;
        systemTime.Today.Returns(now.Date);

        var dto = new SectionStatusDto
        {
            Completed = completed ?? false,
        };

        if (completedToday == null)
        {
            dto.DateUpdated = now.AddDays(-1);
        }
        else
        {
            dto.LastCompletionDate = completedToday == true ? now : now.AddDays(-1);
            dto.DateUpdated = (DateTime)dto.LastCompletionDate;
        }

        // Act
        var tag = SubmissionStatusHelpers.GetGroupsSubmissionStatusTag(retrievalError, dto, systemTime);

        // Assert
        Assert.StartsWith(expectedMessageStart, tag.Text);
        Assert.Equal(TagColour.GetMatchingColour(expectedColour), tag.Colour);
    }

    [Fact]
    public void LastEditedDate_ShouldReturnNull_WhenDateIsNull()
    {
        var result = SubmissionStatusHelpers.LastEditedDate(null, Substitute.For<ISystemTime>());
        Assert.Null(result);
    }

    [Fact]
    public void LastEditedDate_ShouldReturnShortDate_WhenDateIsPast()
    {
        var date = DateTime.UtcNow.AddDays(-1);
        var systemTime = Substitute.For<ISystemTime>();
        systemTime.Today.Returns(DateTime.UtcNow.Date);

        var result = SubmissionStatusHelpers.LastEditedDate(date, systemTime);

        Assert.StartsWith("on ", result);
    }

    [Fact]
    public async Task RetrieveSectionStatuses_ShouldPopulateStatusesAndSetCompletedCount()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var query = Substitute.For<IGetSubmissionStatusesQuery>();
        var sections = new List<Section> { new(), new() };
        var category = new Category { Sections = sections };
        var sectionStatuses = new List<SectionStatusDto>
        {
            new() { Completed = true, LastCompletionDate = new DateTime() },
            new() { Completed = false }
        };
        query.GetSectionSubmissionStatuses(sections, 123).Returns(sectionStatuses);

        // Act
        var result = await SubmissionStatusHelpers.RetrieveSectionStatuses(category, logger, query, 123);

        // Assert
        Assert.False(result.RetrievalError);
        Assert.Equal(1, result.Completed);
        Assert.Equal(sectionStatuses, result.SectionStatuses);
    }

    [Fact]
    public void GetTotalSections_ShouldReturnCorrectCount()
    {
        var categories = new List<Category>
    {
        new() { Sections = new List<Section> { new(), new() } },
        new() { Sections = new List<Section> { new() } }
    };

        var result = SubmissionStatusHelpers.GetTotalSections(categories);

        Assert.Equal("3", result);
    }
}
