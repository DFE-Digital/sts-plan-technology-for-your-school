using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class InsetTextMapperTests : BaseMapperTests<InsetTextDbEntity, InsetTextMapper>
{
    private const string InsetTextValue = "Inset text goes here";
    private const string InsetTextId = "Header Id";

    private readonly InsetTextMapper _mapper;

    public InsetTextMapperTests()
    {
        _mapper = new InsetTextMapper(EntityUpdater, Logger, JsonOptions, DatabaseHelper);
    }

    [Fact]
    public void Mapper_Should_Map_Relationship()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["text"] = WrapWithLocalisation(InsetTextValue),
        };

        var payload = CreatePayload(fields, InsetTextId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped;
        Assert.NotNull(concrete);

        Assert.Equal(InsetTextId, concrete.Id);
        Assert.Equal(InsetTextValue, concrete.Text);
    }
}
