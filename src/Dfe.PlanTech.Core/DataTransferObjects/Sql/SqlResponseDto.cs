namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlResponseDto : ISqlDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public SqlUserDto? User { get; set; }
    public int UserEstablishmentId { get; set; }
    public SqlEstablishmentDto? UserEstablishment { get; set; } = null!;
    public int SubmissionId { get; set; }
    public SqlSubmissionDto Submission { get; set; } = null!;
    public int QuestionId { get; set; }
    public SqlQuestionDto Question { get; set; } = null!;
    public int AnswerId { get; set; }
    public SqlAnswerDto Answer { get; set; } = null!;
    public string Maturity { get; set; } = null!;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? DateLastUpdated { get; set; }
}
