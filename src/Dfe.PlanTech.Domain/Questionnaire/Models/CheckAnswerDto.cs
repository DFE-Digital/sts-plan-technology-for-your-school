namespace Dfe.PlanTech.Domain.Questionnaire.Models;

using System.ComponentModel.DataAnnotations;

public class CheckAnswerDto
{
    [Required]
    public List<QuestionWithAnswer> QuestionAnswerList { get; init; } = new List<QuestionWithAnswer>();
}