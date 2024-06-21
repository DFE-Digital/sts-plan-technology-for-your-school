
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class TextBodyTests() : EntityTests<TextBody, TextBodyDbEntity, TextBodyGenerator>
{
    protected override TextBodyGenerator CreateEntityGenerator() => new();

    protected override void ClearDatabase()
    {
        var titles = GetDbEntitiesQuery().ToList();
        Db.TextBodies.RemoveRange(titles);
        Db.SaveChanges();
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(TextBody entity)
     => new()
     {
         ["richText"] = entity.RichText,
     };

    protected override IQueryable<TextBodyDbEntity> GetDbEntitiesQuery() => Db.TextBodies.IgnoreQueryFilters().AsNoTracking();

    protected override async Task<TextBodyDbEntity?> GetDbEntityById(string id)
    {
        var textBody = await Db.TextBodies.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(tb => tb.Id == id) ?? throw new Exception($"Could not find TextBody for ID {id}");

        textBody.RichText ??= await GetRichTextContents.FirstAsync(rtc => rtc.Id == textBody.RichTextId);

        await LoadChildren(textBody.RichText);

        return textBody!;
    }

    protected virtual IQueryable<RichTextContentDbEntity> GetRichTextContents => Db.RichTextContents
                                                                                    .Include(rtc => rtc.Content)
                                                                                    .Include(rtc => rtc.Data)
                                                                                    .Include(rtc => rtc.Marks)
                                                                                    .IgnoreAutoIncludes()
                                                                                    .IgnoreQueryFilters()
                                                                                    .AsNoTracking();
    protected async Task LoadChildren(RichTextContentDbEntity richTextContentDbEntity)
    {
        var contents = await GetRichTextContents.Where(rtc => rtc.ParentId == richTextContentDbEntity.Id)
                                                .ToListAsync();

        richTextContentDbEntity.Content = contents;

        foreach (var content in richTextContentDbEntity.Content)
        {
            await LoadChildren(content);
        }
    }

    protected override void ValidateDbMatches(TextBody entity, TextBodyDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);
        ValidateEntityState(dbEntity, published, archived, deleted);
        ValidateRichText(entity.RichText, dbEntity.RichText);
    }

    protected static void ValidateRichText(RichTextContent richTextContent, RichTextContentDbEntity richTextContentDbEntity)
    {
        ValidateRichTextContent(richTextContent, richTextContentDbEntity);
    }

    public static void ValidateRichTextContent(RichTextContent richTextContent, RichTextContentDbEntity richTextContentDbEntity)
    {
        Assert.Equal(richTextContent.Value, richTextContentDbEntity.Value);
        Assert.Equal(richTextContent.NodeType, richTextContentDbEntity.NodeType);

        Assert.Equal(richTextContent.Marks.Count, richTextContentDbEntity.Marks.Count);
        foreach (var mark in richTextContent.Marks)
        {
            var matching = richTextContentDbEntity.Marks.FirstOrDefault(mark => mark.Type == mark.Type);
            Assert.NotNull(matching);
        }

        Assert.Equal(richTextContent.Data == null, richTextContentDbEntity.Data == null);

        if (richTextContent.Data != null)
        {
            Assert.Equal(richTextContent.Data.Uri, richTextContentDbEntity.Data!.Uri);
        }

        Assert.Equal(richTextContent.Content.Count, richTextContentDbEntity.Content.Count);

        foreach (var content in richTextContent.Content)
        {
            var matching = richTextContentDbEntity.Content.FirstOrDefault(rtc => rtc.Value == content.Value);
            Assert.NotNull(matching);
            ValidateRichText(content, matching);
        }
    }
}