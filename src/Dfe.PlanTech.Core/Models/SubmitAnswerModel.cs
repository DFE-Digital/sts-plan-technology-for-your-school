using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Core.RoutingDataModel;

public class SubmitAnswerModel
{
    public string SectionId { get; init; } = null!;
    public string SectionName { get; init; } = null!;
    public string QuestionId { get; init; } = null!;
    public string QuestionText { get; init; } = null!;
    public string ChosenAnswerJson { get; init; } = null!;
    public AnswerModel? ChosenAnswer { get; init; } = null!;
}
