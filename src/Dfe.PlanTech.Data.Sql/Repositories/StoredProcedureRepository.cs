using System.Data;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Enums;
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

    // Moved GetSectionStatuses sproc into code (need to remove more sprocs when completed column is removed from db)
    public async Task<List<SectionStatusEntity>> GetSectionStatusesAsync(
        string sectionIds,
        int establishmentId
    )
    {
        var sectionIdList = sectionIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var currentSubmissions = await _db
            .Submissions.Where(s =>
                !s.Deleted
                && s.EstablishmentId == establishmentId
                && sectionIdList.Contains(s.SectionId)
            )
            .GroupBy(s => s.SectionId)
            .Select(g => g.OrderByDescending(s => s.DateCreated).First())
            .ToListAsync();

        var lastCompleteSubmissions = await _db
            .Submissions.Where(s =>
                !s.Deleted
                && s.EstablishmentId == establishmentId
                && sectionIdList.Contains(s.SectionId)
                && (s.Status == SubmissionStatus.CompleteReviewed)
            )
            .GroupBy(s => s.SectionId)
            .Select(g => g.OrderByDescending(s => s.DateCreated).First())
            .ToListAsync();

        var currentBySectionId = currentSubmissions.ToDictionary(s => s.SectionId, s => s);
        var lastCompleteBySectionId = lastCompleteSubmissions.ToDictionary(
            s => s.SectionId,
            s => s
        );

        var result = sectionIdList
            .Select(sectionId =>
            {
                currentBySectionId.TryGetValue(sectionId, out var currentSubmission);
                lastCompleteBySectionId.TryGetValue(sectionId, out var lastCompleteSubmission);

                return new SectionStatusEntity
                {
                    SectionId = sectionId,
                    Status = currentSubmission?.Status ?? SubmissionStatus.NotStarted,
                    DateCreated = currentSubmission?.DateCreated ?? DateTime.UtcNow,
                    DateUpdated =
                        currentSubmission?.DateLastUpdated
                        ?? currentSubmission?.DateCreated
                        ?? DateTime.UtcNow,
                    LastMaturity = lastCompleteSubmission?.Maturity,
                    LastCompletionDate = lastCompleteSubmission?.DateCompleted,
                    Viewed = lastCompleteSubmission?.Viewed,
                };
            })
            .ToList();

        return result;
    }

    public async Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel)
    {
        // Stored procedure defined in:
        // - 2025/20250409_1900_CreateSubmitGroupSelectionProcedure.sql (CREATE)
        // Parameters (in order): @userId INT, @userEstablishmentId INT, @selectedEstablishmentId INT, @selectedEstablishmentName NVARCHAR(MAX), @selectionId INT OUTPUT
        var selectionId = new SqlParameter(DatabaseConstants.SelectionIdParam, SqlDbType.Int)
        {
            Direction = ParameterDirection.Output,
        };

        var parameters = new SqlParameter[]
        {
            new(DatabaseConstants.UserIdParam, userGroupSelectionModel.UserId),
            new(
                DatabaseConstants.EstablishmentIdParam,
                userGroupSelectionModel.UserEstablishmentId
            ),
            new(
                DatabaseConstants.SelectedEstablishmentIdParam,
                userGroupSelectionModel.SelectedEstablishmentId
            ),
            new(
                DatabaseConstants.SelectedEstablishmentNameParam,
                SqlValueOrDbNull(userGroupSelectionModel.SelectedEstablishmentName)
            ),
            selectionId,
        };

        var command = BuildCommand(DatabaseConstants.SpSubmitGroupSelection, parameters);
        await _db.Database.ExecuteSqlRawAsync(command, parameters);

        if (selectionId.Value is int id)
        {
            return id;
        }

        throw new InvalidCastException(
            $"{nameof(selectionId)} is not an integer - value is {selectionId.Value ?? "null"}"
        );
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
            new(DatabaseConstants.SubmissionIdParam, submissionId),
        };

        var command = BuildCommand(DatabaseConstants.SpCalculateMaturity, parameters);
        return _db.Database.ExecuteSqlRawAsync(command, parameters);
    }

    public async Task<int> SubmitResponse(AssessmentResponseModel response)
    {
        // Stored procedure defined in:
        // - 2023/20230915_1737_CreateSubmitAnswerProcedure.sql (CREATE)
        // - 2024/20241009_1100_DboSchemaImprovements.sql (ALTER)
        // - 2025/20251010_0130_AddEstablishmentColumnsToResponses.sql (ALTER - LATEST)
        // Parameters (in order): @sectionId NVARCHAR(50), @sectionName NVARCHAR(50), @questionContentfulId NVARCHAR(50),
        //                         @questionText NVARCHAR(MAX), @answerContentfulId NVARCHAR(50), @answerText NVARCHAR(MAX),
        //                         @userId INT, @userEstablishmentId INT, @establishmentId INT, @maturity NVARCHAR(20),
        //                         @responseId INT OUTPUT, @submissionId INT OUTPUT

        if (response.Answer is null)
        {
            throw new InvalidDataException($"{nameof(response.Answer)} cannot be null");
        }

        var responseId = new SqlParameter(DatabaseConstants.ResponseIdParam, SqlDbType.Int)
        {
            Direction = ParameterDirection.Output,
        };

        var submissionId = new SqlParameter(DatabaseConstants.SubmissionIdParam, SqlDbType.Int)
        {
            Direction = ParameterDirection.Output,
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
            new(DatabaseConstants.UserEstablishmentIdParam, response.UserEstablishmentId),
            new(DatabaseConstants.EstablishmentIdParam, response.EstablishmentId),
            new(DatabaseConstants.MaturityParam, ""),
            responseId,
            submissionId,
        };

        var command = BuildCommand(DatabaseConstants.SpSubmitAnswer, parameters);
        await _db.Database.ExecuteSqlRawAsync(command, parameters);

        if (responseId.Value is int id)
        {
            return id;
        }

        throw new InvalidCastException(
            $"{nameof(responseId)} is not an integer - value is {responseId.Value ?? "null"}"
        );
    }

    public Task SetSubmissionDeletedAsync(int establishmentId, string sectionId)
    {
        // Stored procedure defined in:
        // - 2024/20240524_1635_CreateDeleteCurrentSubmissionProcedure.sql (CREATE)
        // - 2024/20240827_1102_UpdateDeleteCurrentSubmissionProcedure.sql (ALTER)
        // - 2024/20241009_1100_DboSchemaImprovements.sql (ALTER - LATEST)
        // Parameters (in order): @sectionId NVARCHAR(50), @establishmentId INT
        var parameters = new SqlParameter[]
        {
            new(DatabaseConstants.SectionIdParam, sectionId),
            new(DatabaseConstants.EstablishmentIdParam, establishmentId),
        };

        var command = BuildCommand(DatabaseConstants.SpDeleteCurrentSubmission, parameters);
        return _db.Database.ExecuteSqlRawAsync(command, parameters);
    }

    private static string BuildCommand(string storedProcedureName, SqlParameter[] parameters)
    {
        ParameterDirection[] outputParameterTypes =
        {
            ParameterDirection.InputOutput,
            ParameterDirection.Output,
        };

        var parameterNames = parameters.Select(p =>
            outputParameterTypes.Contains(p.Direction)
                ? $"{p.ParameterName} OUTPUT"
                : p.ParameterName
        );

        return $@"EXEC {storedProcedureName} {string.Join(", ", parameterNames)}";
    }

    private static object SqlValueOrDbNull(string? value) => value ?? (object)DBNull.Value;
}
