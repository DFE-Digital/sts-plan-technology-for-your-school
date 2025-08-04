using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Models
{
    public class TextBodyEntry : TransformableEntry<TextBodyEntry, CmsTextBodyDto>
    {
        public RichTextContentEntry RichText { get; init; } = null!;

        protected override Func<TextBodyEntry, CmsTextBodyDto> Constructor => entry => new(entry);
    }
}
