using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Web.Content;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Content
{
    public class ContentServiceTests
    {
        private readonly IModelMapper _mapperMock;
        private readonly IGetContentSupportPageQuery _getContentSupportPageQueryMock;

        public ContentServiceTests()
        {
            _mapperMock = Substitute.For<IModelMapper>();
            _getContentSupportPageQueryMock = Substitute.For<IGetContentSupportPageQuery>();
        }

        private ContentService GetService() => new(_mapperMock, _getContentSupportPageQueryMock);

        [Fact]
        public async Task GetContent_Calls_Query_Once()
        {
            var slug = "slug1";
            var sut = GetService();
            var contentSupportPage = new ContentSupportPage { Slug = slug };

            _getContentSupportPageQueryMock.GetContentSupportPage(slug, Arg.Any<CancellationToken>())
                .Returns(contentSupportPage);

            await sut.GetContent(slug);

            await _getContentSupportPageQueryMock.Received(1).GetContentSupportPage(slug, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetContent_EmptyResponse_Returns_Null()
        {
            var slug = "slug1";
            _getContentSupportPageQueryMock.GetContentSupportPage(slug, Arg.Any<CancellationToken>())
                .Returns((ContentSupportPage?)null);

            var sut = GetService();

            var result = await sut.GetContent(slug);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetContent_Returns_Mapped_Result()
        {
            var slug = "slug1";
            var expectedPage = new ContentSupportPage { Slug = slug };
            var expectedCsPage = new CsPage { Slug = slug }; // Adjust this mapping as needed

            _getContentSupportPageQueryMock.GetContentSupportPage(slug, Arg.Any<CancellationToken>())
                .Returns(expectedPage);

            _mapperMock.MapToCsPage(expectedPage)
                .Returns(expectedCsPage);

            var sut = GetService();
            var result = await sut.GetContent(slug);

            result.Should().BeEquivalentTo(expectedCsPage);
        }
    }
}
