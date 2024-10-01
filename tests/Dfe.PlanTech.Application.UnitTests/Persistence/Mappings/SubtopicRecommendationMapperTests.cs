using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class SubtopicRecommendationMapperTests : BaseMapperTests<SubtopicRecommendationDbEntity, SubtopicRecommendationMapper>
{

    private const string ExistingSubtopicRecommendationId = "subtopic-recommendation-id";
    private readonly string[] _introIds = ["content1", "content2", "content3", "content4", "content5"];

    private readonly static SubtopicRecommendationDbEntity ExistingSubtopicRecommendation = new()
    {
        Id = ExistingSubtopicRecommendationId,
        SectionId = "section-id",
        SubtopicId = "subtopic-id"
    };

    private readonly List<RecommendationIntroDbEntity> _recommendationIntros = [];
    private readonly List<SubtopicRecommendationIntroDbEntity> _subTopicRecIntros = [];
    private readonly List<SubtopicRecommendationDbEntity> _subtopicRecs = [ExistingSubtopicRecommendation];

    private readonly SubtopicRecommendationMapper _mapper;

    public SubtopicRecommendationMapperTests()
    {
        for (var x = 0; x < _introIds.Length; x++)
        {
            var introId = _introIds[x];

            var intro = new RecommendationIntroDbEntity()
            {
                Id = introId
            };

            _recommendationIntros.Add(intro);

            if (x < 3)
            {
                var subtopicRecIntro = new SubtopicRecommendationIntroDbEntity()
                {
                    Id = x,
                    SubtopicRecommendationId = ExistingSubtopicRecommendationId,
                    SubtopicRecommendation = ExistingSubtopicRecommendation,
                    RecommendationIntro = intro,
                    RecommendationIntroId = introId
                };

                _subTopicRecIntros.Add(subtopicRecIntro);
            }
        }

        _mapper = new SubtopicRecommendationMapper(EntityUpdater, Logger, JsonOptions, DatabaseHelper);

        MockDatabaseCollection(_subTopicRecIntros);
        MockDatabaseCollection(_recommendationIntros);
        MockDatabaseCollection(_subtopicRecs);
    }

    [Fact]
    public async Task Should_Create_New_Subtopic_Recommendation()
    {
        var subtopicRecId = "new-subtopic-rec-id";
        var subtopicId = "subtopic-id";
        var sectionId = "section-id";

        var recommendationIntroPayload = CreateSubtopicRecommendationPayload(subtopicRecId, sectionId, subtopicId, _introIds);

        var mapped = await _mapper.MapEntity(recommendationIntroPayload, CmsEvent.PUBLISH, default);

        Assert.NotNull(mapped);

        var (incoming, existing) = mapped.GetTypedEntities<SubtopicRecommendationDbEntity>();

        Assert.NotNull(incoming);
        Assert.Null(existing);

        ValidateFields(incoming, subtopicRecId, sectionId, subtopicId, _introIds);
    }

    [Fact]
    public async Task Should_Update_Existing_SubtopicRecommendation()
    {
        var subtopicId = "new-subtopic-id";
        var sectionId = "new-section-id";
        var newIntroIds = _introIds[2..];
        var payload = CreateSubtopicRecommendationPayload(ExistingSubtopicRecommendation.Id, sectionId, subtopicId, newIntroIds);

        var subtopicRecommendation = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.NotNull(subtopicRecommendation);

        var (incoming, existing) = subtopicRecommendation.GetTypedEntities<SubtopicRecommendationDbEntity>();

        Assert.NotNull(incoming);
        Assert.NotNull(existing);

        ValidateFields(existing, ExistingSubtopicRecommendationId, sectionId, subtopicId, newIntroIds);
    }

    [Theory]
    [InlineData("Not-An-Existing-RecIntroId")]
    [InlineData(ExistingSubtopicRecommendationId)]
    public async Task Should_LogWarning_On_Missing_References(string subtopicRecId)
    {
        string[] notFoundIntroIds = ["not-found-intro-id", "this-should-not-be-found"];

        string sectionId = "section-id-to-change-to";
        string subtopicId = "newest-subtopic-id";

        string[] introIds = [.. _introIds, .. notFoundIntroIds];
        var payload = CreateSubtopicRecommendationPayload(subtopicRecId, sectionId, subtopicId, introIds);
        var recommendationSection = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        var (incoming, existing) = recommendationSection.GetTypedEntities<SubtopicRecommendationDbEntity>();

        Assert.NotNull(incoming);

        var subtopicRecToCheck = (subtopicRecId == ExistingSubtopicRecommendationId ? existing : incoming)!;

        if (subtopicRecId == ExistingSubtopicRecommendationId)
        {
            Assert.NotNull(existing);
        }
        else
        {
            Assert.Null(existing);
        }

        FindLogMessagesContainingStrings(EntityUpdaterLogger, notFoundIntroIds);

        ValidateFields(subtopicRecToCheck, subtopicRecId, sectionId, subtopicId, _introIds);
    }

    private static void ValidateFields(SubtopicRecommendationDbEntity entity, string subtopicRecId, string sectionId, string subtopicId, string[] introIds)
    {
        Assert.Equal(subtopicRecId, entity.Id);
        Assert.Equal(sectionId, entity.SectionId);
        Assert.Equal(subtopicId, entity.SubtopicId);

        ValidateIntros(entity, introIds);
    }

    private static void ValidateIntros(SubtopicRecommendationDbEntity entity, string[] introIds)
    {
        Assert.Equal(introIds.Length, entity.Intros.Count);

        foreach (var introId in introIds)
        {
            var matching = entity.Intros.FirstOrDefault(c => c.Id == introId);
            Assert.NotNull(matching);
        }
    }

    private CmsWebHookPayload CreateSubtopicRecommendationPayload(string subtopicRecId, string sectionId, string subtopicId, string[] introIds)
    {
        CmsWebHookSystemDetailsInnerContainer[] intros = introIds.Select(CreateReferenceInnerForId).ToArray();

        var fields = new Dictionary<string, object?>()
        {
            ["section"] = WrapWithLocalisation(CreateReferenceInnerForId(sectionId)),
            ["subtopic"] = WrapWithLocalisation(CreateReferenceInnerForId(subtopicId)),
            ["intros"] = WrapWithLocalisation(intros),
        };

        return CreatePayload(fields, subtopicRecId);
    }

}
