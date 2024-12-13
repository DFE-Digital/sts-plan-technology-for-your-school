using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Web.Content;
using FluentAssertions;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Content
{
    public class ContentServiceTests
    {
        private readonly Mock<IModelMapper> _mapperMock;
        private readonly Mock<IGetContentSupportPageQuery> _getContentSupportPageQueryMock;

        public ContentServiceTests()
        {
            _mapperMock = new Mock<IModelMapper>();
            _getContentSupportPageQueryMock = new Mock<IGetContentSupportPageQuery>();
        }

        private ContentService GetService() => new(_mapperMock.Object, _getContentSupportPageQueryMock.Object);

        [Fact]
        public async Task GetContent_Calls_Query_Once()
        {
            var slug = "slug1";
            var sut = GetService();
            var contentSupportPage = new ContentSupportPage { Slug = slug };

            _getContentSupportPageQueryMock
                .Setup(o => o.GetContentSupportPage(slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contentSupportPage);

            await sut.GetContent(slug);

            _getContentSupportPageQueryMock.Verify(o =>
                    o.GetContentSupportPage(slug, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetContent_EmptyResponse_Returns_Null()
        {
            var slug = "slug1";
            _getContentSupportPageQueryMock
                .Setup(o => o.GetContentSupportPage(slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ContentSupportPage?)null);

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

            _getContentSupportPageQueryMock
                .Setup(o => o.GetContentSupportPage(slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPage);

            _mapperMock.Setup(m => m.MapToCsPage(expectedPage))
                       .Returns(expectedCsPage);

            var sut = GetService();
            var result = await sut.GetContent(slug);

            result.Should().BeEquivalentTo(expectedCsPage);
        }
    }
}
