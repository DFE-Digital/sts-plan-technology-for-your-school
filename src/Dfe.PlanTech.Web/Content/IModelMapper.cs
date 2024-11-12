using Dfe.PlanTech.Web.Models.Content;
using Dfe.PlanTech.Web.Models.Content.Mapped;
using Dfe.PlanTech.Web.Models.Content.Mapped.Custom;
using Dfe.PlanTech.Web.Models.Content.Mapped.Types;

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
