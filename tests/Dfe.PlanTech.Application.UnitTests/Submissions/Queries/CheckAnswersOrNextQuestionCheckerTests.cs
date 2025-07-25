using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Exceptions;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Queries;

public class CheckAnswersOrNextQuestionCheckerTests
{
    public readonly ISubmissionStatusChecker StatusChecker = CheckAnswersOrNextQuestionChecker.CheckAnswersOrNextQuestion;

    public static readonly List<Question> Questions = new() {
    new(){
      Sys = new SystemDetails(){ Id = "Question-One" },
      Answers = new()
    },
    new(){
      Sys = new SystemDetails(){ Id = "Question-Two" },
      Answers = new()
    },
    new(){
      Sys = new SystemDetails(){ Id = "Question-Three" },
      Answers = new()
    },
    new(){
      Sys = new SystemDetails(){ Id = "Question-Four" },
      Answers = new()
    },
    new(){
      Sys = new SystemDetails(){ Id = "Question-Five" },
      Answers = new()
    },
  };

    public readonly List<Answer> Answers = new(){
          new(){
            Sys = new SystemDetails(){
              Id = "Answer-One"
            },
            NextQuestion = Questions[1], //two
          },
          new(){
            Sys = new SystemDetails(){
              Id = "Answer-Two",
            },
            NextQuestion = Questions[3], //four
          },new(){
            Sys = new SystemDetails(){
              Id = "Answer-Three"
            },
            NextQuestion = Questions[4], //five
          },new(){
            Sys = new SystemDetails(){
              Id = "Answer-Four"
            },
          },
        };


    public SubmissionResponsesDto ResponsesForSubmissionDto = new()
    {
        Responses = [
          new QuestionWithAnswer()
          {
              QuestionRef = "Question-One",
              AnswerRef = "Answer-One"
          },
            new QuestionWithAnswer()
            {
                QuestionRef = "Question-Two",
                AnswerRef = "Answer-Two"
            },
            new QuestionWithAnswer()
            {
                QuestionRef = "Question-Three",
                AnswerRef = "Answer-Three"
            },
            new QuestionWithAnswer()
            {
                QuestionRef = "Question-Four",
                AnswerRef = "Answer-Four"
            },
            new QuestionWithAnswer()
            {
                QuestionRef = "Question-Five",
                AnswerRef = "Answer-Four"
            },
        ]
    };

    public CheckAnswersOrNextQuestionCheckerTests()
    {
        foreach (var question in Questions)
        {
            question.Answers.AddRange(Answers);
        }
    }

    [Fact]
    public void Should_Match_InProgress_Status()
    {
        var processor = Substitute.For<ISubmissionStatusProcessor>();
        processor.Section.Returns(new Section() { });
        processor.SectionStatus.Returns(new SectionStatus() { Status = Status.InProgress });

        var matches = StatusChecker.IsMatchingSubmissionStatus(processor);

        Assert.True(matches);
    }

    [Fact]
    public void Should_Not_Match_NotStarted_Status()
    {
        var processor = Substitute.For<ISubmissionStatusProcessor>();
        processor.Section.Returns(new Section() { });
        processor.SectionStatus.Returns(new SectionStatus() { Status = Status.NotStarted });

        var matches = StatusChecker.IsMatchingSubmissionStatus(processor);

        Assert.False(matches);
    }

    [Fact]
    public void Should_Not_Match_Completed_Status()
    {
        var processor = Substitute.For<ISubmissionStatusProcessor>();
        processor.Section.Returns(new Section() { });
        processor.SectionStatus.Returns(new SectionStatus() { Status = Status.CompleteReviewed });

        var matches = StatusChecker.IsMatchingSubmissionStatus(processor);

        Assert.False(matches);
    }

    [Fact]
    public async Task Should_SetStatus_To_CheckAnswers_When_No_NextQuestion()
    {
        var processor = Substitute.For<ISubmissionStatusProcessor>();

        var user = Substitute.For<IUser>();
        user.GetEstablishmentId().Returns(1);
        processor.User.Returns(user);

        var getResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
        getResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(ResponsesForSubmissionDto);

        processor.GetResponsesQuery.Returns(getResponsesQuery);

        var section = Substitute.For<ISectionComponent>();
        section.GetOrderedResponsesForJourney(Arg.Any<IEnumerable<QuestionWithAnswer>>())
                .Returns(new[] {
              ResponsesForSubmissionDto.Responses[0],ResponsesForSubmissionDto.Responses[1],ResponsesForSubmissionDto.Responses[4]
                });

        section.Sys.Returns(new SystemDetails()
        {
            Id = "section-id"
        });

        section.Questions.Returns(Questions);

        processor.Section.Returns(section);
        processor.SectionStatus.Returns(new SectionStatus() { Status = Status.InProgress });

        await StatusChecker.ProcessSubmission(processor, default);

        Assert.Equal(Status.CompleteNotReviewed, processor.Status);
    }

