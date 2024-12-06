using Contentful.Core.Configuration;
using FluentAssertions;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Content;

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
