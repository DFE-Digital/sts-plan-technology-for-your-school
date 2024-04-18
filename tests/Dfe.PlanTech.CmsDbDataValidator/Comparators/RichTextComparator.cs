using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public static class RichTextComparator
{
  //TODO: refactor using the BaseComparator class
  public static IEnumerable<string> CompareRichTextContent(RichTextContentDbEntity dbEntity, JsonNode contentfulEntity)
  {
    if (contentfulEntity?["nodeType"]?.GetValue<string>() != dbEntity.NodeType)
    {
      yield return $"Node type mismatch: Contentful '{contentfulEntity?["nodeType"]?.GetValue<string>()}' - Database '{dbEntity.NodeType}'";
    }

    var value = contentfulEntity?["value"]?.GetValue<string>() ?? "";
    if (value != dbEntity.Value)
    {
      yield return $"Value mismatch: Contentful '{value}' - Database '{dbEntity.Value}'";
    }

    var data = contentfulEntity?["data"];
    if (data?["uri"]?.GetValue<string>() != dbEntity.Data?.Uri)
    {
      yield return $"URI mismatch: Contentful '{data?["uri"]?.GetValue<string>()}' - Database '{dbEntity.Data?.Uri}'";
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

  private static IEnumerable<string> ValidateChildren(RichTextContentDbEntity dbEntity, JsonArray? children)
  {
    if (children?.Count != dbEntity.Content.Count)
    {
      yield return $"Children count mismatch: Contentful '{children?.Count}' vs Database '{dbEntity.Content.Count}'";
    }

    for (var x = 0; x < children?.Count; x++)
    {
      foreach (var error in CompareRichTextContent(dbEntity.Content[x], children[x]!))
      {
        yield return error;
      }
    }
  }
}