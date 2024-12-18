﻿using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.Models.Content.Mapped;

[ExcludeFromCodeCoverage]
public class CsContentItem
{
    public string InternalName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Title { get; set; } = null;
    public string? Subtitle { get; set; } = null;
    public bool UseParentHero { get; set; }

}
