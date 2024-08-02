using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class CSLinkMapperTests : BaseMapperTests
{
    private const string LinkText = "CSLink text goes here";
    private const string Url = "http://test.com";
    private const string CSLinkId = "CSLink-Id";

    private readonly CSLinkMapper _mapper;
    private readonly ILogger<CSLinkMapper> _logger;
    public CSLinkMapperTests()
    {
        _logger = Substitute.For<ILogger<CSLinkMapper>>();
        _mapper = new CSLinkMapper(MapperHelpers.CreateMockEntityRetriever(), MapperHelpers.CreateMockEntityUpdater(), _logger, JsonOptions);
    }

    [Fact]
    public void Mapper_Should_Map_Relationship()
    {
        var fields = new Dictionary<string, object?>()
        {
            ["url"] = WrapWithLocalisation(Url),
            ["linkText"] = WrapWithLocalisation(LinkText),
        };

        var payload = CreatePayload(fields, CSLinkId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped;
        Assert.NotNull(concrete);

        Assert.Equal(CSLinkId, concrete.Id);
        Assert.Equal(Url, concrete.Url);
        Assert.Equal(LinkText, concrete.LinkText);
    }
}
