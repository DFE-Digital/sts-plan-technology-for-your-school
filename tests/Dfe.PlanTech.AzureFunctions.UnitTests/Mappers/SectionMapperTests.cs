using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class SectionMapperTests : BaseMapperTests
{
    private readonly string QuestionIdToKeep = "keep-me-id";
    private readonly static string TestSectionId = "section-id-one";
    private readonly static List<QuestionDbEntity> _testQuestions = [
                    new QuestionDbEntity()
                    {
                        Id = "remove-me-one",
                        SectionId = TestSectionId
                    },
        new QuestionDbEntity()
        {
            Id = "remove-me-two",
            SectionId = TestSectionId
        },
        new QuestionDbEntity()
        {
            Id = "keep-me-id",
            SectionId = TestSectionId
        }
    ];

    private readonly static SectionDbEntity _testSection = new()
    {
        Id = TestSectionId,
        Questions = _testQuestions
    };

    private readonly List<SectionDbEntity> _sections = [_testSection];
    private readonly DbSet<SectionDbEntity> _sectionDbSet;

    private const string SectionName = "Section name";
    private readonly CmsWebHookSystemDetailsInnerContainer[] Questions = new[]{
    new CmsWebHookSystemDetailsInnerContainer() {Sys = new() { Id = "Question One Id" } },
    new CmsWebHookSystemDetailsInnerContainer() {Sys = new() { Id = "Question Two Id" } },
    new CmsWebHookSystemDetailsInnerContainer() {Sys = new() { Id = "Question Three Id" } },
    };
    private readonly CmsWebHookSystemDetailsInnerContainer InterstitialPage = new()
    {
        Sys = new()
        {
            Id = "Interstitial page id"
        }
    };

    private const string SectionId = "Question Id";

    private readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private readonly SectionMapper _mapper;
    private readonly ILogger<SectionMapper> _logger;

    private readonly DbSet<QuestionDbEntity> _questionsDbSet;
    private readonly List<QuestionDbEntity> _attachedQuestions = new(4);

    private readonly List<PageDbEntity> _pages = [new()
    {
        Id = "Interstitial page id",
    }];

    private readonly DbSet<PageDbEntity> _pagesDbSet;

    public SectionMapperTests()
    {
        _questionsDbSet = _testQuestions.BuildMockDbSet();
        _sectionDbSet = _sections.BuildMockDbSet();
        _pagesDbSet = _pages.BuildMockDbSet();

        _db.Pages = _pagesDbSet;
        _db.Questions = _questionsDbSet;
        _db.Sections = _sectionDbSet;

        _db.Set<SectionDbEntity>().Returns(_sectionDbSet);
        _db.Set<QuestionDbEntity>().Returns(_questionsDbSet);

        _questionsDbSet.WhenForAnyArgs(questionDbSet => questionDbSet.Attach(Arg.Any<QuestionDbEntity>()))
                    .Do(callinfo =>
                    {
                        var question = callinfo.ArgAt<QuestionDbEntity>(0);
                        _attachedQuestions.Add(question);
                    });

        MockDbModel();

        _logger = Substitute.For<ILogger<SectionMapper>>();
        _mapper = new SectionMapper(MapperHelpers.CreateMockEntityRetriever(_db), MapperHelpers.CreateMockEntityUpdater(_db), _db, _logger, JsonOptions);
    }

    private void MockDbModel()
    {
        var modelMock = Substitute.For<IModel>();

        var entityTypeMock = Substitute.For<IEntityType>();
        entityTypeMock.ClrType.Returns(typeof(SectionDbEntity));

        var model = Substitute.For<IModel>();

        _db.Model.Returns(model);
        var sectionType = typeof(SectionDbEntity);

        model.FindEntityType(Arg.Any<Type>()).Returns((callinfo) =>
        {
            var type = callinfo.ArgAt<Type>(0);

            if (type == sectionType)
            {
                return entityTypeMock;
            }

            throw new NotImplementedException($"Not expecting type {type.Name}");
        });
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

        Assert.Equal(Questions.Length, _attachedQuestions.Count);

        foreach (var question in Questions)
        {
            var contains = _attachedQuestions.Any(attached => attached.Id == question.Sys.Id);
            Assert.True(contains);
        }
    }

    [Fact]
    public async Task MapEntity_Should_Return_MappedEntity_When_NotExisting_In_DB()
    {
        var payload = """
            {
                "fields": {
                    "questions": {
                        "en-US": [
                                {"id": "question-one-id"}
                        ]
                    }
                },
                "sys": {
                    "id": "section-id",
                    "type": "entry",
                    "contentType": {
                        "id": "section"
                    }
                }
            }
        """;

        var cmsWebHookPayload = JsonSerializer.Deserialize<CmsWebHookPayload>(payload, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

        Assert.NotNull(cmsWebHookPayload);

        var result = await _mapper.MapEntity(cmsWebHookPayload, CmsEvent.PUBLISH, default(CancellationToken));

        Assert.NotNull(result);
        Assert.NotNull(result.IncomingEntity);
        Assert.Null(result.ExistingEntity);
    }

    [Fact]
    public async Task MapEntity_Should_Update_Question_SectionIds()
    {
        CmsWebHookSystemDetailsInnerContainer[] questions = [
            new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = QuestionIdToKeep } },
        ];

        var fields = new Dictionary<string, object?>()
        {
            ["questions"] = WrapWithLocalisation(questions),
        };

        var payload = CreatePayload(fields, TestSectionId);

        var result = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default(CancellationToken));

        Assert.NotNull(result);
        Assert.NotNull(result.IncomingEntity);
        Assert.NotNull(result.ExistingEntity);

        var existingSectionEntity = result.ExistingEntity as SectionDbEntity;

        Assert.NotNull(existingSectionEntity);

        Assert.Single(existingSectionEntity.Questions);

        Assert.Equal(QuestionIdToKeep, existingSectionEntity.Questions.First().Id);

        Assert.All(_testQuestions.Where(answer => answer.Id != QuestionIdToKeep), (question) => Assert.Null(question.SectionId));
    }
}