using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;

public class SubmissionResponsesDto
{
    [Required]
    public List<QuestionWithAnswerModel> Responses { get; init; } = [];

    public int SubmissionId { get; init; }

    public bool HaveAnyResponses => Responses != null && Responses.Count > 0;
}
