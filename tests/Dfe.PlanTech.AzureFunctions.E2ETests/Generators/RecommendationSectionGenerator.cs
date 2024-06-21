using Bogus;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Generators;

public class RecommendationSectionGenerator : BaseGenerator<RecommendationSection>
{
    protected readonly ReferencedEntityGeneratorHelper<Answer> AnswerGeneratorHelper;
    protected readonly ReferencedEntityGeneratorHelper<RecommendationChunk> ChunkGeneratorHelper;

    public RecommendationSectionGenerator(List<Answer> answers, List<RecommendationChunk> recommendationChunks)
    {
        AnswerGeneratorHelper = new(answers);
        ChunkGeneratorHelper = new(recommendationChunks);

        RuleFor(recommendationSection => recommendationSection.Answers, faker => answers.Count > 0 ? AnswerGeneratorHelper.GetEntities(faker, 0, 5) : []);
        RuleFor(recommendationSection => recommendationSection.Chunks, faker => recommendationChunks.Count > 0 ? ChunkGeneratorHelper.GetEntities(faker, 3, 10) : []);
    }

    public static RecommendationSectionGenerator CreateInstance(CmsDbContext db)
    {
        var answerGenerator = new AnswerGenerator();
        var answers = answerGenerator.Generate(500);
        db.Answers.AddRange(AnswerGenerator.MapToDbEntities(answers));
        db.SaveChanges();

        var headerGenerator = new HeaderGenerator();
        var headers = headerGenerator.Generate(600);
        db.Headers.AddRange(HeaderGenerator.MapToDbEntities(headers));

        var chunkGenerator = new RecommendationChunkGenerator([], [], headers);

        var chunks = chunkGenerator.Generate(500);
        db.RecommendationChunks.AddRange(RecommendationChunkGenerator.MapToDbEntities(chunks));
        db.SaveChanges();

        return new RecommendationSectionGenerator(answers, chunks);
    }
}
