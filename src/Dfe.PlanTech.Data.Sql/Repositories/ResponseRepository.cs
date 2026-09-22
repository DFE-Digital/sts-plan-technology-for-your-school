using Dfe.PlanTech.Core.Providers.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class ResponseRepository(
    PlanTechDbContext dbContext,
    IUserActionIdProvider userActionIdProvider
) : IResponseRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    private readonly IUserActionIdProvider _userActionIdProvider =
        userActionIdProvider ?? throw new ArgumentNullException(nameof(userActionIdProvider));

    public async Task<int> SubmitResponseAsync(
        int userId,
        int userEstablishmentId,
        int submissionId,
        int questionId,
        int answerId
    )
    {
        var userActionId = _userActionIdProvider.GetUserActionId();

        var responseEntity = new ResponseEntity
        {
            UserId = userId,
            UserEstablishmentId = userEstablishmentId,
            SubmissionId = submissionId,
            QuestionId = questionId,
            AnswerId = answerId,
            DateCreated = DateTime.UtcNow,
            UserActionId = userActionId,
        };

        await _db.Responses.AddAsync(responseEntity);
        return responseEntity.Id;
    }
}
