using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Web.Models.Inputs;

public class SubmitAnswerInputModel
{
    [Required]
    public string SectionId { get; init; } = null!;

    [Required]
    public string SectionName { get; init; } = null!;

    [Required]
    public string QuestionId { get; init; } = null!;

    [Required]
    public string QuestionText { get; init; } = null!;

    [Required(AllowEmptyStrings = false, ErrorMessage = "You must select an answer to continue")]
    public string ChosenAnswerJson { get; init; } = null!;

    public AnswerViewModel? ChosenAnswer => !string.IsNullOrEmpty(ChosenAnswerJson)
        ? JsonSerializer.Deserialize<AnswerViewModel>(ChosenAnswerJson)
        : null;

    public SubmitAnswerModel ToModel()
    {
        return new SubmitAnswerModel
        {
            SectionId = SectionId,
            SectionName = SectionName,
            QuestionId = QuestionId,
            QuestionText = QuestionText,
            ChosenAnswer = ChosenAnswer?.ToModel()
        };
    }
}
