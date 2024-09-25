using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class SectionMapperTests : BaseMapperTests<SectionDbEntity, SectionMapper>
{
    private static readonly string QuestionIdToKeep = "keep-me-id";
    private static readonly string TestSectionId = "section-id-one";

    private static readonly QuestionDbEntity QuestionToKeep = new()
    {
        Id = QuestionIdToKeep,
        SectionId = TestSectionId
    };

    private static readonly List<QuestionDbEntity> TestQuestions = [
        new QuestionDbEntity()
        {
            Id = "remove-me-one",
            SectionId = TestSectionId,
        },
        new QuestionDbEntity()
        {
            Id = "remove-me-two",
            SectionId = TestSectionId
        },
        QuestionToKeep
    ];

    private readonly SectionDbEntity _testSection;

    private readonly List<SectionDbEntity> _sections = [];

    private const string SectionName = "Section name";
    private readonly CmsWebHookSystemDetailsInnerContainer[] Questions = [
        new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = "Question One Id" } },
        new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = "Question Two Id" } },
        new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = "Question Three Id" } },
    ];

    private static readonly CmsWebHookSystemDetailsInnerContainer InterstitialPage = new()
    {
        Sys = new()
        {
            Id = "Interstitial page id"
        }
    };

    private const string SectionId = "Question Id";

    private readonly SectionMapper _mapper;

    private readonly List<PageDbEntity> _pages = [new()
    {
        Id = "Interstitial page id",
    }];

    public SectionMapperTests()
    {
        _testSection = new()
        {
            Id = TestSectionId,
            Questions = TestQuestions,
            InterstitialPageId = "Interstitial page id"
        };

        _sections.Add(_testSection);
        _mapper = new SectionMapper(EntityUpdater, Logger, JsonOptions, DatabaseHelper);

        MockDatabaseCollection(_sections);
        MockDatabaseCollection(_pages);
        MockDatabaseCollection(TestQuestions);
    }

    [Fact]
    public void Mapper_Should_Map_Relationship()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["name"] = WrapWithLocalisation(SectionName),
            ["interstitialPage"] = WrapWithLocalisation(InterstitialPage),
            ["questions"] = WrapWithLocalisation(Questions),
        };

        var payload = CreatePayload(fields, SectionId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        Assert.Equal(SectionId, mapped.Id);
        Assert.Equal(SectionName, mapped.Name);
        Assert.Equal(InterstitialPage.Sys.Id, mapped.InterstitialPageId);
    }

    [Fact]
    public async Task MapEntity_Should_Return_MappedEntity_When_NotExisting_In_DB()
    {
        CmsWebHookSystemDetailsInnerContainer[] questions = [
        ];

        var fields = new Dictionary<string, object?>()
        {
            ["questions"] = WrapWithLocalisation(questions),
        };

        var payload = CreatePayload(fields, "test-section-not-found");

        var result = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.IncomingEntity);
        Assert.Null(result.ExistingEntity);
    }

    [Fact]
    public async Task Should_Update_Existing_Entity()
    {
        CmsWebHookSystemDetailsInnerContainer[] questions = [
            new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = QuestionIdToKeep } },
        ];

        var fields = new Dictionary<string, object?>()
        {
            ["questions"] = WrapWithLocalisation(questions),
        };

        var payload = CreatePayload(fields, TestSectionId);

        var result = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.IncomingEntity);
        Assert.NotNull(result.ExistingEntity);

        var existingSectionEntity = result.ExistingEntity as SectionDbEntity;

        Assert.NotNull(existingSectionEntity);

        Assert.Single(existingSectionEntity.Questions);

        Assert.Equal(QuestionIdToKeep, existingSectionEntity.Questions.First().Id);
    }

    [Fact]
    public async Task Should_Log_Error_If_Question_Not_Found()
    {
        var missingId = "not-existing-id";
        CmsWebHookSystemDetailsInnerContainer[] questions = [
            new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = missingId } },
        ];

        var interstitialPage = new CmsWebHookSystemDetailsInnerContainer()
        { Sys = new CmsWebHookSystemDetailsInner() { Id = "Interstitial page Id" } };

        var fields = new Dictionary<string, object?>()
        {
            ["interstitialPage"] = WrapWithLocalisation(interstitialPage),
            ["questions"] = WrapWithLocalisation(questions),
        };

        var payload = CreatePayload(fields, TestSectionId);

        await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.Single(EntityUpdaterLogger.ReceivedCalls());
    }
}
