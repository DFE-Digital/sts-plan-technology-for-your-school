using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Models;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public static class RichTextComparator
{
    //TODO: refactor using the BaseComparator class
    public static IEnumerable<DataValidationError> CompareRichTextContent(RichTextContentDbEntity dbEntity, JsonNode contentfulEntity)
    {
        var contentfulNodeType = contentfulEntity?["nodeType"]?.GetValue<string>();
        if (contentfulNodeType != dbEntity.NodeType)
        {
            yield return new DataValidationError("NodeType", $"Contentful '{contentfulNodeType}' - Database '{dbEntity.NodeType}'");
        }

        var value = contentfulEntity?["value"]?.GetValue<string>() ?? "";
        if (value != dbEntity.Value)
        {
            yield return new DataValidationError("Value", $"Contentful '{value}' - Database '{dbEntity.Value}'");
        }

        var data = contentfulEntity?["data"];
        var uri = data?["uri"]?.GetValue<string>();

        if (uri != dbEntity.Data?.Uri)
        {
            yield return new DataValidationError("Data.Uri", $"Contentful '{uri}' - Database '{dbEntity.Data?.Uri}'");
        }

        var children = contentfulEntity?["content"]?.AsArray();

        if (children != null)
        {
            foreach (var error in ValidateChildren(dbEntity, children))
            {
                yield return error;
            }
        }
    }

    private static IEnumerable<DataValidationError> ValidateChildren(RichTextContentDbEntity dbEntity, JsonArray? children)
    {
        var childrenNullOrEmpty = children == null || children.Count == 0;
        var dbChildrenNullorEmpty = dbEntity.Content == null || dbEntity.Content.Count == 0;

        if (childrenNullOrEmpty && !dbChildrenNullorEmpty)
        {
            yield return new DataValidationError("Content children", $"Contentful has no children - Database {dbEntity.Id} has {dbEntity.Content!.Count} children");
            yield break;
        }
        else if (!childrenNullOrEmpty && dbChildrenNullorEmpty)
        {
            yield return new DataValidationError("Content children", $"Contentful has {children!.Count} children - Database {dbEntity.Id} has no children");
            yield break;
        }
        else if (children!.Count != dbEntity.Content!.Count)
        {
            yield return new DataValidationError("Content children", $"Count mismatch: Contentful '{children!.Count}' - Database {dbEntity.Id} has '{dbEntity.Content.Count}'");
            yield break;
        }

        var errors = children.SelectMany((child, index) => CompareRichTextContent(dbEntity.Content[index], child!));
        foreach (var error in errors)
        {
            yield return error;
        }
    }
}
