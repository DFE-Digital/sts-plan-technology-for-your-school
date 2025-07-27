using System.Data;
using Contentful.Core.Models.Management;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.RoutingDataModel;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class StoredProcedureRepository
{
    protected readonly PlanTechDbContext _db;

    public StoredProcedureRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public Task<List<SectionStatusEntity>> GetSectionStatusesAsync(string sectionIds, int establishmentId)
    {
        return _db.SectionStatuses
            .FromSqlInterpolated($"EXEC {DatabaseConstants.SpGetSectionStatuses} {sectionIds}, {establishmentId}")
            .ToListAsync();
    }

    public async Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel)
    {
        var selectionId = new SqlParameter(DatabaseConstants.SelectionIdParam, SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        var parameters = new List<SqlParameter>
        {
            new(DatabaseConstants.EstablishmentIdParam, userGroupSelectionModel.UserEstablishmentId),
            new(DatabaseConstants.SelectedEstablishmentIdParam, userGroupSelectionModel.SelectedEstablishmentId),
            new(DatabaseConstants.SelectedEstablishmentNameParam, SqlValueOrDbNull(userGroupSelectionModel.SelectedEstablishmentName)),
            new(DatabaseConstants.UserIdParam, userGroupSelectionModel.UserId),
            selectionId
        };

        await _db.Database.ExecuteSqlRawAsync($"EXEC {DatabaseConstants.SpSubmitGroupSelection}", parameters);

        return (int)(selectionId.Value
        ?? throw new InvalidCastException($"{nameof(selectionId)} is not an integer - value is {selectionId.Value}"));
    }

    public Task<int> SetMaturityForSubmissionAsync(int submissionId)
    {
        var parameters = new List<SqlParameter>
        {
            new(DatabaseConstants.SubmissionIdParam, submissionId)
        };

        return _db.Database.ExecuteSqlRawAsync($"EXEC {DatabaseConstants.SpCalculateMaturity}", parameters);
    }

    public async Task<int> SubmitResponse(AssessmentResponseModel response)
    {
        var responseId = new SqlParameter(DatabaseConstants.ResponseIdParam, SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        var submissionId = new SqlParameter(DatabaseConstants.SubmissionIdParam, SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        var parameters = new List<SqlParameter>
        {
            new(DatabaseConstants.EstablishmentIdParam, response.EstablishmentId),
            new(DatabaseConstants.UserIdParam, response.UserId),
            new(DatabaseConstants.SectionIdParam, response.SectionId),
            new(DatabaseConstants.SectionNameParam, response.SectionName),
            new(DatabaseConstants.QuestionContentfulIdParam, response.Question.Id),
            new(DatabaseConstants.QuestionTextParam, response.Question.Text),
            new(DatabaseConstants.AnswerContentfulIdParam, response.Answer.Id),
            new(DatabaseConstants.AnswerTextParam, response.Answer.Text),
            new(DatabaseConstants.MaturityParam, response.Maturity),
            responseId,
            submissionId
        };

        await _db.Database.ExecuteSqlRawAsync($@"EXEC {DatabaseConstants.SpSubmitAnswer}", parameters);

        return (int)(responseId.Value
            ?? throw new InvalidCastException($"{nameof(responseId)} is not an integer - value is {responseId.Value}"));
    }

    public Task HardDeleteCurrentSubmissionAsync(int establishmentId, string sectionId)
    {
        var parameters = new List<SqlParameter>
        {
            new(DatabaseConstants.EstablishmentIdParam, establishmentId),
            new(DatabaseConstants.SectionIdParam, sectionId)
        };

        return _db.Database.ExecuteSqlRawAsync($@"EXEC {DatabaseConstants.SpDeleteCurrentSubmission}", parameters);
    }

    private static object SqlValueOrDbNull(string? value) => value ?? (object)DBNull.Value;
}
