using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;

namespace Dfe.PlanTech.Application.Submissions.Commands;

public class DeleteCurrentSubmissionCommand : IDeleteCurrentSubmissionCommand
{
    private readonly IPlanTechDbContext _db;
    private readonly IUser _user;

    public DeleteCurrentSubmissionCommand(IPlanTechDbContext db, IUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task DeleteCurrentSubmission(ISectionComponent section, CancellationToken cancellationToken = default)
    {
        var establishmentId = await _user.GetEstablishmentId();

        await _db.ExecuteSqlAsync($@"EXEC DeleteCurrentSubmission
                                        @establishmentId={establishmentId},
                                        @sectionId={section.Sys.Id},
                                        @sectionName={section.Name}",
                                        cancellationToken);
    }
}
