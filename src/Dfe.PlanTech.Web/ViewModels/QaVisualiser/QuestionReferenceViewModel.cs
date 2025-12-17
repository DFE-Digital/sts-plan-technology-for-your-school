using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels.QaVisualiser;

[ExcludeFromCodeCoverage]
public class QuestionReferenceViewModel
{
    public SystemDetailsViewModel Sys { get; init; } = null!;

    public QuestionReferenceViewModel(QuestionnaireQuestionEntry questionDto)
    {
        Sys = new SystemDetailsViewModel(questionDto.Sys!);
    }
}
