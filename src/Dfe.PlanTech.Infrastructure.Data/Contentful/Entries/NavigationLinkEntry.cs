namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class NavigationLinkEntry : ContentfulEntry
    {
        public string DisplayText { get; set; } = null!;

        public string? Href { get; set; } = null;

        /// <summary>
        /// Should this link open in a new tab?
        /// </summary>
        public bool OpenInNewTab { get; set; } = false;

        public ContentfulEntry? ContentToLinkTo { get; set; }
    }

}
