using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Questions.Models;

namespace Dfe.PlanTech.Domain.Responses.Models;

public class Response
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SubmissionId { get; set; }

    public int QuestionId { get; set; }

    public Question Question { get; set; } = null!;

    public int AnswerId { get; set; }

    public Answer Answer { get; set; } = null!;

    public string Maturity { get; set; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? DateLastUpdated { get; set; }
}