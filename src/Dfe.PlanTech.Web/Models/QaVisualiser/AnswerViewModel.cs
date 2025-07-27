using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.ViewModels.QaVisualiser
{
    public class AnswerViewModel
    {
        public SystemDetailsViewModel Sys { get; init; } = null!;
        public QuestionReferenceViewModel? NextQuestion { get; init; }
        public string Text { get; init; } = null!;

        public AnswerViewModel(CmsQuestionnaireAnswerDto answerDto)
        {
            Sys = new SystemDetailsViewModel(answerDto.Sys);

            if (answerDto.NextQuestion is not null)
            {
                NextQuestion = new QuestionReferenceViewModel(answerDto.NextQuestion);
            }
        }
    }
}
