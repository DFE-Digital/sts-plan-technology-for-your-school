namespace Dfe.PlanTech.Domain.DataTransferObjects;

public class ResponseDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public UserDto User { get; set; } = null!;

    public int SubmissionId { get; set; }

    public SubmissionDto Submission { get; set; } = null!;

    public int QuestionId { get; set; }

    public QuestionDto Question { get; set; } = null!;

    public int AnswerId { get; set; }

    public AnswerDto Answer { get; set; } = null!;

    public string Maturity { get; set; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? DateLastUpdated { get; set; }
}
