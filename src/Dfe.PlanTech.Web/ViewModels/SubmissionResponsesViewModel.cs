using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.ViewModels
{
    public class SubmissionResponsesViewModel
    {
        [Required]
        public List<SubmissionResponseViewModel>? Responses { get; init; } = [];

        public int? SubmissionId { get; init; }

        public bool HasResponses => Responses != null && Responses.Count > 0;
    }
}
