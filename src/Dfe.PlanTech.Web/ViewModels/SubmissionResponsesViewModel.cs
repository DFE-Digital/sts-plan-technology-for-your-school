using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

public class SubmissionResponsesViewModel
{
    [Required]
    public List<SubmissionResponseViewModel>? Responses { get; init; } = [];

    public int? SubmissionId { get; init; }

    public bool HasResponses => Responses != null && Responses.Count > 0;

    public SubmissionResponsesViewModel(SubmissionResponsesModel submission)
    {
        SubmissionId = submission.SubmissionId;
        Responses = submission?.Responses.Select(r => new SubmissionResponseViewModel(r)).ToList();
    }
}
