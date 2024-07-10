using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class SubmissionResponsesDto
{
    [Required]
    public List<QuestionWithAnswer> Responses { get; init; } = [];

    public int SubmissionId { get; init; }

    public bool HaveAnyResponses => Responses != null && Responses.Count > 0;
}