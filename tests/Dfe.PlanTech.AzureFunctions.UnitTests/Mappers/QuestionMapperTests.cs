using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class QuestionMapperTests : BaseMapperTests
{
    private const string QuestionText = "Question text goes here";
    private const string QuestionHelpText = "Question help text";
    private const string QuestionSlug = "/question-slug";
    private readonly CmsWebHookSystemDetailsInnerContainer[] Answers = new[]{
    new CmsWebHookSystemDetailsInnerContainer() {Sys = new() { Id = "Answer One Id" } },
    new CmsWebHookSystemDetailsInnerContainer() {Sys = new() { Id = "Answer Two Id" } },
    new CmsWebHookSystemDetailsInnerContainer() {Sys = new() { Id = "Answer Three Id" } },
    };

    private const string QuestionId = "Question Id";

    private readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private readonly QuestionMapper _mapper;
    private readonly ILogger<JsonToDbMapper<QuestionDbEntity>> _logger;

    private readonly DbSet<AnswerDbEntity> _answerDbSet = Substitute.For<DbSet<AnswerDbEntity>>();
    private readonly List<AnswerDbEntity> _attachedAnswers = new(4);
    public QuestionMapperTests()
    {
        _logger = Substitute.For<ILogger<JsonToDbMapper<QuestionDbEntity>>>();
        _mapper = new QuestionMapper(_db, _logger, JsonOptions);
    }

    [Fact]
    public void Mapper_Should_Map_Relationship()
    {
        _db.Answers = _answerDbSet;

        _answerDbSet.WhenForAnyArgs(answerDbSet => answerDbSet.Attach(Arg.Any<AnswerDbEntity>()))
                    .Do(callinfo =>
                    {
                        var answer = callinfo.ArgAt<AnswerDbEntity>(0);
                        _attachedAnswers.Add(answer);
                    });

        var fields = new Dictionary<string, object?>()
        {
            ["text"] = WrapWithLocalisation(QuestionText),
            ["helpText"] = WrapWithLocalisation(QuestionHelpText),
            ["slug"] = WrapWithLocalisation(QuestionSlug),
            ["answers"] = WrapWithLocalisation(Answers),
        };

        var payload = CreatePayload(fields, QuestionId);

        var mapped = _mapper.MapEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped as QuestionDbEntity;
        Assert.NotNull(concrete);

        Assert.Equal(QuestionId, concrete.Id);
        Assert.Equal(QuestionText, concrete.Text);
        Assert.Equal(QuestionHelpText, concrete.HelpText);
        Assert.Equal(QuestionSlug, concrete.Slug);

        Assert.Equal(Answers.Length, _attachedAnswers.Count);

        foreach (var item in Answers.Select((answer, index) => new { answer, index }))
        {
            var matchingAnswer = _attachedAnswers.Find(attached => attached.Id == item.answer.Sys.Id);
            Assert.NotNull(matchingAnswer);
            Assert.Equal(item.index, matchingAnswer.Order);
        }
    }
}