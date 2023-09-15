using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class SubmitAnswerDto
{
    [Required]
    public string QuestionId { get; init; } = null!;

    [Required]
    public string ChosenAnswerJson { get; init; } = null!;

    public AnswerViewModelDto? ChosenAnswer => !string.IsNullOrEmpty(ChosenAnswerJson) ? 
                                                    JsonSerializer.Deserialize<AnswerViewModelDto>(ChosenAnswerJson) : null;

    public string? Params { get; init; } = null!;

    public int? SubmissionId { get; init; }
}