    [Fact]
    public async Task Should_SetStatus_To_CheckAnswers_When_NextQuestion()
    {
        var processor = Substitute.For<ISubmissionStatusProcessor>();

        var user = Substitute.For<IUser>();
        user.GetEstablishmentId().Returns(1);
        processor.User.Returns(user);

        var getResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
        getResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(ResponsesForSubmissionDto);

        processor.GetResponsesQuery.Returns(getResponsesQuery);

        var section = Substitute.For<ISectionComponent>();
        section.GetOrderedResponsesForJourney(Arg.Any<IEnumerable<QuestionWithAnswer>>())
                .Returns([ResponsesForSubmissionDto.Responses[0], ResponsesForSubmissionDto.Responses[1]]);

        section.Sys.Returns(new SystemDetails()
        {
            Id = "section-id"
        });

        section.Questions.Returns(Questions);

        processor.Section.Returns(section);
        processor.SectionStatus.Returns(new SectionStatus() { Status = Status.InProgress });

        await StatusChecker.ProcessSubmission(processor, default);

        Assert.Equal(Status.InProgress, processor.Status);
        Assert.Equal(Questions[3], processor.NextQuestion);
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Question_NotFound()
    {
        var processor = Substitute.For<ISubmissionStatusProcessor>();

        var user = Substitute.For<IUser>();
        user.GetEstablishmentId().Returns(1);
        processor.User.Returns(user);

        var getResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
        getResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                            .Returns(ResponsesForSubmissionDto);

        processor.GetResponsesQuery.Returns(getResponsesQuery);

        var section = Substitute.For<ISectionComponent>();
        section.GetOrderedResponsesForJourney(Arg.Any<IEnumerable<QuestionWithAnswer>>())
            .Returns([ResponsesForSubmissionDto.Responses[0], ResponsesForSubmissionDto.Responses[1]]);

        section.Sys.Returns(new SystemDetails()
        {
            Id = "section-id"
        });

        section.Questions.Returns(new List<Question>()
        {
            new Question()
            {
                Text = "not a question text",
                Slug = "not a slug",
                Sys = new(){ Id = "Not a question id"}
            }
        });

        processor.Section.Returns(section);
        processor.SectionStatus.Returns(new SectionStatus() { Status = Status.InProgress });

        var exception = await Assert.ThrowsAsync<UserJourneyMissingContentException>(() => StatusChecker.ProcessSubmission(processor, default));
        Assert.Equal(section, exception.Section);

        var lastResponse = section.GetOrderedResponsesForJourney(ResponsesForSubmissionDto.Responses).Last();

        var expectedErrorMessage = $"Could not find question with ID {lastResponse.QuestionRef}";

        Assert.Equal(expectedErrorMessage, exception.Message);
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Answer_NotFound()
    {
        var processor = Substitute.For<ISubmissionStatusProcessor>();

        var user = Substitute.For<IUser>();
        user.GetEstablishmentId().Returns(1);
        processor.User.Returns(user);

        var getResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
        getResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(ResponsesForSubmissionDto);

        processor.GetResponsesQuery.Returns(getResponsesQuery);

        var section = Substitute.For<ISectionComponent>();
        section.GetOrderedResponsesForJourney(Arg.Any<IEnumerable<QuestionWithAnswer>>())
            .Returns([ResponsesForSubmissionDto.Responses[0], ResponsesForSubmissionDto.Responses[1]]);

        section.Sys.Returns(new SystemDetails()
        {
            Id = "section-id"
        });

        section.Questions.Returns(new List<Question>()
        {
            new Question()
            {
                Text = "Question text",
                Slug = "Question Slug",
                Sys = new()
                {
                    Id = "Question-Two",

                },
                Answers =
                [
                    new Answer()
                    {
                        Sys = new() { Id = "Not a found answer" }
                    }
                ]
            }
        });

        processor.Section.Returns(section);
        processor.SectionStatus.Returns(new SectionStatus() { Status = Status.InProgress });

        var exception =
            await Assert.ThrowsAsync<UserJourneyMissingContentException>(() =>
                StatusChecker.ProcessSubmission(processor, default));
        Assert.Equal(section, exception.Section);

        var lastResponse = section.GetOrderedResponsesForJourney(ResponsesForSubmissionDto.Responses).Last();

        var expectedErrorMessage = $"Could not find answer with ID {lastResponse.AnswerRef} in question {lastResponse.QuestionRef}";

        Assert.Equal(expectedErrorMessage, exception.Message);
    }
}
