using Dfe.PlanTech.Application.Responses.Commands;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Commands;

public class ProcessSubmissionResponsesDtoCommandTests
{
    [Fact]
    public async Task Should_Remove_Detached_Questions()
    {
        var questionIds = new[] { "QuestionRef1", "QuestionRef2", "QuestionRef3" };
        var answerIds = new[] { "AnswerRef1", "AnswerRef2", "AnswerRef3" };
        var questionSlugs = new[] { "question-1", "question-2", "question-3" };

        SubmissionResponsesDto response = new()
        {
            SubmissionId = 1,
            Responses = [
            new()
            {
                QuestionRef = questionIds[0],
                AnswerRef = answerIds[0],
                AnswerText = "Answer 1 text",
                QuestionText = "Question 1 text",
                DateCreated = DateTime.Now,
            },
                new()
                {
                    QuestionRef = questionIds[1],
                    AnswerRef = answerIds[1],
                    AnswerText = "Answer 2 text",
                    QuestionText = "Question 2 text",
                    DateCreated = DateTime.Now
                },
                new()
                {
                    QuestionRef = questionIds[2],
                    AnswerRef = answerIds[2],
                    AnswerText = "Answer 3 text",
                    QuestionText = "Question 3 text",
                    DateCreated = DateTime.Now
                },
            ]
        };

        List<Question> questions = new() {new()
    {
      Sys = new SystemDetails()
      {
        Id = questionIds[0]
      },
      Answers = new(){
          new(){
            Sys = new SystemDetails(){
              Id = answerIds[0]
            },
          }
        },
        Slug = questionSlugs[0]
    },new(){
      Sys = new SystemDetails()
      {
        Id = questionIds[1],
      },
        Answers = new(){
          new() {
            Sys = new SystemDetails() {
              Id = answerIds[1]
            }
          }
        },
        Slug = questionSlugs[1]
      },
    new()
    {
    Sys = new SystemDetails()
    {
      Id = questionIds[2],
    },
      Answers = new(){
        new() {
          Sys = new SystemDetails() {
            Id = answerIds[2]
          }
        }
      },
      Slug = questionSlugs[2]
    }
  };

        questions[0].Answers[0] = new Answer()
        {
            Sys = new SystemDetails()
            {
                Id = answerIds[0]
            },
            NextQuestion = questions[2]
        };

        var section = new Section()
        {
            Name = "Test section",
            Sys = new SystemDetails() { Id = "ABCD" },
            Questions = questions,
        };

        var getLatestResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
        getLatestResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                             .Returns((callinfo) => response);

        var processSubmissionResponsesDtoCommand = new ProcessSubmissionResponsesDto(getLatestResponsesQuery);

        var submissionResponsesDto = await processSubmissionResponsesDtoCommand.GetSubmissionResponsesDtoForSection(3, section);

        Assert.NotNull(submissionResponsesDto);

        Assert.Equal(submissionResponsesDto.SubmissionId, response.SubmissionId);
        Assert.Equal(2, submissionResponsesDto.Responses.Count);
    }

    [Fact]
    public async Task Should_Return_Null_When_No_Responses()
    {
        SubmissionResponsesDto? response = null;

        var getLatestResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
        getLatestResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), false, Arg.Any<CancellationToken>())
                            .Returns(Task.FromResult(response));

        var processSubmissionResponsesDtoCommand = new ProcessSubmissionResponsesDto(getLatestResponsesQuery);

        var section = new Section() { Sys = new SystemDetails() { Id = "ABCD" } };

        var submissionResponsesDto = await processSubmissionResponsesDtoCommand.GetSubmissionResponsesDtoForSection(3, section);

        Assert.Null(submissionResponsesDto);
    }
}