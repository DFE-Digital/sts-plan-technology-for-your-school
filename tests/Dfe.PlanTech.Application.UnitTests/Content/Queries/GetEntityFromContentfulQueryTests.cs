using Dfe.PlanTech.Application.Caching.Interfaces;
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
    private readonly ICmsCache _cache = Substitute.For<ICmsCache>();
    private readonly GetEntityFromContentfulQuery _getEntityFromContentfulQuery;

    private readonly Question _firstQuestion = new() { Sys = new SystemDetails { Id = "question-1" } };

    public GetEntityFromContentfulQueryTests()
    {
        _getEntityFromContentfulQuery = new GetEntityFromContentfulQuery(_logger, _contentRepository, _cache);
        _cache.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<Question?>>>())
            .Returns(callInfo =>
            {
                var func = callInfo.ArgAt<Func<Task<Question?>>>(1);
                return func();
            });
    }


    [Fact]
    public async Task Should_LogError_When_Single_Entity_Contentful_Exception()
    {
        _contentRepository.GetEntityById<Question>(Arg.Any<string>(), cancellationToken: CancellationToken.None)
            .Throws(_ => new Exception("Contentful error"));

        var result = await _getEntityFromContentfulQuery.GetEntityById<Question>(_firstQuestion.Sys.Id);
        var receivedLoggerMessages = _logger.GetMatchingReceivedMessages(GetEntityFromContentfulQuery.ExceptionMessageEntityContentful, LogLevel.Error);

        Assert.Single(receivedLoggerMessages);
        Assert.Null(result);
    }
}
