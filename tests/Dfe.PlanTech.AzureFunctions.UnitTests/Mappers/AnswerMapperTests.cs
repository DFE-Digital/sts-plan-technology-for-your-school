using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class AnswerMapperTests : BaseMapperTests
{
    private const string AnswerText = "Answer text goes here";
    private const Maturity AnswerMaturity = Maturity.Low;
    private const string AnswerId = "Answer Id";
    private readonly CmsWebHookSystemDetailsInnerContainer NextQuestion = new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = "Answer Id" } };

    private readonly AnswerMapper _mapper;
    private readonly ILogger<JsonToDbMapper<AnswerDbEntity>> _logger;
    private readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private readonly ILogger<EntityUpdater> _entityUpdaterLogger = Substitute.For<ILogger<EntityUpdater>>();

    public AnswerMapperTests()
    {
        _logger = Substitute.For<ILogger<JsonToDbMapper<AnswerDbEntity>>>();
        _mapper = new AnswerMapper(new EntityRetriever(_db), new EntityUpdater(_entityUpdaterLogger, _db), _logger, JsonOptions);
    }

    [Fact]
    public void Mapper_Should_Map_Relationship()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["maturity"] = WrapWithLocalisation(AnswerMaturity),
            ["text"] = WrapWithLocalisation(AnswerText),
            ["nextQuestion"] = WrapWithLocalisation(NextQuestion),
        };

        var payload = CreatePayload(fields, AnswerId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped;
        Assert.NotNull(concrete);

        Assert.Equal(AnswerId, concrete.Id);
        Assert.True(string.Equals(AnswerMaturity.ToString(), concrete.Maturity, StringComparison.InvariantCultureIgnoreCase));
        Assert.Equal(AnswerText, concrete.Text);
        Assert.Equal(NextQuestion.Sys.Id, concrete.NextQuestionId);
    }
}