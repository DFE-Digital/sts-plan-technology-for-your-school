//using Contentful.Core.Models;
//using Dfe.PlanTech.Application.Constants;
//using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
//using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
//using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Custom;
//using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Standard;
//using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Types;
//using Dfe.PlanTech.Web.Configuration;
//using Hyperlink = Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Standard.Hyperlink;

//namespace Dfe.PlanTech.Web.Content;

//public class ModelMapper(SupportedAssetTypes supportedAssetTypes) : IModelMapper
//{



//    //public RichTextContentItem? MapContent(ContentItem contentItem)
//    //{
//    //    RichTextContentItem? item;
//    //    var nodeType = ConvertToRichTextNodeType(contentItem.NodeType);
//    //    var internalName = contentItem.InternalName;


//    //    switch (nodeType)
//    //    {
//    //        case RichTextNodeType.Hyperlink:
//    //            var uri = contentItem.Data.Uri.ToString();
//    //            item = new Hyperlink
//    //            {
//    //                Uri = uri,
//    //                IsVimeo = uri.Contains("vimeo.com")
//    //            };
//    //            break;
//    //        case RichTextNodeType.EmbeddedAsset:
//    //            var asset = contentItem.Data.Target.Fields;
//    //            item = new EmbeddedAsset
//    //            {
//    //                AssetContentType = ConvertToAssetContentType(asset.File.ContentType),
//    //                Description = asset.Description,
//    //                Title = asset.Title,
//    //                Uri = asset.File.Url
//    //            };
//    //            break;
//    //        case RichTextNodeType.Paragraph:
//    //        case RichTextNodeType.UnorderedList:
//    //        case RichTextNodeType.OrderedList:
//    //        case RichTextNodeType.ListItem:
//    //        case RichTextNodeType.Table:
//    //        case RichTextNodeType.TableRow:
//    //        case RichTextNodeType.TableHeaderCell:
//    //        case RichTextNodeType.TableCell:
//    //        case RichTextNodeType.Hr:
//    //        case RichTextNodeType.Heading2:
//    //        case RichTextNodeType.Heading3:
//    //        case RichTextNodeType.Heading4:
//    //        case RichTextNodeType.Heading5:
//    //        case RichTextNodeType.Heading6:
//    //            item = new RichTextContentItem
//    //            {
//    //                NodeType = nodeType
//    //            };
//    //            break;
//    //        case RichTextNodeType.Document or RichTextNodeType.Unknown:
//    //        default:
//    //            return null;
//    //    }

//    //    item.Content = MapRichTextNodes(contentItem.Content);
//    //    item.Value = contentItem.Value;
//    //    item.InternalName = internalName;
//    //    item.Tags = FlattenMetadata(contentItem.Metadata);
//    //    return item;
//    //}
//}
