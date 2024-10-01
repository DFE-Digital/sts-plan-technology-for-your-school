using Bogus;
using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public class RichTextContentMapperTests : BaseMapperTests
{
    private readonly RichTextContentMapper _mapper;

    public static readonly Faker<RichTextContent> RichTextContentMapper = new Faker<RichTextContent>().RuleFor(content => content.Data, faker => RichTextDataFaker!.Generate())
                                                                                                      .RuleFor(content => content.Marks, faker => RichTextMarkFaker.GenerateBetween(0, 5));
    private static readonly Faker<RichTextData> RichTextDataFaker = new Faker<RichTextData>().RuleFor(data => data.Uri, faker => faker.Internet.Url());
    private static readonly Faker<RichTextMark> RichTextMarkFaker = new Faker<RichTextMark>().RuleFor(mark => mark.Type, faker => faker.PickRandom<MarkType>().ToString());

    private readonly Faker<RichTextContentDbEntity> _richTextContentDbEntityFaker;
    private readonly Faker<RichTextDataDbEntity> _richTextDataDbEntityFaker;
    private readonly Faker<RichTextMarkDbEntity> _richTextMarkDbEntityFaker;

    private readonly string[] _nodeTypes = new[] { "document", "paragraph" };

    public RichTextContentMapperTests()
    {
        _mapper = new RichTextContentMapper();

        _richTextDataDbEntityFaker = new Faker<RichTextDataDbEntity>().RuleFor(data => data.Uri, faker => faker.Internet.Url());
        _richTextMarkDbEntityFaker = new Faker<RichTextMarkDbEntity>().RuleFor(mark => mark.Type, faker => faker.PickRandom<MarkType>().ToString());
        _richTextContentDbEntityFaker = new Faker<RichTextContentDbEntity>().RuleFor(content => content.Data, faker => _richTextDataDbEntityFaker.Generate())
                                                                            .RuleFor(content => content.Marks, faker => _richTextMarkDbEntityFaker.GenerateBetween(0, 5))
                                                                            .RuleFor(content => content.Value, faker => faker.Lorem.Text())
                                                                            .RuleFor(content => content.NodeType, faker => faker.PickRandom(_nodeTypes));

    }

    [Fact]
    public void Mapper_Should_Map_ToDbEntity_FromContent()
    {
        var richText = GenerateContentParent();

        var mapped = _mapper.MapToDbEntity(richText);

        var matches = ContentMatches<RichTextContent, RichTextData, RichTextMark, RichTextContentDbEntity, RichTextDataDbEntity, RichTextMarkDbEntity>(richText, mapped);

        Assert.True(matches);
    }

    [Fact]
    public void Mapper_Should_Map_ToContent_FromDb()
    {
        var richText = GenerateContentDbEntityParent();

        var mapped = _mapper.MapToRichTextContent(richText);

        var matches = ContentMatches<RichTextContentDbEntity, RichTextDataDbEntity, RichTextMarkDbEntity, RichTextContent, RichTextData, RichTextMark>(richText, mapped);

        Assert.True(matches);
    }

    public static RichTextContent GenerateContentParent()
    {
        int minimumChildren = 4;
        var parent = RichTextContentMapper.Generate();
        parent.Content = RichTextContentMapper.GenerateBetween(minimumChildren, minimumChildren * 2);

        GenerateContent(parent, minimumChildren);

        return parent;
    }

    static void GenerateContent(RichTextContent node, int minimumChildren)
    {
        if (node == null || minimumChildren < 1)
        {
            return;
        }

        foreach (var child in node.Content)
        {
            child.Content = RichTextContentMapper.GenerateBetween(minimumChildren, minimumChildren * 2);
            GenerateContent(child, minimumChildren - 1);
        }
    }

    public RichTextContentDbEntity GenerateContentDbEntityParent()
    {
        int minimumChildren = 4;
        var parent = _richTextContentDbEntityFaker.Generate();
        parent.Content = _richTextContentDbEntityFaker.GenerateBetween(minimumChildren, minimumChildren * 2);

        var currentContent = parent;

        while (minimumChildren > 0)
        {
            minimumChildren--;

            foreach (var child in currentContent.Content)
            {
                child.Content = _richTextContentDbEntityFaker.GenerateBetween(minimumChildren, minimumChildren * 2);
            }
        }

        return parent;
    }


    public static bool ContentMatches<TContentIn, TDataIn, TMarkIn, TContentOut, TDataOut, TMarkOut>(TContentIn expected, TContentOut actual)
    where TContentIn : IRichTextContent<TMarkIn, TContentIn, TDataIn>, new()
    where TMarkIn : IRichTextMark, new()
    where TDataIn : IRichTextData, new()
    where TContentOut : IRichTextContent<TMarkOut, TContentOut, TDataOut>, new()
    where TMarkOut : IRichTextMark, new()
    where TDataOut : IRichTextData, new()
    {
        var marksMatch = expected.Marks.Zip(actual.Marks, (expectedMark, actualMark) => MarkMatches(expectedMark, actualMark))
                                       .All(matches => matches);

        if (!marksMatch)
            return false;

        var dataMatches = DataMatches(expected.Data, actual.Data);

        if (!dataMatches)
            return false;

        var valuesMatch = expected.NodeType == actual.NodeType && expected.Value == actual.Value;

        if (!valuesMatch)
            return false;

        var childrenMatch = expected.Content.Count == actual.Content.Count &&
                              expected.Content.All(content => actual.Content.Any(actualContent => ContentMatches<TContentIn, TDataIn, TMarkIn, TContentOut, TDataOut, TMarkOut>(content, actualContent)));

        return childrenMatch;
    }

    public static bool MarkMatches(IRichTextMark expected, IRichTextMark actual)
    {
        return expected.Type == actual.Type;
    }

    public static bool DataMatches(IRichTextData? expected, IRichTextData? actual)
    {
        if (expected == null && actual == null)
            return true;

        var nullabilityMatches = (expected == null) == (actual == null);

        if (!nullabilityMatches)
            return nullabilityMatches;

        return expected!.Uri == actual!.Uri;
    }
}
