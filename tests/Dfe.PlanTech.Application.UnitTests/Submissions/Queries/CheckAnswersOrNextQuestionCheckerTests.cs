using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interface;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Queries;

public class CheckAnswersOrNextQuestionCheckerTests
{
  public readonly ISubmissionStatusChecker StatusChecker = CheckAnswersOrNextQuestionChecker.CheckAnswersOrNextQuestion;

  public static readonly Question[] Questions = new Question[] {
  new(){
    Sys = new SystemDetails(){ Id = "Question-One" },
    Answers = new Answer[4]
  },
   new(){
    Sys = new SystemDetails(){ Id = "Question-Two" },
    Answers = new Answer[4]
  },
   new(){
    Sys = new SystemDetails(){ Id = "Question-Three" },
    Answers = new Answer[4]
  },
   new(){
    Sys = new SystemDetails(){ Id = "Question-Four" },
    Answers = new Answer[4]
  },
  new(){
    Sys = new SystemDetails(){ Id = "Question-Five" },
    Answers = new Answer[4]
  },
  };

  public readonly Answer[] Answers = new Answer[]{
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


  public CheckAnswerDto CheckAnswersDto = new()
  {
    Responses = new List<QuestionWithAnswer>(){
        new QuestionWithAnswer(){
          QuestionRef = "Question-One",
          AnswerRef = "Answer-One"
        },
        new QuestionWithAnswer(){
          QuestionRef = "Question-Two",
          AnswerRef = "Answer-Two"
        },
        new QuestionWithAnswer(){
          QuestionRef = "Question-Three",
          AnswerRef = "Answer-Three"
        },
        new QuestionWithAnswer(){
          QuestionRef = "Question-Four",
          AnswerRef = "Answer-Four"
        },
        new QuestionWithAnswer(){
          QuestionRef = "Question-Five",
          AnswerRef = "Answer-Four"
        },

      }
  };

  public CheckAnswersOrNextQuestionCheckerTests()
  {
    foreach (var question in Questions)
    {
      for (var x = 0; x < question.Answers.Length; x++)
      {
        question.Answers[x] = Answers[x];
      }
    }
  }

  [Fact]
  public void Should_Match_InProgress_Status()
  {
    var processor = Substitute.For<ISubmissionStatusProcessor>();
    processor.Section.Returns(new Section() { });
    processor.SectionStatus.Returns(new SectionStatusNew() { Status = Status.InProgress });

    var matches = StatusChecker.IsMatchingSubmissionStatus(processor);

    Assert.True(matches);
  }

  [Fact]
  public void Should_Not_Match_NotStarted_Status()
  {
    var processor = Substitute.For<ISubmissionStatusProcessor>();
    processor.Section.Returns(new Section() { });
    processor.SectionStatus.Returns(new SectionStatusNew() { Status = Status.NotStarted });

    var matches = StatusChecker.IsMatchingSubmissionStatus(processor);

    Assert.False(matches);
  }

  [Fact]
  public void Should_Not_Match_Completed_Status()
  {
    var processor = Substitute.For<ISubmissionStatusProcessor>();
    processor.Section.Returns(new Section() { });
    processor.SectionStatus.Returns(new SectionStatusNew() { Status = Status.Completed });

    var matches = StatusChecker.IsMatchingSubmissionStatus(processor);

    Assert.False(matches);
  }

  /*
        var responses = await userJourneyRouter.GetResponsesQuery.GetLatestResponses(await userJourneyRouter.User.GetEstablishmentId(),
                                                                              userJourneyRouter.Section!.Sys.Id,
                                                                              cancellationToken) ?? throw new InvalidDataException("Missing responses");

        var lastResponseInUserJourney = userJourneyRouter.Section!.GetAttachedQuestions(responses.Responses).Last();

        var lastSelectedAnswer = userJourneyRouter.Section!.Questions.First(question => question.Sys.Id == lastResponseInUserJourney.QuestionRef)
                                                                      .Answers.First(answer => answer.Sys.Id == lastResponseInUserJourney.AnswerRef);

        if (lastSelectedAnswer.NextQuestion == null)
        {
          userJourneyRouter.Status = SubmissionStatus.CheckAnswers;
          return;
        }

        userJourneyRouter.Status = SubmissionStatus.NextQuestion;
        userJourneyRouter.NextQuestion = lastSelectedAnswer.NextQuestion;
  */

  [Fact]
  public async Task Should_SetStatus_To_CheckAnswers_When_No_NextQuestion()
  {
    var processor = Substitute.For<ISubmissionStatusProcessor>();

    var user = Substitute.For<IUser>();
    user.GetEstablishmentId().Returns(1);
    processor.User.Returns(user);
    var getResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
    getResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(CheckAnswersDto);
    processor.GetResponsesQuery.Returns(getResponsesQuery);

    var section = Substitute.For<ISection>();
    section.GetAttachedQuestions(Arg.Any<IEnumerable<QuestionWithAnswer>>())
            .Returns(new[] {
              CheckAnswersDto.Responses[0],CheckAnswersDto.Responses[1],CheckAnswersDto.Responses[4]
            });

    section.Sys.Returns(new SystemDetails()
    {
      Id = "section-id"
    });

    section.Questions.Returns(Questions);

    processor.Section.Returns(section);
    processor.SectionStatus.Returns(new SectionStatusNew() { Status = Status.InProgress });

    await StatusChecker.ProcessSubmission(processor, default);

    Assert.Equal(SubmissionStatus.CheckAnswers, processor.Status);
  }

  [Fact]
  public async Task Should_SetStatus_To_CheckAnswers_When_NextQuestion()
  {
    var processor = Substitute.For<ISubmissionStatusProcessor>();

    var user = Substitute.For<IUser>();
    user.GetEstablishmentId().Returns(1);
    processor.User.Returns(user);

    var getResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
    getResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(CheckAnswersDto);
    processor.GetResponsesQuery.Returns(getResponsesQuery);

    var section = Substitute.For<ISection>();
    section.GetAttachedQuestions(Arg.Any<IEnumerable<QuestionWithAnswer>>())
            .Returns(new[] {
              CheckAnswersDto.Responses[0],CheckAnswersDto.Responses[1]
            });

    section.Sys.Returns(new SystemDetails()
    {
      Id = "section-id"
    });

    section.Questions.Returns(Questions);

    processor.Section.Returns(section);
    processor.SectionStatus.Returns(new SectionStatusNew() { Status = Status.InProgress });

    await StatusChecker.ProcessSubmission(processor, default);

    Assert.Equal(SubmissionStatus.NextQuestion, processor.Status);
    Assert.Equal(Questions[3], processor.NextQuestion);
  }

}