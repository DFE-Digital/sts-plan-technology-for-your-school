using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Models;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class TextBodyComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, [], "TextBody")
{
    public override Task ValidateContent()
    {
        ValidateTextBodys(_dbEntities.OfType<TextBodyDbEntity>().ToArray());
        return Task.CompletedTask;
    }

    private void ValidateTextBodys(TextBodyDbEntity[] textBodies)
    {
        foreach (var contentfulTextBody in _contentfulEntities)
        {
            ValidateTextBody(textBodies, contentfulTextBody);
        }
    }

    private void ValidateTextBody(TextBodyDbEntity[] textBodies, JsonNode contentfulTextBody)
    {
        var matchingTextBody = TryRetrieveMatchingDbEntity(textBodies, contentfulTextBody);

        if (matchingTextBody == null)
        {
            return;
        }
        ValidateProperties(contentfulTextBody, matchingTextBody, GenerateDataValidationErrors(matchingTextBody, contentfulTextBody).ToArray());
    }

    protected IEnumerable<DataValidationError> GenerateDataValidationErrors(TextBodyDbEntity dbTextbody, JsonNode contentfulTextBody)
    {
        var contentfulRichText = contentfulTextBody["richText"];

        if (contentfulRichText == null && dbTextbody.RichText != null)
        {
            yield return new DataValidationError("RichText", $"Null in Contentful but exists in DB");
            yield break;
        }
        else if (contentfulRichText != null && dbTextbody.RichText == null)
        {
            yield return new DataValidationError("RichText", $"Null in DB but exists in Contentful");
            yield break;
        }

        var errors = RichTextComparator.CompareRichTextContent(dbTextbody.RichText!, contentfulRichText!).ToArray();

        if (errors.Length == 0)
            yield break;

        foreach (var error in errors)
        {
            if (error != null)
                yield return error;
        }
    }

    /// <remarks>
    /// Due to the size of TextBody entities, fetching all in one will cause timeout issue.
    /// Paginate through them instead.
    /// </remarks>
    protected override Task<bool> GetDbEntities()
    {
        return GetDbEntitiesPaginated(50);
    }

    protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
    {
        return _db.TextBodies;
    }
}
