using Contentful.Core.Models;
using Dfe.ContentSupport.Web.Common;
using Dfe.ContentSupport.Web.Configuration;
using Dfe.ContentSupport.Web.Models;
using Dfe.ContentSupport.Web.Models.Mapped;
using Dfe.ContentSupport.Web.Models.Mapped.Custom;
using Dfe.ContentSupport.Web.Models.Mapped.Standard;
using Dfe.ContentSupport.Web.Models.Mapped.Types;
using Dfe.ContentSupport.Web.ViewModels;
using Hyperlink = Dfe.ContentSupport.Web.Models.Mapped.Standard.Hyperlink;

namespace Dfe.ContentSupport.Web.Services;

public class ModelMapper(SupportedAssetTypes supportedAssetTypes) : IModelMapper
{
    public List<CsPage> MapToCsPages(IEnumerable<ContentSupportPage> incoming)
    {
        return incoming.Select(MapToCsPage).ToList();
    }

    public CsContentItem ConvertEntryToContentItem(Entry entry)
    {
        var item = entry.RichText is not null
            ? MapRichTextContent(entry.RichText, entry)!
            : new CsContentItem
            {
                InternalName = entry.InternalName,
                Slug = entry.Slug,
                Title = entry.Title,
                Subtitle = entry.Subtitle,
                UseParentHero = entry.UseParentHero
            };
        return item;
    }

    public List<RichTextContentItem> MapRichTextNodes(List<ContentItem> nodes)
    {
        return nodes.Select(node => MapContent(node) ?? new RichTextContentItem
        { NodeType = RichTextNodeType.Unknown, InternalName = node.InternalName }).ToList();
    }


    public RichTextContentItem? MapContent(ContentItem contentItem)
    {
        RichTextContentItem? item;
        var nodeType = ConvertToRichTextNodeType(contentItem.NodeType);
        var internalName = contentItem.InternalName;


        switch (nodeType)
        {
            case RichTextNodeType.Text:
                item = new CsText
                {
                    IsBold = contentItem.Marks.Exists(mark => mark.Type == "bold")
                };
                break;
            case RichTextNodeType.Hyperlink:
                var uri = contentItem.Data.Uri.ToString();
                item = new Hyperlink
                {
                    Uri = uri,
                    IsVimeo = uri.Contains("vimeo.com")
                };
                break;
            case RichTextNodeType.EmbeddedAsset:
                var asset = contentItem.Data.Target.Fields;
                item = new EmbeddedAsset
                {
                    AssetContentType = ConvertToAssetContentType(asset.File.ContentType),
                    Description = asset.Description,
                    Title = asset.Title,
                    Uri = asset.File.Url
                };
                break;
            case RichTextNodeType.EmbeddedEntry:
                var target = contentItem.Data.Target;
                internalName = target.InternalName;
                item = new EmbeddedEntry
                {
                    JumpIdentifier = target.JumpIdentifier,
                    RichText = MapRichTextContent(target.RichText, target),
                    CustomComponent = GenerateCustomComponent(target)
                };
                break;
            case RichTextNodeType.Paragraph:
            case RichTextNodeType.UnorderedList:
            case RichTextNodeType.OrderedList:
            case RichTextNodeType.ListItem:
            case RichTextNodeType.Table:
            case RichTextNodeType.TableRow:
            case RichTextNodeType.TableHeaderCell:
            case RichTextNodeType.TableCell:
            case RichTextNodeType.Hr:
            case RichTextNodeType.Heading2:
            case RichTextNodeType.Heading3:
            case RichTextNodeType.Heading4:
            case RichTextNodeType.Heading5:
            case RichTextNodeType.Heading6:
                item = new RichTextContentItem
                {
                    NodeType = nodeType
                };
                break;
            case RichTextNodeType.Document or RichTextNodeType.Unknown:
            default:
                return null;
        }

        item.Content = MapRichTextNodes(contentItem.Content);
        item.Value = contentItem.Value;
        item.InternalName = internalName;
        item.Tags = FlattenMetadata(contentItem.Metadata);
        return item;
    }

    public CustomComponent? GenerateCustomComponent(Target target)
    {
        var contentType = target.SystemProperties.ContentType?.SystemProperties.Id;
        if (contentType is null)
            return null;
        return contentType switch
        {
            "CSAccordion" => GenerateCustomAccordion(target),
            "Attachment" => GenerateCustomAttachment(target),
            "csCard" => GenerateCustomCard(target),
            "GridContainer" => GenerateCustomGridContainer(target),
            _ => null
        };
    }

