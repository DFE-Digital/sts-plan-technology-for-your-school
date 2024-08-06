using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Domain.Submissions.Models;

public class Response
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int SubmissionId { get; set; }

    public Submission Submission { get; set; } = null!;

    public int QuestionId { get; set; }

    public ResponseQuestion Question { get; set; } = null!;

    public int AnswerId { get; set; }

    public ResponseAnswer Answer { get; set; } = null!;

    public string Maturity { get; set; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? DateLastUpdated { get; set; }
}
