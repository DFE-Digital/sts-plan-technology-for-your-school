using System.Data;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

public class CategoryGenerator : BaseGenerator<Category>
{
    private readonly ReferencedEntityGeneratorHelper<Header> HeaderGeneratorHelper;
    private readonly ReferencedEntityGeneratorHelper<Section> SectionGeneratorHelper;

    public CategoryGenerator(List<Header> headers, List<Section> sections)
    {
        HeaderGeneratorHelper = new(headers);
        SectionGeneratorHelper = new(sections);

        RuleFor(cat => cat.InternalName, faker => faker.Lorem.Sentence(faker.Random.Int(2, 5)));
        RuleFor(cat => cat.Header, faker => HeaderGeneratorHelper.GetEntity(faker));
        RuleFor(cat => cat.Sections, faker => SectionGeneratorHelper.GetEntities(faker, 3, 5));
    }
}