    public RichTextNodeType ConvertToRichTextNodeType(string str)
    {
        return str switch
        {
            RichTextTags.Document => RichTextNodeType.Document,
            RichTextTags.Paragraph => RichTextNodeType.Paragraph,
            RichTextTags.Heading2 => RichTextNodeType.Heading2,
            RichTextTags.Heading3 => RichTextNodeType.Heading3,
            RichTextTags.Heading4 => RichTextNodeType.Heading4,
            RichTextTags.Heading5 => RichTextNodeType.Heading5,
            RichTextTags.Heading6 => RichTextNodeType.Heading6,
            RichTextTags.UnorderedList => RichTextNodeType.UnorderedList,
            RichTextTags.OrderedList => RichTextNodeType.OrderedList,
            RichTextTags.ListItem => RichTextNodeType.ListItem,
            RichTextTags.Hyperlink => RichTextNodeType.Hyperlink,
            RichTextTags.Table => RichTextNodeType.Table,
            RichTextTags.TableRow => RichTextNodeType.TableRow,
            RichTextTags.TableHeaderCell => RichTextNodeType.TableHeaderCell,
            RichTextTags.TableCell => RichTextNodeType.TableCell,
            RichTextTags.Hr => RichTextNodeType.Hr,
            RichTextTags.EmbeddedAsset => RichTextNodeType.EmbeddedAsset,
            RichTextTags.Text => RichTextNodeType.Text,
            RichTextTags.EmbeddedEntry or RichTextTags.EmbeddedEntryInline => RichTextNodeType
                .EmbeddedEntry,
            _ => RichTextNodeType.Unknown
        };
    }

    public CsPage MapToCsPage(ContentSupportPage incoming)
    {
        var result = new CsPage
        {
            Heading = incoming.Heading,
            Slug = incoming.Slug,
            IsSitemap = incoming.IsSitemap,
            HasCitation = incoming.HasCitation,
            HasBackToTop = incoming.HasBackToTop,
            HasFeedbackBanner = incoming.HasFeedbackBanner,
            HasPrint = incoming.HasPrint,
            Content = MapEntriesToContent(incoming.Content),
            ShowVerticalNavigation = incoming.ShowVerticalNavigation,
            CreatedAt = incoming.SystemProperties.CreatedAt,
            UpdatedAt = incoming.SystemProperties.UpdatedAt,
            Tags = FlattenMetadata(incoming.Metadata)
        };
        return result;
    }

    private static List<string> FlattenMetadata(ContentfulMetadata item)
    {
        if (item is null)
            return [];

        return item.Tags.Select(o => o.Sys.Id).ToList();
    }

    private List<CsContentItem> MapEntriesToContent(List<Entry> entries)
    {
        return entries.Select(ConvertEntryToContentItem).ToList();
    }

    public RichTextContentItem? MapRichTextContent(ContentItemBase? richText, Entry entry)
    {
        if (richText is null)
            return null;
        var item =
            new RichTextContentItem
            {
                InternalName = entry.InternalName,
                Slug = entry.Slug,
                Title = entry.Title,
                Subtitle = entry.Subtitle,
                UseParentHero = entry.UseParentHero,
                NodeType = ConvertToRichTextNodeType(richText.NodeType),
                Content = MapRichTextNodes(richText.Content),
                Tags = FlattenMetadata(entry.Metadata)
            };
        return item;
    }

    private CustomAccordion GenerateCustomAccordion(Target target)
    {
        return new CustomAccordion
        {
            InternalName = target.InternalName,
            Body = MapRichTextContent(target.RichText, target),
            SummaryLine = target.SummaryLine,
            Title = target.Title,
            Accordions = target.Content.Select(GenerateCustomAccordion).ToList()
        };
    }

    private CustomAttachment GenerateCustomAttachment(Target target)
    {
        return new CustomAttachment
        {
            InternalName = target.InternalName,
            ContentType = target.Asset.File.ContentType,
            Size = target.Asset.File.Details.Size,
            Title = target.Title,
            Uri = target.Asset.File.Url,
            UpdatedAt = target.Asset.SystemProperties.UpdatedAt
        };
    }

    private CustomCard GenerateCustomCard(Target target)
    {
        var card = new CustomCard
        {
            InternalName = target.InternalName,
            Title = target.Title,
            Uri = target.Uri,
            Description = target.Description,
            ImageAlt = target.ImageAlt,
            ImageUri = target.Image.Fields.File.Url,
            Meta = target.Meta
        };
        return card;
    }

    private CustomGridContainer GenerateCustomGridContainer(Target target)
    {
        return new CustomGridContainer
        {
            InternalName = target.InternalName,
            Cards = target.Content.Select(GenerateCustomCard).ToList()
        };
    }


    public AssetContentType ConvertToAssetContentType(string str)
    {
        if (supportedAssetTypes.ImageTypes.Contains(str))
            return AssetContentType.Image;
        if (supportedAssetTypes.VideoTypes.Contains(str))
            return AssetContentType.Video;
        return AssetContentType.Unknown;
    }
}
