using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

//TODO: merge with SubmissionWithResponses 
public class CheckAnswerDto
{
    [Required]
    public List<QuestionWithAnswer> QuestionAnswerList { get; init; } = new List<QuestionWithAnswer>();

    public int SubmissionId { get; init; }
}