using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Infrastructure.Data.Contentful.Entries
{
    public class SectionEntry : ContentfulEntry<CmsSectionDto>
    {
        public string Name { get; init; } = null!;

        public PageEntry? InterstitialPage { get; init; }

        public List<QuestionEntry> Questions { get; init; } = new();

        public string FirstQuestionSysId => Questions
            .Select(question => question.Sys.Id)
            .FirstOrDefault() ?? "";

        protected override CmsSectionDto CreateDto()
        {
            return new CmsSectionDto
            {
                Name = Name,
                InterstitialPage = InterstitialPage?.ToDto(),
                Questions = Questions.Select(q => q.ToDto()).ToList(),
                FirstQuestionSysId = FirstQuestionSysId
            };
        }
    }
}
