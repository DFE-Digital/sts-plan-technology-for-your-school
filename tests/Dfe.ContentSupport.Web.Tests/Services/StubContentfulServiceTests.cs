using Contentful.Core.Configuration;

namespace Dfe.ContentSupport.Web.Tests.Http;

public class StubContentfulClientTests
{
    [Fact]
    public async Task Client_Get_MockContent()
    {
        var sut = new StubContentfulService(new HttpClient(), new ContentfulOptions());

        var collection = await sut.GetContentSupportPages(It.IsAny<string>(), It.IsAny<string>());
        collection.First().InternalName.Should().BeEquivalentTo("MockContent");
    }
}
