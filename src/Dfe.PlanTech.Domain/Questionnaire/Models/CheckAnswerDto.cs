using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class CheckAnswerDto
{
    [Required]
    public List<QuestionWithAnswer> QuestionAnswerList { get; init; } = new List<QuestionWithAnswer>();
}