using System.Data;
using System.Data.Common;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

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

    public async Task<FirstActivityForEstablishmentRecommendationEntity?> GetFirstActivityForEstablishmentRecommendationAsync(
        int establishmentId,
        string recommendationContentfulReference
    )
    {
        // Stored procedure defined in:
        // - 2026/20260122_1720_GetFirstActivityForEstablishmentRecommendation.sql (CREATE)
        // Parameters (in order): @establishmentId INT, @recommendationContentfulReference NVARCHAR(50)

        var parameters = new SqlParameter[]
        {
            new(DatabaseConstants.EstablishmentIdParam, establishmentId),
            new(
                DatabaseConstants.RecommendationContentfulReferenceParam,
                recommendationContentfulReference
            ),
        };

        var command = BuildCommandString(
            DatabaseConstants.SpGetFirstActivityForEstablishmentRecommendation,
            parameters
        );

        var results = await _db.Set<FirstActivityForEstablishmentRecommendationEntity>()
            .FromSqlRaw(command, parameters)
            .AsNoTracking()
            .ToListAsync();

        if (results.Count == 0)
        {
            return null;
        }

        return results[0];
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

        var command = BuildCommandString(DatabaseConstants.SpCalculateMaturity, parameters);
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

        var command = BuildCommandString(DatabaseConstants.SpSubmitAnswer, parameters);
        await _db.Database.ExecuteSqlRawAsync(command, parameters);

        if (responseId.Value is int id)
        {
            return id;
        }

        throw new InvalidCastException(
            $"{nameof(responseId)} is not an integer - value is {responseId.Value ?? "null"}"
        );
    }

    private static string BuildCommandString(string storedProcedureName, SqlParameter[] parameters)
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
}
