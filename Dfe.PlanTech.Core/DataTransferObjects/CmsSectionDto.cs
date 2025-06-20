namespace Dfe.PlanTech.Core.DataTransferObjects
{
    public class CmsSectionDto
    {
        public string Name { get; init; } = null!;

        public CmsPageDto? InterstitialPage { get; init; }

        public List<CmsQuestionDto> Questions { get; init; } = new();

        public required string FirstQuestionSysId { get; init; }
    }
}
