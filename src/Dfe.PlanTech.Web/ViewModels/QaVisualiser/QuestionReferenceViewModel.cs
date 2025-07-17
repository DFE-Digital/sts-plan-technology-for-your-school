using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.ViewModels.QaVisualiser
{
    public class QuestionReferenceViewModel
    {
        public SystemDetailsViewModel Sys { get; init; } = null!;

        public QuestionReferenceViewModel(CmsQuestionnaireQuestionDto questionDto)
        {
            Sys = new SystemDetailsViewModel(questionDto.Sys);
        }
    }
}
