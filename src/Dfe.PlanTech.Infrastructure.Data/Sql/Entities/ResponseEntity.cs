using Dfe.PlanTech.Domain.DataTransferObjects;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

public class ResponseEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public UserEntity User { get; set; } = null!;

    public int SubmissionId { get; set; }

    public SubmissionEntity Submission { get; set; } = null!;

    public int QuestionId { get; set; }

    public QuestionEntity Question { get; set; } = null!;

    public int AnswerId { get; set; }

    public AnswerEntity Answer { get; set; } = null!;

    public string Maturity { get; set; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? DateLastUpdated { get; set; }

    public ResponseDto ToDto()
    {
        return new ResponseDto
        {
            Id = Id,
            UserId = UserId,
            User = User.ToDto(),
            SubmissionId = SubmissionId,
            Submission = Submission.ToDto(),
            QuestionId = QuestionId,
            Question = Question.ToDto(),
            AnswerId = AnswerId,
            Answer = Answer.ToDto(),
            Maturity = Maturity,
            DateCreated = DateCreated,
            DateLastUpdated = DateLastUpdated
        };
    }
}
