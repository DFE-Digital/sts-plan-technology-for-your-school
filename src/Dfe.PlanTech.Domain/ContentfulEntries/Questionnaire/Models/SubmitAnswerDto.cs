using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;

public class SubmitAnswerDto
{
    [Required]
    public string QuestionId { get; init; } = null!;

    [Required]
    public string QuestionText { get; init; } = null!;

    [Required(AllowEmptyStrings = false, ErrorMessage = "You must select an answer to continue")]
    public string ChosenAnswerJson { get; init; } = null!;

    public AnswerViewModelDto? ChosenAnswer => !string.IsNullOrEmpty(ChosenAnswerJson) ?
                                                    JsonSerializer.Deserialize<AnswerViewModelDto>(ChosenAnswerJson) : null;

    [Required]
    public string SectionId { get; init; } = null!;

    [Required]
    public string SectionName { get; init; } = null!;
}
