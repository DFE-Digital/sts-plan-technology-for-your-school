using System.Data;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
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
}
