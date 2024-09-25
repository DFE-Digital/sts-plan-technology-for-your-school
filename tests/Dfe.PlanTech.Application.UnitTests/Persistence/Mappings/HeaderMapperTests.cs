using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class HeaderMapperTests : BaseMapperTests<HeaderDbEntity, HeaderMapper>
{
    private const string HeaderText = "Header text goes here";
    private const HeaderSize HeaderSizeValue = HeaderSize.Medium;
    private const HeaderTag HeaderTagValue = HeaderTag.H2;
    private const string HeaderId = "Header Id";

    private readonly HeaderMapper _mapper;

    public HeaderMapperTests()
    {
        _mapper = new HeaderMapper(EntityUpdater, Logger, JsonOptions, DatabaseHelper);
    }

    [Fact]
    public void Mapper_Should_Map_Relationship()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["text"] = WrapWithLocalisation(HeaderText),
            ["size"] = WrapWithLocalisation(HeaderSizeValue),
            ["tag"] = WrapWithLocalisation(HeaderTagValue),
        };

        var payload = CreatePayload(fields, HeaderId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped;
        Assert.NotNull(concrete);

        Assert.Equal(HeaderId, concrete.Id);
        Assert.Equal(HeaderText, concrete.Text);
        Assert.Equal(HeaderSizeValue, concrete.Size);
        Assert.Equal(HeaderTagValue, concrete.Tag);
    }
}
