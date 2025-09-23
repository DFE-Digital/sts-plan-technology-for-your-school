using System.Data;
using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
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

public class StoredProcedureRepository : IStoredProcedureRepository
{
    protected readonly PlanTechDbContext _db;

    public StoredProcedureRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public Task<List<SectionStatusEntity>> GetSectionStatusesAsync(string sectionIds, int establishmentId)
    {
        // Stored procedure defined in:
        // - 2023/20230718_168650_GetSectionStatusesSproc.sql (CREATE)
        // - 2023/20230725_166107_ModifyGetSectionStatusesSproc.sql (ALTER)
        // - 2023/20230725_166107_ModifyGetSectionStatusesSprocFix.sql (ALTER)
        // - 2023/20230922_1420_UpdateGetSectionStatusToAcceptEstablishmentId.sql (ALTER)
        // - 2024/20240524_1730_UpdateGetSectionStatusesSproc.sql (ALTER)
        // - 2024/20240605_1030_UpdateGetSectionStatusesSproc.sql (ALTER)
        // - 2024/20240612_1220_CreateGetSectionStatusesForCategory.sql (CREATE related proc)
        // - 2024/20240702_1700_RenameUpdatedSectionStatusesProc.sql (ALTER)
        // - 2024/20240717_1510_AlterSectionStatusesProc.sql (ALTER)
        // - 2024/20241002_0900_CreateDateUpdatedTriggers.sql (ALTER)
        // - 2024/20241104_1200_RemoveContentfulDependencies.sql (ALTER)
        // - 2024/20241127_1700_AddSubmissionViewedColumn.sql (ALTER)
        // - 2024/20241212_1500_DatafixForDateLastUpdated.sql (ALTER)
        // - 2025/20250404_1500_UpdateGetSectionStatusesSproc.sql (ALTER - LATEST)
        // Parameters (in order): @sectionIds NVARCHAR(MAX), @establishmentId INT
        return _db.SectionStatuses
            .FromSqlInterpolated($"EXEC {DatabaseConstants.SpGetSectionStatuses} {sectionIds}, {establishmentId}")
            .ToListAsync();
    }

    public async Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel)
    {
        // Stored procedure defined in:
        // - 2025/20250409_1900_CreateSubmitGroupSelectionProcedure.sql (CREATE)
        // Parameters (in order): @userId INT, @userEstablishmentId INT, @selectedEstablishmentId INT, @selectedEstablishmentName NVARCHAR(MAX), @selectionId INT OUTPUT
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
        // Stored procedure defined in:
        // - 2023/20230712_167773_CalculateMaturitySproc.sql (CREATE)
        // - 2023/20230713_167773_ModifyCalculateMaturitySproc.sql (ALTER)
        // - 2023/20230908_140500_ModifyCalculateMaturitySproc.sql (ALTER - LATEST)
        // Parameters (in order): @submissionId INT
        var parameters = new SqlParameter[]
        {
            new(DatabaseConstants.SubmissionIdParam, submissionId)
        };

        var command = BuildCommand(DatabaseConstants.SpCalculateMaturity, parameters);
        return _db.Database.ExecuteSqlRawAsync(command, parameters);
    }

    public async Task<int> SubmitResponse(AssessmentResponseModel response)
    {
        // Stored procedure defined in:
        // - 2023/20230915_1737_CreateSubmitAnswerProcedure.sql (CREATE)
        // - 2024/20241009_1100_DboSchemaImprovements.sql (ALTER - LATEST)
        // Parameters (in order): @sectionId NVARCHAR(50), @sectionName NVARCHAR(50), @questionContentfulId NVARCHAR(50),
        //                         @questionText NVARCHAR(MAX), @answerContentfulId NVARCHAR(50), @answerText NVARCHAR(MAX),
        //                         @userId INT, @establishmentId INT, @maturity NVARCHAR(20),
        //                         @responseId INT OUTPUT, @submissionId INT OUTPUT
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

    /// <summary>
    /// NOTE: Despite the method name "HardDelete", this method actually performs a SOFT DELETE.
    /// The underlying stored procedure sets the 'deleted' flag to 1 rather than removing the record entirely.
    /// </summary>
    public Task HardDeleteCurrentSubmissionAsync(int establishmentId, string sectionId)
    {
        // Stored procedure defined in:
        // - 2024/20240524_1635_CreateDeleteCurrentSubmissionProcedure.sql (CREATE)
        // - 2024/20240827_1102_UpdateDeleteCurrentSubmissionProcedure.sql (ALTER)
        // - 2024/20241009_1100_DboSchemaImprovements.sql (ALTER - LATEST)
        // Parameters (in order): @sectionId NVARCHAR(50), @establishmentId INT
        var parameters = new SqlParameter[]
        {
            new(DatabaseConstants.SectionIdParam, sectionId),
            new(DatabaseConstants.EstablishmentIdParam, establishmentId)
        };

        // IMPORTANT: This stored procedure (`"[dbo].[DeleteCurrentSubmission]"`) performs a SOFT DELETE (sets deleted = 1), not a hard delete
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
