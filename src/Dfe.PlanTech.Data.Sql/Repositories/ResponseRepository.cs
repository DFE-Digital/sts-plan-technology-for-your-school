using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class ResponseRepository(PlanTechDbContext dbContext) : IResponseRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<int> SubmitResponseAndReturnId(
        int userId,
        int userEstablishmentId,
        int submissionId,
        int questionId,
        int answerId,
        Guid userActionId
    )
    {
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
        return responseEntity.Id;
    }
}
