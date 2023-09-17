using System.Security.Authentication;
using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Commands;
using Dfe.PlanTech.Application.Submissions.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Commands;

public class SubmitAnswerCommandTests
{
  private readonly ISubmitAnswerCommand _submitAnswerCommand;
  private readonly IPlanTechDbContext _db;
  private readonly IUser _user;

  public SubmitAnswerCommandTests()
  {
    _db = Substitute.For<IPlanTechDbContext>();
    _user = Substitute.For<IUser>();

    _submitAnswerCommand = new SubmitAnswerCommand(_db, _user);
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
    var chosenAnswer = new AnswerViewModelDto()
    {
      Maturity = "Low",
      Answer = new IdWithText()
      {
        Id = "Id",
        Text = "Text"
      },
      NextQuestion = new IdAndSlugAndText()
      {
        Id = "QId",
        Text = "QText",
        Slug = "QSlug"
      }
    };

    var dto = new SubmitAnswerDto()
    {
      ChosenAnswerJson = JsonSerializer.Serialize(chosenAnswer)
    };

    _user.GetCurrentUserId().Returns((callinfo) =>
    {
      int? userId = null;
      return Task.FromResult(userId);
    });

    await Assert.ThrowsAnyAsync<AuthenticationException>(() => _submitAnswerCommand.SubmitAnswer(dto));
  }
}