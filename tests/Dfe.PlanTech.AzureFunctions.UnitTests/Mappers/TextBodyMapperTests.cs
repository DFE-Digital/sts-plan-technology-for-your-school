namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public class TextBodyMapperTests : BaseMapperTests
{
    private string TextBodyId = "text body id";
    private readonly TextBodyMapper _mapper;
    private readonly ILogger<TextBodyMapper> _logger;
    private readonly RichTextContentMapper _richTextMapper = new();

    public TextBodyMapperTests()
    {
        _logger = Substitute.For<ILogger<TextBodyMapper>>();
        _mapper = new TextBodyMapper(_richTextMapper, _logger, JsonOptions);
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

        var mapped = _mapper.MapEntity(payload);

        Assert.NotNull(mapped);

        var concrete = mapped as TextBodyDbEntity;
        Assert.NotNull(concrete);

        Assert.True(RichTextContentMapperTests.ContentMatches<RichTextContent, RichTextData, RichTextMark, RichTextContentDbEntity, RichTextDataDbEntity, RichTextMarkDbEntity>(richText, concrete.RichText));
    }
}