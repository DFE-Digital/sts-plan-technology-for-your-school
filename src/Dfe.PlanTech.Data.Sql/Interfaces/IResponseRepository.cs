namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IResponseRepository
{
    Task<int> SubmitResponseAsync(
        int userId,
        int userEstablishmentId,
        int submissionId,
        int questionId,
        int answerId
    );
}
