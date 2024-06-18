using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Generators;

public class AnswerGenerator : BaseGenerator<Answer>
{
  public AnswerGenerator()
  {
    RuleFor(answer => answer.Text, faker => faker.Lorem.Sentence(faker.Random.Int(1, 5)));
    RuleFor(answer => answer.Maturity, faker => faker.PickRandom<Maturity>().ToString());
  }

  public static IEnumerable<AnswerDbEntity> MapToDbEntities(IEnumerable<Answer> answers)
  => answers.Select(answer => new AnswerDbEntity()
  {
    Id = answer.Sys.Id,
    Text = answer.Text,
    Maturity = answer.Maturity,
    Published = true,
  });
}