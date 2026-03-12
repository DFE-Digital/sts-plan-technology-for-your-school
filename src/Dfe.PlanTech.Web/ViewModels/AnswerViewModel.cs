using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class AnswerViewModel
{
    public IdWithTextModel Answer { get; set; } = null!;
}
