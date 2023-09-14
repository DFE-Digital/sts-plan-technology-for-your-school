namespace Dfe.PlanTech.Domain.Questionnaire.Models
{
    public class TempDataCheckAnswers
    {
        public int SubmissionId { get; init; }

        public string SectionId { get; init; } = null!;

        public string SectionName { get; init; } = null!;

        public string SectionSlug { get; init; } = null!;
    }
}