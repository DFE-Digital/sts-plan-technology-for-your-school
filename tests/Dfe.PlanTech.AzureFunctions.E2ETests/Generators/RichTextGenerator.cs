using Bogus;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Generators;

public class RichTextGenerator : Faker<RichTextContent>
{
    private float _chanceOfNull = 0.125f;

    public RichTextGenerator()
    {
        RuleFor(richText => richText.Content, f =>
        {
            var shouldGenerate = f.Random.Float(_chanceOfNull, 1.0f);
            if (shouldGenerate >= 0.75f)
            {
                return [];
            }

            var amountToGenerate = f.Random.Int(1, 5);
            _chanceOfNull *= 1.35f;

            return Generate(amountToGenerate);
        });
        RuleFor(richText => richText.Value, f => f.Lorem.Sentences(f.Random.Int(3, 10)));
        RuleFor(richText => richText.NodeType, f => f.PickRandom<RichTextNodeType>().ToString().ToLower());
        RuleFor(richText => richText.Data, (faker, richTextContent) =>
        {
            if (richTextContent.NodeType != "hyperlink")
            {
                return null;
            }

            return new RichTextData() { Uri = faker.Internet.Url() };
        });
        RuleFor(richText => richText.Marks, (faker, richTextContent) =>
        {
            var amountToGenerate = faker.Random.Int(0, 2).OrNull(faker, 0.8f);

            if (amountToGenerate == null) { return []; };

            return Enumerable.Range(0, amountToGenerate ?? 0).Select(_ => new RichTextMark() { Type = faker.PickRandom<MarkType>().ToString().ToLower() }).ToList();
        });
    }

    public IEnumerable<RichTextContent> Generate(Faker faker)
    {
        var amountToGenerate = faker.Random.Int(1, 5);
        return Generate(amountToGenerate);
    }

    public static IEnumerable<RichTextContentDbEntity> MapToDbEntities(IEnumerable<RichTextContent> richTextContents)
        => richTextContents.Select(MapToDbEntity).ToList();

    public static RichTextContentDbEntity MapToDbEntity(RichTextContent rtc)
    => new()
    {
        Value = rtc.Value,
        Content = MapToDbEntities(rtc.Content).ToList(),
        Marks = rtc.Marks.Select(mark => new RichTextMarkDbEntity()
        {
            Type = mark.Type,
        }).ToList(),
        Data = rtc.Data == null ? null : new RichTextDataDbEntity()
        {
            Uri = rtc.Data.Uri
        },
    };
}