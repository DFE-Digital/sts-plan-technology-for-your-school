using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Questionnaire.Queries;

public class GetNextUnansweredQuestionQueryTests{
  [Fact]
  public async Task Should_ReturnNull_When_No_Responses(){

    CheckAnswerDto? response = null;
    var getLatestResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
    getLatestResponsesQuery.GetLatestResponses(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
    .Returns(Task.FromResult(response));

    var contentRepository = 
  }
}