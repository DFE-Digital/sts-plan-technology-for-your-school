using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Caching;

public class QueryCacherTests
{
    private readonly QueryCacher _queryCacher;
    private readonly IQueryable<ContentComponent> _mockQueryable = Substitute.For<IQueryable<ContentComponent>>();
    private readonly Func<IQueryable<ContentComponent>, CancellationToken, Task<ContentComponent>> _mockQueryFunc;
    private readonly ContentComponent _mockResult = Substitute.For<ContentComponent>();

    public QueryCacherTests()
    {
        _queryCacher = new QueryCacher(new NullLogger<QueryCacher>());
        _mockQueryFunc = Substitute.For<Func<IQueryable<ContentComponent>, CancellationToken, Task<ContentComponent>>>();
        _mockQueryFunc.Invoke(_mockQueryable, Arg.Any<CancellationToken>()).Returns(Task.FromResult(_mockResult));
        _queryCacher.ClearCache();
    }

    [Fact]
    public async Task Should_Cache_Query_Results_By_Key()
    {
        await _queryCacher.GetOrCreateAsyncWithCache("key", _mockQueryable, _mockQueryFunc);
        await _mockQueryFunc.Received(1).Invoke(_mockQueryable, Arg.Any<CancellationToken>());

        await _queryCacher.GetOrCreateAsyncWithCache("key", _mockQueryable, _mockQueryFunc);
        await _mockQueryFunc.Received(1).Invoke(_mockQueryable, Arg.Any<CancellationToken>());

        await _queryCacher.GetOrCreateAsyncWithCache("different_key", _mockQueryable, _mockQueryFunc);
        await _mockQueryFunc.Received(2).Invoke(_mockQueryable, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Clear_All_With_ClearCache()
    {
        await _queryCacher.GetOrCreateAsyncWithCache("key", _mockQueryable, _mockQueryFunc);
        await _queryCacher.GetOrCreateAsyncWithCache("different_key", _mockQueryable, _mockQueryFunc);
        await _mockQueryFunc.Received(2).Invoke(_mockQueryable, CancellationToken.None);

        _queryCacher.ClearCache();

        await _queryCacher.GetOrCreateAsyncWithCache("key", _mockQueryable, _mockQueryFunc);
        await _queryCacher.GetOrCreateAsyncWithCache("different_key", _mockQueryable, _mockQueryFunc);
        await _mockQueryFunc.Received(4).Invoke(_mockQueryable, CancellationToken.None);
    }
}
