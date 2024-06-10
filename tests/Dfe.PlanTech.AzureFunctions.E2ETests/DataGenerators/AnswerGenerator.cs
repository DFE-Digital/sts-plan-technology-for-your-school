using Bogus;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.DataGenerators;

public class AnswerGenerator : Faker<AnswerDbEntity>
{
  public AnswerGenerator()
  {
    this.AddCommonFakes();

    RuleFor(a => a.BeforeTitleContentPages, []);
    RuleFor(a => a.Maturity, faker => faker.PickRandom<Maturity>().ToString());
    RuleFor(a => a.Order, faker => faker.Random.Int(0, 10));
    RuleFor(a => a.Text, faker => faker.Lorem.Sentence(faker.Random.Int(1, 5)));
  }
}