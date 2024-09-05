using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Queries;

public class SectionCompleteStatusCheckerTests
{
    public readonly ISubmissionStatusChecker StatusChecker = SectionCompleteStatusChecker.SectionComplete;

    [Fact]
    public void Should_Match_Completed_Status()
    {
        var processor = Substitute.For<ISubmissionStatusProcessor>();
        processor.Section.Returns(new Section() { });
        processor.SectionStatus.Returns(new SectionStatus() { Status = Status.Completed, Completed = true });

        var matches = StatusChecker.IsMatchingSubmissionStatus(processor);

        Assert.True(matches);
    }

    [Theory]
    [InlineData(Status.NotStarted)]
    [InlineData(Status.InProgress)]
    public void Should_Not_Match_AnyOtherStatus(Status status)
    {
        var processor = Substitute.For<ISubmissionStatusProcessor>();
        processor.Section.Returns(new Section() { });
        processor.SectionStatus.Returns(new SectionStatus() { Status = status, Completed = false });

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
        new Question(){
          Slug = "question-one"
        },
        new Question(){
          Slug = "question-two"
        }
          }
        };
        var processor = Substitute.For<ISubmissionStatusProcessor>();
        processor.Section.Returns(section);
        processor.SectionStatus.Returns(new SectionStatus() { Status = Status.Completed, Completed = true });

        await StatusChecker.ProcessSubmission(processor, default);
        Assert.Equal(SubmissionStatus.Completed, processor.Status);
        Assert.Equal(section.Questions.First(), processor.NextQuestion);
    }
}
