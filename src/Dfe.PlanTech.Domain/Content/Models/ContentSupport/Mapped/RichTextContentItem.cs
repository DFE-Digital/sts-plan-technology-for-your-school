﻿using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;

[ExcludeFromCodeCoverage]
public class RichTextContentItem : CsContentItem
{
    public List<RichTextContentItem> Content { get; set; } = null!;
    public RichTextNodeType NodeType { get; set; } = RichTextNodeType.Unknown;
    public string Value { get; set; } = null!;
    public List<string> Tags { get; set; } = [];

    public virtual bool HasChildren => Content != null && Content.Count > 0;
    public virtual bool HasContent => HasChildren || !string.IsNullOrEmpty(Value);
}
