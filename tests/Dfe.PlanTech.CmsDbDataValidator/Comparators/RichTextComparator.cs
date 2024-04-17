using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public static class RichTextComparator
{
  //TODO: refactor using the BaseComparator class
  public static void CompareRichTextContent(RichTextContentDbEntity dbEntity, JsonNode contentfulEntity)
  {
    if (contentfulEntity?["nodeType"]?.GetValue<string>() != dbEntity.NodeType)
    {
      Console.WriteLine($"Node type mismatch: Contentful '{contentfulEntity?["nodeType"]?.GetValue<string>()}' - Database '{dbEntity.NodeType}'");
    }

    var value = contentfulEntity?["value"]?.GetValue<string>() ?? "";
    if (value != dbEntity.Value)
    {
      Console.WriteLine($"Value mismatch: Contentful '{value}' - Database '{dbEntity.Value}'");
    }

    var data = contentfulEntity?["data"];
    if (data?["uri"]?.GetValue<string>() != dbEntity.Data?.Uri)
    {
      Console.WriteLine($"URI mismatch: Contentful '{data?["uri"]?.GetValue<string>()}' - Database '{dbEntity.Data?.Uri}'");
    }

    var children = contentfulEntity?["content"]?.AsArray();

    if (children != null)
    {
      ValidateChildren(dbEntity, children);
    }
  }

  private static void ValidateChildren(RichTextContentDbEntity dbEntity, JsonArray? children)
  {
    if (children?.Count != dbEntity.Content.Count)
    {
      Console.WriteLine($"Children count mismatch: Contentful '{children?.Count}' vs Database '{dbEntity.Content.Count}'");
    }

    for (var x = 0; x < children?.Count; x++)
    {
      CompareRichTextContent(dbEntity.Content[x], children[x]!);
    }
  }
}