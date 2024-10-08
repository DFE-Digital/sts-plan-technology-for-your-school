using Contentful.Core;
using Contentful.Core.Search;

namespace Dfe.ContentSupport.Web.Tests.Http;

public class ContentfulServiceTests
{
    private readonly Mock<IContentfulClient> _clientMock = new();

    [Fact]
    public async Task Client_Calls_Contentful_And_Bounce()
    {
        var sut = new ContentfulService(_clientMock.Object);

        await sut.GetContentSupportPages(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>());

        _clientMock.Verify(o => o.GetEntries(It.IsAny<QueryBuilder<ContentSupportPage>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
