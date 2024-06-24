using Dfe.PlanTech.AzureFunctions.E2ETests.Utilities;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Generators;

public class SubtopicRecommendationGenerator : BaseGenerator<SubtopicRecommendation>
{
    protected readonly ReferencedEntityGeneratorHelper<Section> SubtopicGeneratorHelper;
    protected readonly ReferencedEntityGeneratorHelper<RecommendationIntro> IntroGeneratorHelper;
    protected readonly ReferencedEntityGeneratorHelper<RecommendationSection> RecommendationSectionGeneratorHelper;

    public SubtopicRecommendationGenerator(List<RecommendationIntro> recommendationIntros, List<Section> subtopics, List<RecommendationSection> recommendationSections)
    {
        SubtopicGeneratorHelper = new(subtopics);
        IntroGeneratorHelper = new(recommendationIntros);
        RecommendationSectionGeneratorHelper = new(recommendationSections);

        RuleFor(recommendationSection => recommendationSection.Intros, faker => recommendationIntros.Count > 0 ? IntroGeneratorHelper.GetEntities(faker, 1, 3) : []);
        RuleFor(recommendationSection => recommendationSection.Section, faker => RecommendationSectionGeneratorHelper.GetEntity(faker));
        RuleFor(recommendationSection => recommendationSection.Subtopic, faker => SubtopicGeneratorHelper.GetEntity(faker));

    }

    public static SubtopicRecommendationGenerator CreateInstance(CmsDbContext db)
    {
        var introGenerator = RecommendationIntroGenerator.CreateInstance(db);
        var intros = introGenerator.Generate(200);
        var introDbEntities = RecommendationIntroGenerator.MapToDbEntities(intros);

        db.RecommendationIntros.AddRange(introDbEntities);
        db.SaveChanges();

        var subtopicGenerator = SectionGenerator.CreateInstance(db);
        var subtopics = subtopicGenerator.Generate(200);
        var subtopicDbEntities = subtopics.Select(subtopic => new SectionDbEntity()
        {
            Id = subtopic.Sys.Id,
            InterstitialPageId = subtopic.InterstitialPage.Sys.Id,
            Name = subtopic.Name,
        });
        db.Sections.AddRange(subtopicDbEntities);
        db.SaveChanges();

        var sectionGenerator = RecommendationSectionGenerator.CreateInstance(db);
        var recommendationSections = sectionGenerator.Generate(100);
        var recommendationSectionDbEntities = recommendationSections.Select(recSec => new RecommendationSectionDbEntity()
        {
            Id = recSec.Sys.Id,
            Answers = [],
            Chunks = [],
        });
        db.RecommendationSections.AddRange(recommendationSectionDbEntities);
        db.SaveChanges();

        return new SubtopicRecommendationGenerator(intros, subtopics, recommendationSections);
    }
}
