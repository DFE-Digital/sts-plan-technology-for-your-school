using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public static class RichTextComparator
{
  //TODO: refactor using the BaseComparator class
  public static IEnumerable<string> CompareRichTextContent(RichTextContentDbEntity dbEntity, JsonNode contentfulEntity)
  {
    var contentfulNodeType = contentfulEntity?["nodeType"]?.GetValue<string>();
    if (contentfulNodeType != dbEntity.NodeType)
    {
      yield return $"Node type mismatch: Contentful '{contentfulNodeType}' - Database '{dbEntity.NodeType}'";
    }

    var value = contentfulEntity?["value"]?.GetValue<string>() ?? "";
    if (value != dbEntity.Value)
    {
      yield return $"Value mismatch: Contentful '{value}' - Database '{dbEntity.Value}'";
    }

    var data = contentfulEntity?["data"];
    var uri = data?["uri"]?.GetValue<string>();
    if (uri != dbEntity.Data?.Uri)
    {
      yield return $"URI mismatch: Contentful '{uri}' - Database '{dbEntity.Data?.Uri}'";
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
    var childrenNullOrEmpty = children == null || children.Count == 0;
    var dbChildrenNullorEmpty = dbEntity.Content == null || dbEntity.Content.Count == 0;

    if (childrenNullOrEmpty && !dbChildrenNullorEmpty)
    {
      yield return $"Contentful entity has no children but DB entity has {dbEntity.Content!.Count} children";
      yield break;
    }
    else if (!childrenNullOrEmpty && dbChildrenNullorEmpty)
    {
      yield return $"Contentful entity has {children!.Count} children but DB entity has no children";
      yield break;
    }
    else if (children!.Count != dbEntity.Content!.Count)
    {
      yield return $"Children count mismatch: Contentful '{children!.Count}' vs Database '{dbEntity.Content.Count}'";
      yield break;
    }

    var errors = children.SelectMany((child, index) => CompareRichTextContent(dbEntity.Content[index], child!));
    foreach (var error in errors)
    {
      yield return error;
    }
  }
}