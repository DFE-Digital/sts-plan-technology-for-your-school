namespace Dfe.PlanTech.Domain.Questionnaire.Models
{
    public class TempDataQuestions
    {
        public string QuestionRef { get; init; } = null!;

        public string? AnswerRef { get; init; }

        public int? SubmissionId { get; init; }
    }
}