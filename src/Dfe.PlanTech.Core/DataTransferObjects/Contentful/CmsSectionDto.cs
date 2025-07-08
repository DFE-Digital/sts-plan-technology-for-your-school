namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful
{
    public class CmsSectionDto : CmsEntryDto
    {
        public string Name { get; set; } = null!;

        public CmsPageDto? InterstitialPage { get; set; }

        public List<CmsQuestionDto> Questions { get; set; } = [];

        public string FirstQuestionSysId { get; set; }
    }
}
