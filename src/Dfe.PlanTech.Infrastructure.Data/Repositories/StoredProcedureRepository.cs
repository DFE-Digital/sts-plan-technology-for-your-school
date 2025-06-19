using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Infrastructure.Data.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data.Repositories;

public class StoredProcedureRepository
{
    protected readonly PlanTechDbContext _db;

    public StoredProcedureRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public Task<int> CalculateMaturityForSubmissionsAsync(int submissionId)
    {
        var parameters = new List<SqlParameter>
        {
            new() {
                Direction = System.Data.ParameterDirection.Input,
                ParameterName = DatabaseConstants.SubmissionIdParam,
                Value = submissionId,
            }
        };

        return _db.Database.ExecuteSqlRawAsync(DatabaseConstants.StoredProcedures.CalculateMaturity, parameters);
    }

    public Task<List<SectionStatusEntity>> GetSectionStatusesAsync(string sectionIds, int establishmentId)
    {
        FormattableString sql = $"{DatabaseConstants.GetSectionStatuses} {sectionIds}, {establishmentId}";
        return _db.SectionStatuses.FromSqlInterpolated(sql).ToListAsync();
    }

    public Task<int> ExecuteSqlAsync(FormattableString sql)
    {
        return _db.Database.ExecuteSqlAsync(sql);
    }
}
