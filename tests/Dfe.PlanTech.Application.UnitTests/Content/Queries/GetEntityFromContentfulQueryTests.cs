using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetEntityFromContentfulQueryTests
{
    private readonly IContentRepository _contentRepository = Substitute.For<IContentRepository>();
    private readonly ILogger<GetEntityFromContentfulQuery> _logger = Substitute.For<ILogger<GetEntityFromContentfulQuery>>();
    private readonly GetEntityFromContentfulQuery _getEntityFromContentfulQuery;

    private readonly Question _firstQuestion = new() { Sys = new SystemDetails { Id = "question-1" } };
    private readonly Question _secondQuestion = new() { Sys = new SystemDetails { Id = "question-2" } };
    private readonly IList<Question> _contentfulQuestions;

    public GetEntityFromContentfulQueryTests()
    {
        _contentfulQuestions = [_firstQuestion, _secondQuestion];
        _getEntityFromContentfulQuery = new GetEntityFromContentfulQuery(_logger, _contentRepository);
    }

    [Theory]
    [InlineData("question-1")]
    [InlineData("question-2")]
    public async Task Should_Fetch_Entity_From_Contentful(string questionId)
    {
        _contentRepository.GetEntities<Question>(CancellationToken.None).Returns(_contentfulQuestions);

        var result = await _getEntityFromContentfulQuery.GetEntityById<Question>(questionId);

        Assert.NotNull(result);
        Assert.Equal(questionId, result.Sys.Id);
    }

    [Fact]
    public async Task Should_LogError_When_Contentful_Exception()
    {
        _contentRepository.GetEntities<Question>(CancellationToken.None).Throws(_ => new Exception("Contentful error"));

        var result = await _getEntityFromContentfulQuery.GetEntityById<Question>(_firstQuestion.Sys.Id);
        var receivedLoggerMessages = _logger.GetMatchingReceivedMessages(GetEntityFromContentfulQuery.ExceptionMessageContentful, LogLevel.Error);

        Assert.Single(receivedLoggerMessages);
        Assert.Null(result);
    }
}
