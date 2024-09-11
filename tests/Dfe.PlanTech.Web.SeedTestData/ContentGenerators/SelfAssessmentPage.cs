using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.Web.SeedTestData.ContentGenerators;

public class SelfAssessmentPage(CmsDbContext db) : ContentGenerator
{
    public override void CreateData()
    {
        db.Pages.Add(CreateComponent(new PageDbEntity
        {
            InternalName = "self-assessment-internal-name",
            Slug = "self-assessment",
            Content = [.. db.Categories],
            Title = CreateComponent(new TitleDbEntity { Text = "Technology self-assessment" }),
            DisplayOrganisationName = true,
        }));
    }
}
