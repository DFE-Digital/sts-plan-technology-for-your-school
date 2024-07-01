using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class QuestionMapperTests : BaseMapperTests
{
    private const string QuestionText = "Question text goes here";
    private const string QuestionHelpText = "Question help text";
    private const string QuestionSlug = "/question-slug";
    private readonly CmsWebHookSystemDetailsInnerContainer[] Answers = [
    new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = "Answer One Id" } },
        new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = "Answer Two Id" } },
        new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = "Answer Three Id" } },
    ];

    private const string QuestionId = "Question Id";
    private const string TestQuestionId = "test-question-id";
    private const string AnswerIdToKeep = "keep-me-id";
    private readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private readonly QuestionMapper _mapper;
    private readonly ILogger<JsonToDbMapper<QuestionDbEntity>> _logger;

    private readonly static AnswerDbEntity _expectedAnswer = new()
    {
        Id = "keep-me-id",
        ParentQuestionId = TestQuestionId,
        Order = 999
    };
    private readonly static List<AnswerDbEntity> _testAnswers = [
        new AnswerDbEntity()
        {
            Id = "remove-me-one",
            ParentQuestionId = TestQuestionId
        },
        new AnswerDbEntity()
        {
            Id = "remove-me-two",
            ParentQuestionId = TestQuestionId
        },
        _expectedAnswer
    ];

    private readonly static QuestionDbEntity _testQuestion = new()
    {
        Id = TestQuestionId,
        Answers = _testAnswers
    };

    private readonly List<QuestionDbEntity> _questions = [_testQuestion];
    private readonly DbSet<QuestionDbEntity> _questionDbSet;

    private readonly List<AnswerDbEntity> _answers = [_expectedAnswer];
    private readonly DbSet<AnswerDbEntity> _answerDbSet;

    public QuestionMapperTests()
    {
        _answers.AddRange(Answers.Select(a => new AnswerDbEntity() { Id = a.Sys.Id }));
        _questionDbSet = _questions.BuildMockDbSet();
        _answerDbSet = _answers.BuildMockDbSet();
        _db.Answers = _answerDbSet;
        _db.Questions = _questionDbSet;

        _db.Set<AnswerDbEntity>().Returns(_answerDbSet);
        _db.Set<QuestionDbEntity>().Returns(_questionDbSet);

        var entityTypeMock = Substitute.For<IEntityType>();
        entityTypeMock.ClrType.Returns(typeof(QuestionDbEntity));

        var model = Substitute.For<IModel>();

        var questionType = typeof(QuestionDbEntity);

        model.FindEntityType(Arg.Any<Type>()).Returns((callinfo) =>
        {
            var type = callinfo.ArgAt<Type>(0);

            if (type == questionType)
            {
                return entityTypeMock;
            }

            throw new NotImplementedException($"Not expecting type {type.Name}");
        });

        _db.Model.Returns(model);

        _logger = Substitute.For<ILogger<JsonToDbMapper<QuestionDbEntity>>>();
        _mapper = new QuestionMapper(MapperHelpers.CreateMockEntityRetriever(_db), MapperHelpers.CreateMockEntityUpdater(_db), _db, _logger, JsonOptions);
    }

    [Fact]
    public void Mapper_Should_Map_Relationship()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["text"] = WrapWithLocalisation(QuestionText),
            ["helpText"] = WrapWithLocalisation(QuestionHelpText),
            ["slug"] = WrapWithLocalisation(QuestionSlug),
            ["answers"] = WrapWithLocalisation(Answers),
        };

        var payload = CreatePayload(fields, QuestionId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped;
        Assert.NotNull(concrete);

        Assert.Equal(QuestionId, concrete.Id);
        Assert.Equal(QuestionText, concrete.Text);
        Assert.Equal(QuestionHelpText, concrete.HelpText);
        Assert.Equal(QuestionSlug, concrete.Slug);
    }

    [Fact]
    public async Task MapEntity_Should_Return_MappedEntity_When_NotExisting_In_DB()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["text"] = WrapWithLocalisation(QuestionText),
            ["helpText"] = WrapWithLocalisation(QuestionHelpText),
            ["slug"] = WrapWithLocalisation(QuestionSlug),
            ["answers"] = WrapWithLocalisation(Answers),
        };

        var payload = CreatePayload(fields, "doesnt-exist-in-db");

        var result = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.NotNull(result);

        var (incoming, existing) = result.GetTypedEntities<QuestionDbEntity>();

        Assert.NotNull(incoming);
        Assert.Null(existing);

        ValidateReferencedContent(Answers.Select(answer => answer.Sys.Id).ToArray(), incoming.Answers, true);
    }

    [Fact]
    public async Task MapEntity_Should_Update_Existing_Question()
    {
        CmsWebHookSystemDetailsInnerContainer[] answers = [
            new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = AnswerIdToKeep } },
        ];


        var fields = new Dictionary<string, object?>()
        {
            ["answers"] = WrapWithLocalisation(answers),
        };

        var payload = CreatePayload(fields, TestQuestionId);

        var result = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.NotNull(result);
        Assert.NotNull(result.IncomingEntity);
        Assert.NotNull(result.ExistingEntity);

        var existingQuestionEntity = result.ExistingEntity as QuestionDbEntity;

        Assert.NotNull(existingQuestionEntity);
        Assert.Single(existingQuestionEntity.Answers);

        var firstAnswer = existingQuestionEntity.Answers.First();
        Assert.Equal(AnswerIdToKeep, firstAnswer.Id);
        Assert.Equal(0, firstAnswer.Order);
    }

    [Fact]
    public async Task Should_Log_Error_If_Answer_Not_Found()
    {
        CmsWebHookSystemDetailsInnerContainer[] answers = [
            new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = "not-existing-id" } },
        ];


        var fields = new Dictionary<string, object?>()
        {
            ["answers"] = WrapWithLocalisation(answers),
        };

        var payload = CreatePayload(fields, TestQuestionId);
        _ = await _mapper.MapEntity(payload, CmsEvent.PUBLISH, default);

        Assert.Single(_logger.ReceivedCalls());
    }
}