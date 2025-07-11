using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.ViewModels
{
    public class SubmissionResponseViewModel
    {
        [Required]
        public string QuestionRef { get; init; } = null!;

        [Required]
        public string? QuestionText { get; init; } = "";

        [Required]
        public string AnswerRef { get; init; } = null!;

        [Required]
        public string? AnswerText { get; init; } = "";

        [Required]
        public DateTime? DateCreated { get; init; } = null!;

        public string? QuestionSlug { get; set; }
    }
}
