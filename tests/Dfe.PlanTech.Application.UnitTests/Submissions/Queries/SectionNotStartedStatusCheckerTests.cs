using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Queries;

public class SectionNotStartedStatusCheckerTests
{
  public readonly ISubmissionStatusChecker StatusChecker = SectionNotStartedStatusChecker.SectionNotStarted;

  [Fact]
  public void Should_Match_NotStarted_Status()
  {
    var processor = Substitute.For<ISubmissionStatusProcessor>();
    processor.Section.Returns(new Section() { });
    processor.SectionStatus.Returns(new SectionStatusNew() { Status = Status.NotStarted, Completed = false });

    var matches = StatusChecker.IsMatchingSubmissionStatus(processor);

    Assert.True(matches);
  }

  [Theory]
  [InlineData(Status.InProgress)]
  [InlineData(Status.Completed)]
  public void Should_Not_Match_AnyOtherStatus(Status status)
  {
    var processor = Substitute.For<ISubmissionStatusProcessor>();
    processor.Section.Returns(new Section() { });
    processor.SectionStatus.Returns(new SectionStatusNew() { Status = status, Completed = status == Status.Completed });

    var matches = StatusChecker.IsMatchingSubmissionStatus(processor);

    Assert.False(matches);
  }
}