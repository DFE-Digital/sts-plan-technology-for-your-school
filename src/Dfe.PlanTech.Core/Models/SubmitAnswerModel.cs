namespace Dfe.PlanTech.Core.Models;

public class SubmitAnswerModel
{
    public string SectionId { get; init; } = null!;
    public string SectionName { get; init; } = null!;
    public IdWithTextModel Question { get; init; } = null!;
    public string ChosenAnswerJson { get; init; } = null!;
    public IdWithTextModel? ChosenAnswer { get; init; } = null!;
}
