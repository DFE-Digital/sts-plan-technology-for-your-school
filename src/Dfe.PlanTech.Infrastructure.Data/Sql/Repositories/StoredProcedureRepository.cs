using System.Data;
using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Infrastructure.Data.Sql.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Repositories;

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
            new(DatabaseConstants.SubmissionIdParam, submissionId)
        };

        return _db.Database.ExecuteSqlRawAsync($"EXEC {DatabaseConstants.SpCalculateMaturity}", parameters);
    }

    public Task<List<SectionStatusEntity>> GetSectionStatusesAsync(string sectionIds, int establishmentId)
    {
        return _db.SectionStatuses
            .FromSqlInterpolated($"EXEC {DatabaseConstants.SpGetSectionStatuses} {sectionIds}, {establishmentId}")
            .ToListAsync();
    }

    public async Task<int> RecordGroupSelection(
        int userEstablishmentId,
        int selectedEstablishmentId,
        string? selectedEstablishmentName,
        int userId
    )
    {
        var selectionId = new SqlParameter(DatabaseConstants.SelectionIdParam, SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        var parameters = new List<SqlParameter>
        {
            new(DatabaseConstants.EstablishmentIdParam, userEstablishmentId),
            new(DatabaseConstants.SelectedEstablishmentIdParam, selectedEstablishmentId),
            new(DatabaseConstants.SelectedEstablishmentNameParam, selectedEstablishmentName ?? (object)DBNull.Value),
            new(DatabaseConstants.UserIdParam, userId),
            selectionId
        };

        await _db.Database.ExecuteSqlRawAsync(
            $"EXEC {DatabaseConstants.SpSubmitGroupSelection} {DatabaseConstants.EstablishmentIdParam}, {DatabaseConstants.SelectedEstablishmentIdParam}, {DatabaseConstants.SelectedEstablishmentNameParam}, {DatabaseConstants.UserIdParam}, {DatabaseConstants.SelectionIdParam} OUTPUT",
            parameters);

        return selectionId.Value is int id
            ? id
            : throw new InvalidCastException($"{nameof(selectionId)} is not an integer - value is {selectionId.Value}");
    }
}
