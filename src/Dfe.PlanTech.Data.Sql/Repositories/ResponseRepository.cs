using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class ResponseRepository(PlanTechDbContext dbContext) : IResponseRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<int> SubmitResponseAsync(
        int userId,
        int userEstablishmentId,
        int submissionId,
        int questionId,
        int answerId
    )
    {
        // UserActionId is stamped for every IUserActionEntity by PlanTechDbContext.SaveChangesAsync.
        var responseEntity = new ResponseEntity
        {
            UserId = userId,
            UserEstablishmentId = userEstablishmentId,
            SubmissionId = submissionId,
            QuestionId = questionId,
            AnswerId = answerId,
            DateCreated = DateTime.UtcNow,
        };

        await _db.Responses.AddAsync(responseEntity);
        await _db.SaveChangesAsync();

        return responseEntity.Id;
    }
}
