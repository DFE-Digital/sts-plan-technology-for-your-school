using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.Models.Content;

[ExcludeFromCodeCoverage]
public class Entry : ContentBase
{
    public string JumpIdentifier { get; set; } = null!;
    public ContentItemBase RichText { get; set; } = null!;
    public bool UseParentHero { get; set; }
}
