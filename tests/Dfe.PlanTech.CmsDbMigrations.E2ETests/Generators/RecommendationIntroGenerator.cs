using Bogus;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

public class RecommendationIntroGenerator : BaseGenerator<RecommendationIntro>
{
    protected readonly ReferencedEntityGeneratorHelper<Answer> AnswerGeneratorHelper;
    protected readonly ReferencedEntityGeneratorHelper<ContentComponent> ContentComponentGeneratorHelper;
    protected readonly ReferencedEntityGeneratorHelper<Header> HeaderGeneratorHelper;

    public RecommendationIntroGenerator(List<Answer> answers, List<ContentComponent> contents, List<Header> headers)
    {
        AnswerGeneratorHelper = new(answers);
        ContentComponentGeneratorHelper = new(contents);
        HeaderGeneratorHelper = new(headers);

        RuleFor(recommendationIntro => recommendationIntro.Content, faker => contents.Count > 0 ? ContentComponentGeneratorHelper.GetEntities(faker, 2, 5) : []);
        RuleFor(recommendationIntro => recommendationIntro.Header, faker => headers.Count > 0 ? HeaderGeneratorHelper.GetEntity(faker) : null!);
        RuleFor(recommendationIntro => recommendationIntro.Maturity, faker => faker.PickRandom<Maturity>().ToString());
        RuleFor(recommendationIntro => recommendationIntro.Slug, faker => faker.Lorem.Slug(2));
    }

    public List<RecommendationIntro> GenerateRecommendationIntrosAndSaveToDb(CmsDbContext db, int count)
    {
        var recommendationIntros = Generate(count);
        var recommendationIntroDbEntities = MapToDbEntities(recommendationIntros);
        db.RecommendationIntros.AddRange(recommendationIntroDbEntities);
        db.SaveChanges();

        return recommendationIntros;
    }

    public static IEnumerable<RecommendationIntroDbEntity> MapToDbEntities(IEnumerable<RecommendationIntro> recommendationIntros)
    => recommendationIntros.Select(recommendationIntro => new RecommendationIntroDbEntity()
    {
        Id = recommendationIntro.Sys.Id,
        HeaderId = recommendationIntro.Header.Sys.Id,
        Maturity = recommendationIntro.Maturity,
        Slug = recommendationIntro.Slug,
        Published = true,
        Archived = false,
        Deleted = false,
    });

    public static RecommendationIntroGenerator CreateInstance(CmsDbContext db)
    {
        var answerGenerator = new AnswerGenerator();
        var answers = answerGenerator.Generate(2000);

        db.Answers.AddRange(AnswerGenerator.MapToDbEntities(answers));
        db.SaveChanges();

        var headerGenerator = new HeaderGenerator();
        var headers = headerGenerator.Generate(2000);

        db.Headers.AddRange(HeaderGenerator.MapToDbEntities(headers));
        db.SaveChanges();

        var titles = TitleGenerator.GenerateTitles(db, 2000).Select(title => (ContentComponent)title).ToList();

        return new RecommendationIntroGenerator(answers, titles, headers);
    }
}
