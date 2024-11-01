using Dfe.ContentSupport.Web.Controllers;
using Dfe.ContentSupport.Web.Models.Mapped;

namespace Dfe.ContentSupport.Web.Tests.Controllers;

public class CacheControllerTests
{
    [Fact]
    public void Clear_Calls_CacheClear()
    {
        var cacheServiceMock = new Mock<ICacheService<List<CsPage>>>();
        var sut = new CacheController(cacheServiceMock.Object);
        sut.Clear();

        cacheServiceMock.Verify(o => o.ClearCache(), Times.Once);
    }
}
