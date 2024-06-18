using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class RecommendationIntroMapperTests : BaseMapperTests
{

    private const string ExistingIntroId = "TestingIntroId";
    private readonly string[] _contentIds = ["content1", "content2", "content3", "content4", "content5"];

    private readonly RecommendationIntroDbEntity ExistingRecommendationIntro = new()
    {
        Id = ExistingIntroId,
        Slug = "existing-slug",
        Maturity = Maturity.Unknown.ToString(),
        HeaderId = "existing-header-id",
    };

    private readonly List<ContentComponentDbEntity> _contentComponents = [];
    private readonly List<RecommendationIntroContentDbEntity> _recIntroContents = [];
    private readonly List<RecommendationIntroDbEntity> _recIntros = [];

    private readonly DbSet<ContentComponentDbEntity> _contentComponentDbSet;
    private readonly DbSet<RecommendationIntroContentDbEntity> _recIntroContentDbSet;
    private readonly DbSet<RecommendationIntroDbEntity> _recIntroDbSet;

    private static readonly CmsDbContext _db = Substitute.For<CmsDbContext>();

    private readonly ILogger<RecommendationIntroMapper> _recommendationIntroMapper = Substitute.For<ILogger<RecommendationIntroMapper>>();
    private static readonly ILogger<EntityUpdater> _entityUpdaterLogger = Substitute.For<ILogger<EntityUpdater>>();
    private static EntityUpdater CreateMockRecommendationIntroUpdater() => new(_entityUpdaterLogger, _db);
    private readonly RecommendationIntroMapper _mapper;

    public RecommendationIntroMapperTests()
    {
        for (var x = 0; x < _contentIds.Length; x++)
        {
            var contentId = _contentIds[x];

            var contentComponent = new ContentComponentDbEntity()
            {
                Id = contentId
            };

            _contentComponents.Add(contentComponent);

            if (x < 3)
            {
                var recommendationIntroContentDbEntity = new RecommendationIntroContentDbEntity()
                {
                    Id = x,
                    RecommendationIntroId = ExistingIntroId,
                    RecommendationIntro = ExistingRecommendationIntro,
                    ContentComponent = contentComponent,
                    ContentComponentId = contentId
                };

                _recIntroContents.Add(recommendationIntroContentDbEntity);
            }
        }

        _contentComponentDbSet = _contentComponents.BuildMockDbSet();
        _db.ContentComponents = _contentComponentDbSet;

        _recIntroContentDbSet = _recIntroContents.BuildMockDbSet();
        _db.RecommendationIntroContents = _recIntroContentDbSet;

        _recIntros.Add(ExistingRecommendationIntro);
        _recIntroDbSet = _recIntros.BuildMockDbSet();
        _db.RecommendationIntros = _recIntroDbSet;

        MockEntityEntry(_db, typeof(RecommendationIntroDbEntity), typeof(RecommendationIntroContentDbEntity));

        _db.Set<RecommendationIntroDbEntity>().Returns(_recIntroDbSet);

        _mapper = new RecommendationIntroMapper(MapperHelpers.CreateMockEntityRetriever(_db), CreateMockRecommendationIntroUpdater(), _db, _recommendationIntroMapper, JsonOptions);
    }

    [Fact]
    public async Task Should_Create_New_Recommendation_Intro()
    {
        var introId = "NewIntroId";
        var slug = "test-slug";
        var headerId = "header-id";
        var recommendationIntroPayload = CreateRecommendationIntroPayload(introId, slug, headerId, Maturity.Low, _contentIds);

        var mapped = await _mapper.MapEntity(recommendationIntroPayload, CmsEvent.PUBLISH, default);

        Assert.NotNull(mapped);

        var (incoming, existing) = mapped.GetTypedEntities<RecommendationIntroDbEntity>();
        Assert.NotNull(incoming);
        Assert.Null(existing);

        Assert.Equal(introId, incoming.Id);
        Assert.Equal(slug, incoming.Slug);
        Assert.Equal(headerId, incoming.HeaderId);

        Assert.Equal(_contentIds.Length, incoming.Content.Count);

        foreach (var contentId in _contentIds)
        {
            var matching = incoming.Content.FirstOrDefault(c => c.Id == contentId);
            Assert.NotNull(matching);
        }
    }

    [Fact]
    public async Task Should_Update_Existing_RecommendationIntro()
    {
        var newSlug = "new-slug";
        var newHeaderId = "new-header-id";
        var newContentIds = _contentIds[3..];
        var payload = CreateRecommendationIntroPayload(ExistingRecommendationIntro.Id, newSlug, newHeaderId, Maturity.High, newContentIds);

        var recommendationIntro = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.NotNull(recommendationIntro);

        var (incoming, existing) = recommendationIntro.GetTypedEntities<RecommendationIntroDbEntity>();

        Assert.NotNull(incoming);
        Assert.NotNull(existing);

        Assert.Equal(newContentIds.Length, existing.Content.Count);
        foreach (var contentId in newContentIds)
        {
            var matching = existing.Content.FirstOrDefault(content => content.Id == contentId);
            Assert.NotNull(matching);
        }
    }

    [Theory]
    [InlineData("Not-An-Existing-RecIntroId")]
    [InlineData(ExistingIntroId)]
    public async Task Should_LogWarning_On_Missing_References(string recIntroId)
    {
        string[] notFoundContentIds = ["not-found-content-id", "this-should-not-be-found"];

        var payload = CreateRecommendationIntroPayload(recIntroId, "test-slug", "test-header-id", Maturity.Medium, [.. _contentIds, .. notFoundContentIds]);
        var recommendationSection = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        var (incoming, existing) = recommendationSection.GetTypedEntities<RecommendationIntroDbEntity>();

        Assert.NotNull(incoming);

        var recIntroToCheck = (recIntroId == ExistingIntroId ? existing : incoming)!;

        if (recIntroId == ExistingIntroId)
        {
            Assert.NotNull(existing);
        }
        else
        {
            Assert.Null(existing);
        }

        Assert.Equal(_contentIds.Length, recIntroToCheck.Content.Count);
        FindLogMessagesContainingStrings(_entityUpdaterLogger, notFoundContentIds);

        foreach (var contentId in _contentIds)
        {
            var matching = recIntroToCheck.Content.FirstOrDefault(c => c.Id == contentId);
            Assert.NotNull(matching);
        }
    }

    private CmsWebHookPayload CreateRecommendationIntroPayload(string introId, string slug, string headerId, Maturity maturity, string[] contentIds)
    {
        CmsWebHookSystemDetailsInnerContainer[] content = contentIds.Select(CreateReferenceInnerForId).ToArray();

        var fields = new Dictionary<string, object?>()
        {
            ["slug"] = WrapWithLocalisation(slug),
            ["header"] = WrapWithLocalisation(CreateReferenceInnerForId(headerId)),
            ["maturity"] = WrapWithLocalisation(maturity.ToString()),
            ["content"] = WrapWithLocalisation(content),
        };

        var payload = CreatePayload(fields, introId);
        return payload;
    }

}