using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    internal class RichTextEntry : ContentfulEntry
    {
        public string Value { get; set; } = "";

        public string NodeType { get; set; } = "";

        public List<RichTextMark> Marks { get; set; } = [];

        public List<RichTextEntry> Content { get; set; } = [];

        public RichTextContentSupportData? Data { get; set; }
    }
}
