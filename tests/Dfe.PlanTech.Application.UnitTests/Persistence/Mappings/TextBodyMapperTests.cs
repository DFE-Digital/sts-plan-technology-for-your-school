using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class TextBodyMapperTests : BaseMapperTests<TextBodyDbEntity, TextBodyMapper>
{
    private readonly string TextBodyId = "text body id";
    private readonly TextBodyMapper _mapper;
    private readonly ILogger<TextBodyMapper> _logger;
    private readonly RichTextContentMapper _richTextMapper = new();

    public TextBodyMapperTests()
    {
        _logger = Substitute.For<ILogger<TextBodyMapper>>();
        _mapper = new TextBodyMapper(EntityUpdater, _richTextMapper, Logger, JsonOptions, DatabaseHelper);
    }

    [Fact]
    public void Mapper_Should_Map_ToDbEntity()
    {
        var richText = RichTextContentMapperTests.GenerateContentParent();
        var fields = new Dictionary<string, object?>()
        {
            ["richText"] = WrapWithLocalisation(richText),
        };

        var payload = CreatePayload(fields, TextBodyId);

        var mapped = _mapper.ToEntity(payload);

        Assert.NotNull(mapped);
        Assert.NotNull(mapped.RichText);
        Assert.True(RichTextContentMapperTests.ContentMatches<RichTextContent, RichTextData, RichTextMark, RichTextContentDbEntity, RichTextDataDbEntity, RichTextMarkDbEntity>(richText, mapped.RichText));
    }
}
