using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Microsoft.Data.SqlClient;

namespace Dfe.PlanTech.Application.Submission.Commands
{
    public class CalculateMaturityCommand : ICalculateMaturityCommand
    {
        private readonly IPlanTechDbContext _db;

        public CalculateMaturityCommand(IPlanTechDbContext db)
        {
            _db = db;
        }

        public async Task<int> CalculateMaturityAsync(int submissionId)
        {
            var sprocName = DatabaseConstants.CalculateMaturitySproc;
            var parms = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = DatabaseConstants.CalculateMaturitySprocParam, Value = submissionId, Direction = System.Data.ParameterDirection.Input }
            };

            var successfullyCalculated = await _db.CallStoredProcedureWithReturnInt(sprocName, parms);
            return successfullyCalculated;
        }
    }
}
