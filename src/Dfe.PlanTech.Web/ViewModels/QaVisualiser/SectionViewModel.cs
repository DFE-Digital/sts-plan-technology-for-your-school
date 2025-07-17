using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.ViewModels.QaVisualiser
{
    public class SectionViewModel
    {
        public SystemDetailsViewModel Sys { get; init; } = null!;
        public string Name { get; init; } = null!;
        public List<QuestionViewModel> Questions { get; init; } = [];

        public SectionViewModel(CmsQuestionnaireSectionDto sectionDto)
        {
            Sys = new SystemDetailsViewModel(sectionDto.Sys);
            Name = sectionDto.Name;
            Questions = sectionDto.Questions.Select(q => new QuestionViewModel(q)).ToList();
        }
    }
}
