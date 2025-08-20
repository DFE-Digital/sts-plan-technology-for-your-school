using System.Data;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

/*
 * IMPORTANT:
 * Any stored procedure parameters must send the params in the order they're notated in the DB.
 * 
 * For example the SubmitAnswer SP looks like this:
 * ALTER PROCEDURE [dbo].[SubmitAnswer]
    @sectionId NVARCHAR(50),
    @sectionName NVARCHAR(50),
    @questionContentfulId NVARCHAR(50),
    @questionText NVARCHAR(MAX),
    @answerContentfulId NVARCHAR(50),
    @answerText NVARCHAR(MAX),
    @userId INT,
    @establishmentId INT,
    @maturity NVARCHAR(20),
    @responseId INT OUTPUT,
    @submissionId INT OUTPUT
 *
 * As you'll note in SubmitResponse below, the parameters are sent in that order.
 */

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

        var parameters = new SqlParameter[]
        {
            new(DatabaseConstants.UserIdParam, userGroupSelectionModel.UserId),
            new(DatabaseConstants.EstablishmentIdParam, userGroupSelectionModel.UserEstablishmentId),
            new(DatabaseConstants.SelectedEstablishmentIdParam, userGroupSelectionModel.SelectedEstablishmentId),
            new(DatabaseConstants.SelectedEstablishmentNameParam, SqlValueOrDbNull(userGroupSelectionModel.SelectedEstablishmentName)),
            selectionId
        };

        var command = BuildCommand(DatabaseConstants.SpSubmitGroupSelection, parameters);
        await _db.Database.ExecuteSqlRawAsync(command, parameters);

        if (selectionId.Value is int id)
        {
            return id;
        }

        throw new InvalidCastException($"{nameof(selectionId)} is not an integer - value is {selectionId.Value ?? "null"}");
    }

    public Task<int> SetMaturityForSubmissionAsync(int submissionId)
    {
        var parameters = new SqlParameter[]
        {
            new(DatabaseConstants.SubmissionIdParam, submissionId)
        };

        var command = BuildCommand(DatabaseConstants.SpCalculateMaturity, parameters);
        return _db.Database.ExecuteSqlRawAsync(command, parameters);
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

        var parameters = new SqlParameter[]
        {
            new(DatabaseConstants.SectionIdParam, response.SectionId),
            new(DatabaseConstants.SectionNameParam, response.SectionName),
            new(DatabaseConstants.QuestionContentfulIdParam, response.Question.Id),
            new(DatabaseConstants.QuestionTextParam, response.Question.Text),
            new(DatabaseConstants.AnswerContentfulIdParam, response.Answer.Id),
            new(DatabaseConstants.AnswerTextParam, response.Answer.Text),
            new(DatabaseConstants.UserIdParam, response.UserId),
            new(DatabaseConstants.EstablishmentIdParam, response.EstablishmentId),
            new(DatabaseConstants.MaturityParam, response.Maturity),
            responseId,
            submissionId
        };

        var command = BuildCommand(DatabaseConstants.SpSubmitAnswer, parameters);
        await _db.Database.ExecuteSqlRawAsync(command, parameters);

        if (responseId.Value is int id)
        {
            return id;
        }

        throw new InvalidCastException($"{nameof(responseId)} is not an integer - value is {responseId.Value ?? "null"}");
    }

    public Task HardDeleteCurrentSubmissionAsync(int establishmentId, string sectionId)
    {
        var parameters = new SqlParameter[]
        {
            new(DatabaseConstants.EstablishmentIdParam, establishmentId),
            new(DatabaseConstants.SectionIdParam, sectionId)
        };

        var command = BuildCommand(DatabaseConstants.SpDeleteCurrentSubmission, parameters);
        return _db.Database.ExecuteSqlRawAsync(command, parameters);
    }

    private static string BuildCommand(string storedProcedureName, SqlParameter[] parameters)
    {
        ParameterDirection[] outputParameterTypes =
        {
            ParameterDirection.InputOutput,
            ParameterDirection.Output
        };

        var parameterNames = parameters.Select(p =>
            outputParameterTypes.Contains(p.Direction)
                ? $"{p.ParameterName} OUTPUT"
                : p.ParameterName);

        return $@"EXEC {storedProcedureName} {string.Join(", ", parameterNames)}";
    }

    private static object SqlValueOrDbNull(string? value) => value ?? (object)DBNull.Value;
}
