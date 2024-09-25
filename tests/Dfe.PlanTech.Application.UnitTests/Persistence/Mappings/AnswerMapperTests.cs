using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class AnswerMapperTests : BaseMapperTests<AnswerDbEntity, AnswerMapper>
{
    private const string AnswerText = "Answer text goes here";
    private const Maturity AnswerMaturity = Maturity.Low;
    private const string AnswerId = "Answer Id";

    private readonly CmsWebHookSystemDetailsInnerContainer _nextQuestion = new CmsWebHookSystemDetailsInnerContainer() { Sys = new() { Id = "Answer Id" } };

    private readonly AnswerMapper _mapper;

    public AnswerMapperTests()
    {
        _mapper = new AnswerMapper(EntityUpdater, Logger, JsonOptions, DatabaseHelper);
    }

    [Fact]
    public void Mapper_Should_Map_Relationship()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["maturity"] = WrapWithLocalisation(AnswerMaturity),
            ["text"] = WrapWithLocalisation(AnswerText),
            ["nextQuestion"] = WrapWithLocalisation(_nextQuestion),
        };

        var payload = CreatePayload(fields, AnswerId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped;
        Assert.NotNull(concrete);

        Assert.Equal(AnswerId, concrete.Id);
        Assert.True(string.Equals(AnswerMaturity.ToString(), concrete.Maturity, StringComparison.InvariantCultureIgnoreCase));
        Assert.Equal(AnswerText, concrete.Text);
        Assert.Equal(_nextQuestion.Sys.Id, concrete.NextQuestionId);
    }
}
