﻿using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class StatusLinkViewModel
{
    public string CategorySlug { get; set; } = null!;
    public string Context { get; set; } = null!;
    public CategoryLandingSectionViewModel Section { get; set; } = null!;
}
