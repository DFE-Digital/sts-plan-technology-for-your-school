using System.Data;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Microsoft.Data.SqlClient;

namespace Dfe.PlanTech.Application.Submissions.Commands;

public class DeleteCurrentSubmissionCommand: IDeleteCurrentSubmissionCommand
{
    private readonly IPlanTechDbContext _db;
    private readonly IUser _user;
    
    public DeleteCurrentSubmissionCommand(IPlanTechDbContext db, IUser user)
    {
        _db = db;
        _user = user;
    }
    
    public async Task DeleteCurrentSubmission(Section section, CancellationToken cancellationToken)
    {
        var sectionId = new SqlParameter("@sectionId", section.Sys.Id);
        var sectionName = new SqlParameter("@sectionName", section.Name);
        var establishmentId = new SqlParameter("@establishmentId", await _user.GetEstablishmentId());
        
        await _db.ExecuteRaw($@"EXEC DeleteCurrentSubmission
                                        @establishmentId={establishmentId},
                                        @sectionId={sectionId},
                                        @sectionName={sectionName}",
            cancellationToken);
    }
}
