namespace Dfe.PlanTech.Domain.Questionnaire.Models;

using System.ComponentModel.DataAnnotations;

public class CheckAnswerDto
{
    [Required]
    public QuestionWithAnswer[] QuestionAnswerList { get; init; } = Array.Empty<QuestionWithAnswer>();
}