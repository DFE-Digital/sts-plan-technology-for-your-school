using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Generators;

public class SectionGenerator : BaseGenerator<Section>
{
    protected readonly ReferencedEntityGeneratorHelper<Page> PageGeneratorHelper;
    protected readonly ReferencedEntityGeneratorHelper<Question> QuestionGeneratorHelper;

    public SectionGenerator(List<Question> questions, List<Page> pages)
    {
        QuestionGeneratorHelper = new(questions);
        PageGeneratorHelper = new(pages);

        RuleFor(section => section.Name, faker => faker.Lorem.Sentence(faker.Random.Int(1, 5)));
        RuleFor(section => section.Questions, faker => QuestionGeneratorHelper.GetEntities(faker, 3, 10));
        RuleFor(section => section.InterstitialPage, faker => PageGeneratorHelper.GetEntity(faker));
    }

    public static SectionGenerator CreateInstance(CmsDbContext db)
    {
        var questionGenerator = new QuestionGenerator([]);
        var questions = questionGenerator.Generate(400);

        var dbQuestions = QuestionGenerator.MapToDbEntities(questions);
        db.Questions.AddRange(dbQuestions);
        db.SaveChanges();

        var pageGenerator = new PageGenerator([]);
        var pages = pageGenerator.Generate(200);

        var pageDbEntities = PageGenerator.MapToDbEntities(pages);
        db.Pages.AddRange(pageDbEntities);
        db.SaveChanges();

        return new SectionGenerator(questions, pages);
    }
}