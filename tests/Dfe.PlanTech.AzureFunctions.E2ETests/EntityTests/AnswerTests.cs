using Dfe.PlanTech.AzureFunctions.E2ETests.DataGenerators;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

public class AnswerTests
{
  private readonly AnswerGenerator _answerGenerator = new();

  private readonly CmsDbContext _dbContext;
  public AnswerTests()
  {
    var serviceProvider = Startup.BuildServiceProvider();
    _dbContext = serviceProvider.GetRequiredService<CmsDbContext>();
  }

  [Fact]
  public async Task Should_Populate_Db()
  {
    var answers = _answerGenerator.Generate(20);

    _dbContext.Answers.AddRange(answers);
    var result = await _dbContext.SaveChangesAsync();

    Assert.Equal(40, result);
  }
}