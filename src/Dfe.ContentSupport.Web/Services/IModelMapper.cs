using Dfe.ContentSupport.Web.Models;
using Dfe.ContentSupport.Web.Models.Mapped;
using Dfe.ContentSupport.Web.Models.Mapped.Custom;
using Dfe.ContentSupport.Web.Models.Mapped.Types;
using Dfe.ContentSupport.Web.ViewModels;

namespace Dfe.ContentSupport.Web.Services;

public interface IModelMapper
{
    List<CsPage> MapToCsPages(IEnumerable<ContentSupportPage> incoming);
    CsContentItem ConvertEntryToContentItem(Entry entry);
    RichTextContentItem? MapContent(ContentItem contentItem);
    RichTextNodeType ConvertToRichTextNodeType(string str);
    CustomComponent? GenerateCustomComponent(Target target);
    List<RichTextContentItem> MapRichTextNodes(List<ContentItem> nodes);
}