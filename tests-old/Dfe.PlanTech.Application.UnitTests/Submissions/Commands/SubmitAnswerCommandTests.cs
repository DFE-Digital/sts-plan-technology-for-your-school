using System.Security.Authentication;
using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Commands;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Microsoft.Data.SqlClient;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Commands;

public class SubmitAnswerCommandTests
{
    private readonly SubmitAnswerCommand _submitAnswerCommand;
    private readonly IPlanTechDbContext _db;
    private readonly IUser _user;

    private readonly AnswerViewModel _chosenAnswer;
    private readonly SubmitAnswerDto _dto;

    public SubmitAnswerCommandTests()
    {
        _db = Substitute.For<IPlanTechDbContext>();
        _user = Substitute.For<IUser>();

        _submitAnswerCommand = new SubmitAnswerCommand(_db, _user);

        _chosenAnswer = new AnswerViewModel()
        {
            Maturity = "Low",
            Answer = new IdWithText()
            {
                Id = "Id",
                Text = "Text"
            }
        };

        _dto = new SubmitAnswerDto()
        {
            ChosenAnswerJson = JsonSerializer.Serialize(_chosenAnswer)
        };

    }

    [Fact]
    public async Task Should_Throw_Exception_If_Dto_Null()
    {
        await Assert.ThrowsAnyAsync<InvalidDataException>(() => _submitAnswerCommand.SubmitAnswer(null!));
    }


    [Fact]
    public async Task Should_Throw_Exception_If_ChosenAnswer_Null()
    {
        var dto = new SubmitAnswerDto()
        {

        };

        await Assert.ThrowsAnyAsync<InvalidDataException>(() => _submitAnswerCommand.SubmitAnswer(dto));
    }

    [Fact]
    public async Task Should_Throw_Exception_If_UserId_Null()
    {
        _user.GetCurrentUserId().Returns((callinfo) =>
        {
            int? userId = null;
            return Task.FromResult(userId);
        });

        await Assert.ThrowsAnyAsync<AuthenticationException>(() => _submitAnswerCommand.SubmitAnswer(_dto));
    }

    [Fact]
    public async Task Should_Call_ExecuteRaw_With_Params()
    {
        var result = "";
        FormattableString? formattableString = null;
        _db.ExecuteSqlAsync(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>())
            .Returns((callInfo) =>
            {
                formattableString = callInfo.ArgAt<FormattableString>(0);

                SqlParameter? responseParam = formattableString.GetArguments()
                                                  .Select(argument => argument is SqlParameter sqlParameter ? sqlParameter : null)
                                                  .Where(param => param != null && param.ParameterName.Contains("responseId"))
                                                  .First();

                Assert.NotNull(responseParam);
                responseParam.Value = 2;
                result = formattableString.Format;

                return 1;
            });

        _user.GetCurrentUserId().Returns((callinfo) =>
        {
            int? userId = 1;
            return Task.FromResult(userId);
        });

        var id = await _submitAnswerCommand.SubmitAnswer(_dto);

        Assert.NotNull(formattableString);
    }
}
