using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels.QaVisualiser;

[ExcludeFromCodeCoverage]
public class AnswerViewModel
{
    public SystemDetailsViewModel Sys { get; init; } = null!;
    public QuestionReferenceViewModel? NextQuestion { get; init; }
    public string Text { get; init; } = null!;

    public AnswerViewModel(QuestionnaireAnswerEntry answerDto)
    {
        Sys = new SystemDetailsViewModel(answerDto.Sys!);
        Text = answerDto.Text;

        if (answerDto.NextQuestion is not null)
        {
            NextQuestion = new QuestionReferenceViewModel(answerDto.NextQuestion);
        }
    }
}
