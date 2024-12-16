using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetContentSupportPageQueryTests
{
    private readonly IContentRepository _contentRepository = Substitute.For<IContentRepository>();
    private readonly ICmsCache _cache = Substitute.For<ICmsCache>();
    private readonly ILogger<GetContentSupportPageQuery> _logger = Substitute.For<ILogger<GetContentSupportPageQuery>>();

    private readonly string _testSlug = "test-slug";

    private readonly ContentSupportPage _contentSupportPage = new ContentSupportPage
    {
        Slug = "test-slug"
    };

    public GetContentSupportPageQueryTests()
    {
        _cache.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<ContentSupportPage>?>>>())
            .Returns(callInfo =>
            {
                var func = callInfo.ArgAt<Func<Task<IEnumerable<ContentSupportPage>?>>>(1);
                return func();
            });
    }

    [Fact]
    public async Task Should_Retrieve_Content_Support_Page_From_Cache()
    {
        _contentRepository.GetEntities<ContentSupportPage>(Arg.Any<IGetEntitiesOptions>(), CancellationToken.None)
            .Returns(new List<ContentSupportPage> { _contentSupportPage });

        var query = new GetContentSupportPageQuery(_cache, _contentRepository, _logger);

        var result = await query.GetContentSupportPage(_testSlug);

        Assert.NotNull(result);
        Assert.Equal(_contentSupportPage.Slug, result.Slug);
    }

    [Fact]
    public async Task Should_Return_Null_When_Content_Support_Page_Does_Not_Exist()
    {
        var slug = "non-existent-slug";
        _contentRepository.GetEntities<ContentSupportPage>(Arg.Any<IGetEntitiesOptions>(), CancellationToken.None)
            .Returns(new List<ContentSupportPage>());

        var query = new GetContentSupportPageQuery(_cache, _contentRepository, _logger);

        var result = await query.GetContentSupportPage(slug);

        Assert.Null(result);
    }

    [Fact]
    public async Task Content_Support_Page_Not_Found_Exception_Is_Thrown_When_There_Is_An_Issue_Retrieving_Data()
    {
        _contentRepository.GetEntities<ContentSupportPage>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Test Exception"));

        var query = new GetContentSupportPageQuery(_cache, _contentRepository, _logger);

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(async () => await query.GetContentSupportPage(_testSlug));
    }
}
