using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

//TODO: merge with CheckAnswerDto 
public class CheckAnswerDto
{
    [Required]
    public List<QuestionWithAnswer> Responses { get; init; } = new List<QuestionWithAnswer>();

    public int SubmissionId { get; init; }
}