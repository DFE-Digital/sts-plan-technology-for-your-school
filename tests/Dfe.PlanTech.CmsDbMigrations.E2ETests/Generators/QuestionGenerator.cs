using Bogus;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

public class QuestionGenerator : BaseGenerator<Question>
{
    protected readonly ReferencedEntityGeneratorHelper<Answer> AnswerGeneratorHelper;

    public QuestionGenerator(List<Answer> answers)
    {
        AnswerGeneratorHelper = new(answers);

        RuleFor(question => question.Text, faker => faker.Lorem.Sentence(faker.Random.Int(1, 5)));
        RuleFor(question => question.HelpText, faker => faker.Lorem.Sentence(faker.Random.Int(5, 20)).OrNull(faker, 0.85f));
        RuleFor(question => question.Slug, faker => faker.Lorem.Slug(2));

        RuleFor(question => question.Answers, faker => answers.Count > 0 ? AnswerGeneratorHelper.GetEntities(faker, 2, 7) : []);
    }

    public List<Question> GenerateQuestionsAndSaveToDb(CmsDbContext db, int count)
    {
        var questions = Generate(count);
        var questionDbEntities = MapToDbEntities(questions);
        db.Questions.AddRange(questionDbEntities);
        db.SaveChanges();

        return questions;
    }

    public static IEnumerable<QuestionDbEntity> MapToDbEntities(IEnumerable<Question> questions)
    => questions.Select(question => new QuestionDbEntity()
    {
        Id = question.Sys.Id,
        Text = question.Text,
        HelpText = question.HelpText,
        Slug = question.Slug,
        Answers = [],
        Published = true,
        Archived = false,
        Deleted = false,
    });

    public static QuestionGenerator CreateInstance(CmsDbContext db)
    {
        var answerGenerator = new AnswerGenerator();
        var answers = answerGenerator.Generate(2000);

        db.Answers.AddRange(AnswerGenerator.MapToDbEntities(answers));
        db.SaveChanges();

        return new QuestionGenerator(answers);
    }
}
