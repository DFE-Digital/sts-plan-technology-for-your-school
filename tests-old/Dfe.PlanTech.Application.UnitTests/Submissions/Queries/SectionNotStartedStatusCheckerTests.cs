using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Queries;

public class SectionNotStartedStatusCheckerTests
{
    public readonly ISubmissionStatusChecker StatusChecker =
        SectionNotStartedStatusChecker.SectionNotStarted;

    [Fact]
    public void Should_Match_NotStarted_Status()
    {
        var processor = Substitute.For<ISubmissionStatusProcessor>();
        processor.Section.Returns(new Section() { });
        processor.SectionStatus.Returns(
            new SectionStatus() { Status = Status.NotStarted, Completed = false }
        );

        var matches = StatusChecker.IsMatchingSubmissionStatus(processor);

        Assert.True(matches);
    }

    [Theory]
    [InlineData(Status.InProgress)]
    [InlineData(Status.CompleteReviewed)]
    public void Should_Not_Match_AnyOtherStatus(Status status)
    {
        var processor = Substitute.For<ISubmissionStatusProcessor>();
        processor.Section.Returns(new Section() { });
        processor.SectionStatus.Returns(
            new SectionStatus() { Status = status, Completed = status == Status.CompleteReviewed }
        );

        var matches = StatusChecker.IsMatchingSubmissionStatus(processor);

        Assert.False(matches);
    }

    [Fact]
    public async Task Should_Set_SubmissionStatus_And_FirstQuestion()
    {
        var section = new Section()
        {
            Questions = new()
            {
                new Question() { Slug = "question-one" },
                new Question() { Slug = "question-two" },
            },
        };
        var processor = Substitute.For<ISubmissionStatusProcessor>();
        processor.Section.Returns(section);
        processor.SectionStatus.Returns(
            new SectionStatus() { Status = Status.NotStarted, Completed = false }
        );

        await StatusChecker.ProcessSubmission(processor, default);
        Assert.Equal(Status.NotStarted, processor.Status);
        Assert.Equal(section.Questions.First(), processor.NextQuestion);
    }
}
