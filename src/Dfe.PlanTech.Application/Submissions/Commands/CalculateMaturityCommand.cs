using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Microsoft.Data.SqlClient;

namespace Dfe.PlanTech.Application.Submissions.Commands;

public class CalculateMaturityCommand : ICalculateMaturityCommand
{
    private readonly IPlanTechDbContext _db;

    public CalculateMaturityCommand(IPlanTechDbContext db)
    {
        _db = db;
    }

    public async Task<int> CalculateMaturityAsync(int submissionId, CancellationToken cancellationToken = default)
    {
        var sprocName = DatabaseConstants.CalculateMaturitySproc;
        var parms = new List<SqlParameter>
            {
                new() {
                    ParameterName = DatabaseConstants.CalculateMaturitySprocParam,
                    Value = submissionId,
                    Direction = System.Data.ParameterDirection.Input
                }
            };

        return await _db.CallStoredProcedureWithReturnInt(sprocName, parms, cancellationToken);
    }
}
