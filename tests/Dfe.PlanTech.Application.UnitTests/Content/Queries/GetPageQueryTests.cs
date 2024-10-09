using AutoMapper;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetPageQueryTests
{
    private readonly static string DbPageSlug = "/retrieved-from-db";
    private readonly static string ContentfulPageSlug = "/retrieved-from-contentful";

    private readonly GetPageFromDbQuery _getPageFromDbQuery;
    private readonly GetPageFromContentfulQuery _getPageFromContentfulQuery;

    private readonly IContentRepository _repository = Substitute.For<IContentRepository>();
    private readonly ICmsDbContext _db = Substitute.For<ICmsDbContext>();

    private readonly Page _page = new()
    {
        Slug = ContentfulPageSlug
    };

    private readonly PageDbEntity _pageDbEntity = new()
    {
        Id = "PageId",
        Slug = DbPageSlug,
        Content = [
            new QuestionDbEntity()
            {
                Id = "QuestionId"
            }
        ],
        AllPageContents = [
            new PageContentDbEntity()
            {
                Id = 1,
                PageId = "PageId",
                ContentComponentId = "QuestionId"
            }
        ]
    };

    public GetPageQueryTests()
    {
        var mapper = Substitute.For<IMapper>();

        _getPageFromDbQuery = new GetPageFromDbQuery(_db, new NullLogger<GetPageFromDbQuery>(), mapper, Array.Empty<IGetPageChildrenQuery>());

        _db.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(callinfo =>
        {
            var slug = callinfo.ArgAt<string>(0);

            var test = slug.Equals(DbPageSlug);
            return slug.Equals(DbPageSlug) ? _pageDbEntity : null;
        });

        mapper.Map<PageDbEntity, Page>(Arg.Any<PageDbEntity>()).Returns(callinfo =>
        {
            var page = callinfo.ArgAt<PageDbEntity>(0);

            return new Page()
            {
                Slug = page.Slug
            };
        });


        _repository = Substitute.For<IContentRepository>();

        _repository.GetEntities<Page>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>())
                    .Returns(callinfo =>
                    {
                        var pages = new List<Page>();

                        var options = callinfo.ArgAt<GetEntitiesOptions>(0);

                        var queries = options.Queries;

                        var slugQuery = (queries?.OfType<ContentQueryEquals>()).FirstOrDefault(query => query.Field == "fields.slug");
                        if (slugQuery != null && slugQuery.Value.Equals(_page.Slug))
                        {
                            pages.Add(_page);
                        }

                        return pages;
                    });

        _getPageFromContentfulQuery = new GetPageFromContentfulQuery(_repository,
                                                                    new NullLogger<GetPageFromContentfulQuery>(),
                                                                    new GetPageFromContentfulOptions() { Include = 4 });
    }

    [Fact]
    public async Task Should_Retrieve_Page_By_Slug_From_Db()
    {
        var query = new GetPageQuery(_getPageFromContentfulQuery, _getPageFromDbQuery);

        var result = await query.GetPageBySlug(DbPageSlug);

        Assert.NotNull(result);
        Assert.Equal(DbPageSlug, result.Slug);

        await _db.Received(1).GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _repository.Received(0).GetEntities<Page>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Retrieve_Page_By_Slug_From_Contentful_When_Db_Page_Not_Found()
    {
        var query = new GetPageQuery(_getPageFromContentfulQuery, _getPageFromDbQuery);

        var result = await query.GetPageBySlug(ContentfulPageSlug);

        Assert.NotNull(result);
        Assert.Equal(ContentfulPageSlug, result.Slug);

        await _db.Received(1).GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).GetEntities<Page>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>());
    }
}
