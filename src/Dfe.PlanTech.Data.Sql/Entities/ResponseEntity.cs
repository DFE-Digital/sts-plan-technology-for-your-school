using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Entities;

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

    public SqlResponseDto AsDto()
    {
        return new SqlResponseDto
        {
            Id = Id,
            UserId = UserId,
            User = User.AsDto(),
            SubmissionId = SubmissionId,
            Submission = Submission.AsDto(),
            QuestionId = QuestionId,
            Question = Question.AsDto(),
            AnswerId = AnswerId,
            Answer = Answer.AsDto(),
            Maturity = Maturity,
            DateCreated = DateCreated,
            DateLastUpdated = DateLastUpdated
        };
    }
}
