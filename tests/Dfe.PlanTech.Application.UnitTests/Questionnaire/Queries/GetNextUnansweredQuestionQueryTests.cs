using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Questionnaire.Queries;

public class GetNextUnansweredQuestionQueryTests
{
  private readonly Section _section = new()
  {
    Sys = new SystemDetails()
    {
      Id = "TestSection"
    },
    Questions = new Question[]{ new()
    {
      Text = "First question",
      Sys = new SystemDetails(){
        Id = "FQ"
      },
      Answers = new(){
        new(){
          Sys = new SystemDetails(){
            Id = "FA"
          },
          Text = "First Answer"
        }
      }
    }, new(){
          Text = "Second question",
          Sys = new SystemDetails(){
            Id = "SQ",
          },
          Answers = new(){
            new(){
              Sys = new SystemDetails(){
                Id = "FA"
              },
              Text = "First Answer"
            }
          }
        },
        new(){
          Text = "Third Question",
          Sys = new SystemDetails(){
            Id = "TQ"
          },
          Answers = new(){
            new(){
              Sys = new SystemDetails(){
                Id = "FA"
              },
              Text = "First Answer"
            }
          }
        }
      }
  };

  [Fact]
  public async Task Should_Return_FirstQuestion_When_No_Responses()
  {
    CheckAnswerDto? response = null;
    var getLatestResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
    getLatestResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
    .Returns(Task.FromResult(response));

    var getNextUnansweredQuestionQuery = new GetNextUnansweredQuestionQuery(getLatestResponsesQuery);

    var result = await getNextUnansweredQuestionQuery.GetNextUnansweredQuestion(3, _section);

    Assert.Equal(result, _section.Questions[0]);
  }

  [Fact]
  public async Task Should_Throw_DatabaseException_When_No_Responses_In_OngoingSubmission()
  {
    CheckAnswerDto? response = new() { Responses = new(), SubmissionId = 1 };
    var getLatestResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
    getLatestResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
    .Returns(Task.FromResult(response ?? null));

    var getNextUnansweredQuestionQuery = new GetNextUnansweredQuestionQuery(getLatestResponsesQuery);

    await Assert.ThrowsAsync<DatabaseException>(() => getNextUnansweredQuestionQuery.GetNextUnansweredQuestion(3, _section));
  }

  [Fact]
  public async Task Should_Return_Next_Unanswered_Question()
  {
    for (var x = 0; x < 2; x++)
    {
      var answer = _section.Questions[x].Answers[0];
      _section.Questions[x].Answers[0] = new Answer()
      {
        Sys = answer.Sys,
        NextQuestion = _section.Questions[x + 1],
      };
    }

    CheckAnswerDto response = new()
    {
      SubmissionId = 1,
      Responses = _section.Questions.Select(question => new QuestionWithAnswer()
      {
        QuestionRef = question.Sys.Id,
        AnswerRef = question.Answers[0].Sys.Id
      }).Take(2)
        .ToList()
    };

    var getLatestResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
    getLatestResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
    .Returns((callinfo) => response);

    var getNextUnansweredQuestionQuery = new GetNextUnansweredQuestionQuery(getLatestResponsesQuery);

    var result = await getNextUnansweredQuestionQuery.GetNextUnansweredQuestion(3, _section);

    Assert.Equal(result, _section.Questions[2]);
  }
}