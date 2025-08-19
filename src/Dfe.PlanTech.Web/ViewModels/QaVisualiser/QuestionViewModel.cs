using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels.QaVisualiser
{
    public class QuestionViewModel
    {
        public SystemDetailsViewModel Sys { get; init; } = null!;
        public List<AnswerViewModel> Answers { get; init; } = [];
        public string Text { get; init; } = null!;

        public QuestionViewModel(QuestionnaireQuestionEntry questionDto)
        {
            Sys = new SystemDetailsViewModel(questionDto.Sys);
            Answers = questionDto.Answers.Select(a => new AnswerViewModel(a)).ToList();
            Text = questionDto.Text;
        }
    }
}
