using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

public class ResponseEntity : SqlEntity<SqlResponseDto>
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

    protected override SqlResponseDto CreateDto()
    {
        return new SqlResponseDto
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
