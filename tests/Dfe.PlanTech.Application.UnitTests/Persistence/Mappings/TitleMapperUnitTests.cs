using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class TitleMapperUnitTests : BaseMapperTests<TitleDbEntity, TitleMapper>
{
    private const string TitleText = "Title text";
    private const string TitleId = "Title Id";

    private readonly TitleMapper _mapper;
    private readonly ILogger<TitleMapper> _logger;

    public TitleMapperUnitTests()
    {
        _logger = Substitute.For<ILogger<TitleMapper>>();
        _mapper = new TitleMapper(EntityUpdater, Logger, JsonOptions, DatabaseHelper);
    }

    [Fact]
    public void Mapper_Should_Map_NavigationLink()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["text"] = WrapWithLocalisation(TitleText),
        };

        var payload = CreatePayload(fields, TitleId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        Assert.Equal(TitleId, mapped.Id);
        Assert.Equal(TitleText, mapped.Text);
    }
}
