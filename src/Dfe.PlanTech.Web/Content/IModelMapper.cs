using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Custom;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;

namespace Dfe.PlanTech.Web.Content;

public interface IModelMapper
{
    List<CsPage> MapToCsPages(IEnumerable<ContentSupportPage> incoming);
    CsContentItem ConvertEntryToContentItem(Entry entry);
    RichTextContentItem? MapContent(ContentItem contentItem);
    RichTextNodeType ConvertToRichTextNodeType(string str);
    CustomComponent? GenerateCustomComponent(Target target);
    List<RichTextContentItem> MapRichTextNodes(List<ContentItem> nodes);
}
