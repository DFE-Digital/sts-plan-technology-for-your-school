using Bogus;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

public class RecommendationChunkGenerator : BaseGenerator<RecommendationChunk>
{
    protected readonly ReferencedEntityGeneratorHelper<Answer> AnswerGeneratorHelper;
    protected readonly ReferencedEntityGeneratorHelper<ContentComponent> ContentComponentGeneratorHelper;
    protected readonly ReferencedEntityGeneratorHelper<Header> HeaderGeneratorHelper;

    public RecommendationChunkGenerator(List<Answer> answers, List<ContentComponent> contents, List<Header> headers)
    {
        AnswerGeneratorHelper = new(answers);
        ContentComponentGeneratorHelper = new(contents);
        HeaderGeneratorHelper = new(headers);

        RuleFor(recommendationChunk => recommendationChunk.Answers, faker => answers.Count > 0 ? AnswerGeneratorHelper.GetEntities(faker, 2, 5) : []);
        RuleFor(recommendationChunk => recommendationChunk.Content, faker => contents.Count > 0 ? ContentComponentGeneratorHelper.GetEntities(faker, 2, 5) : []);
        RuleFor(recommendationChunk => recommendationChunk.Header, faker => faker.Lorem.Text());
    }

    public List<RecommendationChunk> GenerateRecommendationChunksAndSaveToDb(CmsDbContext db, int count)
    {
        var recommendationChunks = Generate(count);
        var recommendationChunkDbEntities = MapToDbEntities(recommendationChunks);
        db.RecommendationChunks.AddRange(recommendationChunkDbEntities);
        db.SaveChanges();

        return recommendationChunks;
    }

    public static IEnumerable<RecommendationChunkDbEntity> MapToDbEntities(IEnumerable<RecommendationChunk> recommendationChunks)
    => recommendationChunks.Select(recommendationChunk => new RecommendationChunkDbEntity()
    {
        Id = recommendationChunk.Sys.Id,
        Answers = recommendationChunk.Answers.Select(answer => new AnswerDbEntity() { Id = answer.Sys.Id }).ToList(),
        Header = recommendationChunk.Header,
        Published = true,
        Archived = false,
        Deleted = false,
    });

    public static RecommendationChunkGenerator CreateInstance(CmsDbContext db)
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

        return new RecommendationChunkGenerator(answers, titles, headers);
    }
}
