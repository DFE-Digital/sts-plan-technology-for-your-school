using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsRichTextContentDto : CmsEntryDto
    {
        public string Value { get; set; } = "";
        public string NodeType { get; set; } = "";
        public List<CmsRichTextMarkDto> Marks { get; set; } = [];
        public List<CmsRichTextContentDto> Content { get; set; } = [];
        public CmsRichTextContentSupportDataDto? Data { get; set; }

        public CmsRichTextContentDto(RichTextContent richTextContent)
        {
            Value = richTextContent.Value;
            NodeType = richTextContent.NodeType;
            Marks = richTextContent.Marks.Select(m => m.AsDto()).ToList();
            Content = richTextContent.Content.Select(c => c.AsDto()).ToList();
        }
    }
}
