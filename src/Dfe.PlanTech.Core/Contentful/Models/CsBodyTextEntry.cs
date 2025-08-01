using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models;

public class CsBodyTextEntry: TransformableEntry<CsBodyTextEntry, CmsCsBodyTextDto>, IContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public RichTextContent RichText { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Subtitle { get; set; } = null!;

    public CsBodyTextEntry() : base(entry => new CmsCsBodyTextDto(entry)) { }
}
