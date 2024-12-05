using Dfe.PlanTech.Web.Content;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Moq;
using Xunit;

namespace Dfe.ContentSupport.Web.Tests.Controllers;

public class ContentCacheControllerTests
{
    [Fact]
    public void Clear_Calls_CacheClear()
    {
        var cacheServiceMock = new Mock<ICacheService<List<CsPage>>>();
        var sut = new ContentCacheController(cacheServiceMock.Object);
        sut.Clear();

        cacheServiceMock.Verify(o => o.ClearCache(), Times.Once);
    }
}
