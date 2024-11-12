namespace Dfe.PlanTech.Web.Models.Content.Mapped
{
    public class PageLink
    {
        public string? Title { get; set; } = null;
        public string? Subtitle { get; set; } = null;
        public required string Url { get; set; }
        public required bool IsActive { get; set; }
    }
}
